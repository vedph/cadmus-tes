using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cadmus.Tes.Import.Test;

/// <summary>
/// Validates the pipeline profiles in <c>tes-tool/Assets</c> against the
/// region parsers defined in this project and in the shared
/// <c>Cadmus.Import.Excel</c> library.
/// </summary>
public sealed class PipelineConfigTest
{
    private static readonly string[] _prefixes =
    [
        "entry-region-parser.tes.",
        "it.vedph.entry-region-parser.xlsx."
    ];

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly Dictionary<string, Type> _parsers =
        new[]
        {
            typeof(TesImportHelper).Assembly,
            typeof(Cadmus.Import.Excel.ImportHelper).Assembly
        }
        .SelectMany(a => a.GetTypes())
        .Where(t => !t.IsAbstract &&
            typeof(IEntryRegionParser).IsAssignableFrom(t) &&
            t.GetCustomAttribute<TagAttribute>(false) != null)
        .ToDictionary(t => t.GetCustomAttribute<TagAttribute>(false)!.Tag);

    private static string GetAssetsDir()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(
            Path.Combine(dir.FullName, "tes-tool", "Assets")))
        {
            dir = dir.Parent;
        }
        return dir == null
            ? throw new DirectoryNotFoundException("tes-tool/Assets not found")
            : Path.Combine(dir.FullName, "tes-tool", "Assets");
    }

    public static TheoryData<string> GetPipelineFiles()
    {
        TheoryData<string> data = [];
        foreach (string path in Directory.EnumerateFiles(GetAssetsDir(), "*.json")
            .Where(p => !Path.GetFileName(p).StartsWith("Thesauri")))
        {
            data.Add(Path.GetFileName(path));
        }
        return data;
    }

    private static IEntryRegionParser CreateParser(string id, JsonElement? options)
    {
        IEntryRegionParser parser =
            (IEntryRegionParser)Activator.CreateInstance(_parsers[id])!;

        Type? configurable = parser.GetType().GetInterfaces().FirstOrDefault(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IConfigurable<>));
        if (configurable != null && options != null)
        {
            Type optionsType = configurable.GetGenericArguments()[0];
            object o = options.Value.Deserialize(optionsType, _jsonOptions)!;
            configurable.GetMethod("Configure")!.Invoke(parser, [o]);
        }
        return parser;
    }

    [Fact]
    public void ParserTags_AreDistinct()
    {
        List<string> tags = [.. typeof(TesImportHelper).Assembly.GetTypes()
            .Select(t => t.GetCustomAttribute<TagAttribute>(false)?.Tag)
            .Where(t => t != null)
            .Select(t => t!)];

        Assert.Equal(tags.Count, tags.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(GetPipelineFiles))]
    public void Pipeline_ParsersExistAndRegionTagsAreUnique(string fileName)
    {
        using JsonDocument doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(GetAssetsDir(), fileName)));

        Dictionary<string, string> owners = [];
        foreach (JsonElement p in doc.RootElement
            .GetProperty("entryRegionParsers").EnumerateArray())
        {
            string id = p.GetProperty("id").GetString()!;
            Assert.Contains(_prefixes, prefix => id.StartsWith(prefix));
            Assert.True(_parsers.ContainsKey(id), $"{fileName}: unknown parser {id}");

            IEntryRegionParser parser = CreateParser(id,
                p.TryGetProperty("options", out JsonElement o) ? o : null);

            Assert.NotEmpty(parser.RegionTags);
            foreach (string tag in parser.RegionTags)
            {
                Assert.True(owners.TryAdd(tag, id),
                    $"{fileName}: region {tag} handled by both " +
                    $"{(owners.TryGetValue(tag, out string? other) ? other : "")} " +
                    $"and {id}");
            }
        }
    }
}
