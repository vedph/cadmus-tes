using Cadmus.Import.Proteus;
using Tes.Cli.Services;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using Proteus.Entries.Config;
using Proteus.Entries.Pipeline;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Tes.Cli.Commands;

internal sealed class ImportCommand : AsyncCommand<ImportCommandSettings>
{
    private static void ShowSettings(ImportCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]RUN IMPORT PIPELINE[/]");
        AnsiConsole.MarkupLine($"Pipeline: [cyan]{settings.PipelinePath}[/]");
    }

    private static string LoadFileContent(string path)
    {
        using StreamReader reader = new(path);
        return reader.ReadToEnd();
    }

    private static ThesaurusEntryMap LoadThesaurusMap()
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(), "Assets", "Thesauri.json");

        ThesaurusEntryMap map = new();
        using Stream stream = File.OpenText(path).BaseStream;
        map.Load(stream);
        return map;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context,
        ImportCommandSettings settings, CancellationToken cancellationToken)
    {
        ShowSettings(settings);

        try
        {
            ThesaurusEntryMap map = LoadThesaurusMap();

            // load pipeline config
            AnsiConsole.WriteLine("Building pipeline...");
            string config = LoadFileContent(settings.PipelinePath!);

            // build pipeline (either from stock components or from plugin)
            IEntryPipelineFactory factory = PipelineFactoryProvider
                .GetFactory(config);

            EntryPipeline pipeline;
            try
            {
                pipeline = new(factory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }

            // get entry reader
            AnsiConsole.WriteLine("Getting entries set reader...");
            EntrySetReader setReader = factory.GetEntrySetReader();

            // open exporters outputs
            if (pipeline.Exporters.Count > 0)
            {
                AnsiConsole.WriteLine("Opening exporters...");
                foreach (IEntrySetExporter exporter in pipeline.Exporters)
                {
                    await exporter.OpenAsync();
                }
            }

            // process sets
            AnsiConsole.WriteLine("Reading entry sets: ");
            int count = 0;
            pipeline.Start();
            try
            {
                while (setReader.Read())
                {
                    CadmusEntrySetContext c =
                        (CadmusEntrySetContext)setReader.Set.Context;
                    c.ThesaurusEntryMap = map;

                    count++;
                    AnsiConsole.Write('.');
                    factory.Logger?.LogInformation("Reading set #{Number}",
                        c.Number);
                    await pipeline.ExecuteAsync(setReader.Set);
                }
            }
            finally
            {
                AnsiConsole.WriteLine();
                pipeline.End();
            }
            AnsiConsole.MarkupLine($"\n[green]Sets read: {count}[/]");

            // close exporters output
            if (pipeline.Exporters.Count > 0)
            {
                AnsiConsole.WriteLine("Closing exporters...");
                foreach (IEntrySetExporter exporter in pipeline.Exporters)
                {
                    await exporter.CloseAsync();
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class ImportCommandSettings : CommandSettings
{
    [CommandArgument(0, "<PIPELINE_PATH>")]
    [Description("The pipeline configuration file path")]
    public string PipelinePath { get; set; } = "";
}
