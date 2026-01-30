using Cadmus.Api.Services;
using Cadmus.Api.Services.Seeding;
using Cadmus.Core;
using Fusi.Api.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using Cadmus.Core.Config;
using Cadmus.Seed;
using Fusi.Api.Auth.Services;
using System.Text.Json;
using Serilog.Events;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Cadmus.Api.Controllers;
using Cadmus.Api.Controllers.Import;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Cadmus.Api.Config.Services;
using Cadmus.Api.Config;
using CadmusTesApi.Services;

namespace CadmusTesApi;

/// <summary>
/// Program.
/// </summary>
public static class Program
{
    // startup log file name, Serilog is configured later via appsettings.json
    private const string STARTUP_LOG_NAME = "startup.log";

    private static void ConfigureAppServices(IServiceCollection services,
        IConfiguration config)
    {
        // Cadmus repository
        string dataCS = string.Format(
        config.GetConnectionString("Default")!,
            config.GetValue<string>("DatabaseNames:Data"));
        services.AddSingleton<IRepositoryProvider>(
            _ => new AppRepositoryProvider { ConnectionString = dataCS });

        // part seeder factory provider
        services.AddSingleton<IPartSeederFactoryProvider,
            AppPartSeederFactoryProvider>();

        // item browser factory provider
        services.AddSingleton<IItemBrowserFactoryProvider>(_ =>
        new StandardItemBrowserFactoryProvider(
                config.GetConnectionString("Default")!));

        // index and graph
        ServiceConfigurator.ConfigureIndexServices(services, config);
        ServiceConfigurator.ConfigureGraphServices(services, config);

        // previewer
        services.AddSingleton(p => ServiceConfigurator.GetPreviewer(p, config));
    }

    /// <summary>
    /// Configures the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public static void ConfigureServices(IServiceCollection services,
        IConfiguration config, IHostEnvironment hostEnvironment)
    {
        // configuration
        services.AddSingleton(_ => config);
        ServiceConfigurator.ConfigureOptionsServices(services, config);

        // security
        ServiceConfigurator.ConfigureCorsServices(services, config);
        ServiceConfigurator.ConfigureRateLimiterService(services, config, hostEnvironment);
        ServiceConfigurator.ConfigureAuthServices(services, config);

        // proxy
        services.AddHttpClient();
        services.AddResponseCaching();

        // API controllers
        services.AddControllers();
        // camel-case JSON in response
        services.AddMvc()
            // https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio#jsonnet-support
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase;
            });

        // framework services
        // IMemoryCache: https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory
        services.AddMemoryCache();

        // user repository service
        services.AddScoped<IUserRepository<NamedUser>,
            UserRepository<NamedUser, IdentityRole>>();

        // messaging
        ServiceConfigurator.ConfigureMessagingServices(services);

        // logging
        ServiceConfigurator.ConfigureLogging(services);

        // app services
        ConfigureAppServices(services, config);
    }

    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static async Task<int> Main(string[] args)
    {
        // early startup logging to ensure we catch any exceptions
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
#if DEBUG
            .WriteTo.File(STARTUP_LOG_NAME, rollingInterval: RollingInterval.Day)
#endif
            .CreateLogger();

        try
        {
            Log.Information("Starting Cadmus API host");
            ServiceConfigurator.DumpEnvironmentVars();

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            ServiceConfigurator.ConfigureLogger(builder);

            IConfiguration config = new ConfigurationService(builder.Environment)
                .Configuration;

            ServiceConfigurator.ConfigureServices(builder.Services, config,
                builder.Environment);
            ConfigureAppServices(builder.Services, config);

            builder.Services.AddOpenApi();

            // controllers from Cadmus.Api.Controllers
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(ItemController).Assembly)
                .AddApplicationPart(typeof(ThesaurusImportController).Assembly)
                .AddControllersAsServices();

            WebApplication app = builder.Build();

            // forward headers for use with an eventual reverse proxy
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor
                    | ForwardedHeaders.XForwardedProto
            });

            // development or production
            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio
                app.UseExceptionHandler("/Error");
                if (config.GetValue<bool>("Server:UseHSTS"))
                {
                    Console.WriteLine("HSTS: yes");
                    app.UseHsts();
                }
                else
                {
                    Console.WriteLine("HSTS: no");
                }
            }

            // HTTPS redirection
            if (config.GetValue<bool>("Server:UseHttpsRedirection"))
            {
                Console.WriteLine("HttpsRedirection: yes");
                app.UseHttpsRedirection();
            }
            else
            {
                Console.WriteLine("HttpsRedirection: no");
            }

            // CORS
            app.UseCors("CorsPolicy");
            // rate limiter
            if (!config.GetValue<bool>("RateLimit:IsDisabled"))
                app.UseRateLimiter();
            // authentication
            app.UseAuthentication();
            app.UseAuthorization();
            // proxy
            app.UseResponseCaching();

            // seed auth database (via Services/HostAuthSeedExtensions)
            await app.SeedAuthAsync();

            // seed Cadmus database (via Services/HostSeedExtension)
            await app.SeedAsync();

            // map controllers and Scalar API
            app.MapControllers();
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options.WithTitle("Cadmus TES API")
                       .AddPreferredSecuritySchemes("Bearer");
            });

            Log.Information("Running API");
            await app.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Cadmus API host terminated unexpectedly");
            Debug.WriteLine(ex.ToString());
            Console.WriteLine(ex.ToString());
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
