using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES column categories entry region parser for data_accuracy.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-resource-feature-id")]
public sealed class ColResEidEntryRegionParser :
    EntryRegionParser, IEntryRegionParser,
    IConfigurable<ColResEntryRegionParserOptions>
{
    private ColResEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Configure this parser.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColResEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="regionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryRegions">The regions.</param>
    /// <param name="entryRegionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    protected override Task<int> DoParseAsync(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError(
                "{Tag} column without any item at region {Region}",
                _options?.Tag, region);
            throw new InvalidOperationException(
                $"{_options?.Tag} column without any item at region {region}");
        }

        // thesaurus ID must be configured
        if (string.IsNullOrEmpty(_options?.ThesaurusId))
        {
            Logger?.LogError(
                "{Tag} column without any thesaurus configured at region {Region}",
                _options?.Tag, region);
        }
        else
        {
            // get value
            DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
                entryIndex + 1)!;
            string? value = ImportHelper.FilterValue(txt.Value, false);

            // if there is a value, set the part's resource EID to it
            if (!string.IsNullOrEmpty(value))
            {
                SiteResourcesPart part =
                    ctx.EnsurePartForCurrentItem<SiteResourcesPart>();
                part.Resources[^1].Eid = value;
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}
