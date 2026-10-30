using Cadmus.Import.Proteus;
using Cadmus.Tes.Import;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using Proteus.Entries.Config;
using Proteus.Entries.Pipeline;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tes.Cli.Services;

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

    /// <summary>
    /// Loads all the item IDs sets (<c>item-ids*.json</c>) found in the
    /// specified directory. Each set is keyed under its file name without
    /// extension; the merged set of all of them is keyed under null.
    /// </summary>
    private static Dictionary<string, ExportedItemIdSet> LoadItemIdSets(
        string dir, out ExportedItemIdSet? merged)
    {
        Dictionary<string, ExportedItemIdSet> sets = [];
        merged = null;
        if (!Directory.Exists(dir)) return sets;

        List<ExportedItemId> all = [];
        HashSet<string> ids = [];
        foreach (string path in Directory.EnumerateFiles(dir, "item-ids*.json")
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(
                $"Loading exported item IDs set from [cyan]{path}[/]...");
            ExportedItemIdSet set = ExportedItemIdSet.Load(path);
            sets[Path.GetFileNameWithoutExtension(path)] = set;

            foreach (ExportedItemId id in set)
            {
                if (ids.Add(id.Id)) all.Add(id);
                else
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]Duplicate item ID {id.Id} in {path}[/]");
                }
            }
        }

        if (sets.Count > 0) merged = new ExportedItemIdSet(all);
        return sets;
    }

    public override async Task<int> ExecuteAsync(CommandContext context,
        ImportCommandSettings settings, CancellationToken cancellationToken)
    {
        ShowSettings(settings);

        try
        {
            // load thesaurus map
            ThesaurusEntryMap map = LoadThesaurusMap();

            // load item IDs sets if present
            Dictionary<string, ExportedItemIdSet> sets = LoadItemIdSets(
                Path.GetDirectoryName(Path.GetFullPath(settings.PipelinePath!))
                    ?? "", out ExportedItemIdSet? set);

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
            await pipeline.StartAsync();
            try
            {
                while (await setReader.ReadAsync())
                {
                    // get context and assign thesaurus map and IDs set if present
                    CadmusEntrySetContext c =
                        (CadmusEntrySetContext)setReader.Set.Context;
                    c.ThesaurusEntryMap = map;
                    if (set != null) c.Data[ImportHelper.ITEM_IDS_KEY] = set;
                    foreach (KeyValuePair<string, ExportedItemIdSet> p in sets)
                        c.Data[ImportHelper.GetItemIdsKey(p.Key)] = p.Value;

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
                await pipeline.EndAsync();
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
