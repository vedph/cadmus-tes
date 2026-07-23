using Fusi.Cli.Logging;
using Serilog;
using Serilog.Events;
using Spectre.Console.Cli;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tes.Cli.Commands;

namespace Tes.Cli;

/// <summary>
/// Main program.
/// </summary>
public static class Program
{
#if DEBUG
    private static void DeleteLogs()
    {
        foreach (var path in Directory.EnumerateFiles(
            AppDomain.CurrentDomain.BaseDirectory, "tes-log*.txt"))
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
#endif

    /// <summary>
    /// Entry point.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static async Task<int> Main(string[] args)
    {
        LogService? logService = null;

        try
        {
#if DEBUG
            DeleteLogs();
#endif
            Console.OutputEncoding = Encoding.UTF8;

            // create and configure the LogService instance from the reusable library.
            LogServiceOptions options = new()
            {
                FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "tes-log.txt"),
                FileRollingInterval = RollingInterval.Day,
                RetainedFileCountLimit = 7,
#if DEBUG
                MinimumLevel = LogEventLevel.Debug
#else
                MinimumLevel = LogEventLevel.Information
#endif
            };

            logService = new LogService(options);

            Stopwatch stopwatch = new();
            stopwatch.Start();

            CommandApp app = new();
            app.Configure(config =>
            {
                config.AddCommand<ImportCommand>("import")
                      .WithDescription("Import inscriptions from XLSX into MongoDB.");
            });

            int result = await app.RunAsync(args);

            Console.ResetColor();
            Console.CursorVisible = true;
            Console.WriteLine();
            Console.WriteLine();

            stopwatch.Stop();
            Console.WriteLine("\nTime: {0}h{1}'{2}\"",
                    stopwatch.Elapsed.Hours,
                    stopwatch.Elapsed.Minutes,
                    stopwatch.Elapsed.Seconds);

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            Console.CursorVisible = true;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.ToString());
            Console.ResetColor();
            return 2;
        }
        finally
        {
            // dispose the LogService (flushes Serilog and disables SelfLog if enabled)
            try
            {
                logService?.Dispose();
            }
            catch
            {
                // swallow to avoid masking exceptions on shutdown
            }
        }
    }
}
