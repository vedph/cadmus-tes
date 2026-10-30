using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Proteus.Core;


namespace Cadmus.Tes.Import;

/// <summary>
/// TES parent item link column entry region parser.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-internal-link")]
public sealed class ColInternalLinkEntryRegionParser :
    EntryRegionParser, IEntryRegionParser,
    IConfigurable<ColInternalLinkEntryRegionParserOptions>
{
    private ColInternalLinkEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Applies the parser options for internal link entry regions.
    /// </summary>
    /// <remarks>Throws <see cref="ArgumentNullException"/> when <paramref name="options"/> is <see
    /// langword="null"/>.</remarks>
    /// <param name="options">Options used to configure parser behavior.</param>
    public void Configure(ColInternalLinkEntryRegionParserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
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
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        // read the value (required, as it is the parent item EID)
        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (value == null)
        {
            Logger?.LogError(
                "{Tag} column with no value at region {Region}",
                region.Tag, region);
        }
        else
        {
            // get the item IDs set
            ExportedItemIdSet set = ImportHelper.GetItemIdsSet(ctx);

            // add link to the item
            PinLinksPart part = ctx.EnsurePartForCurrentItem<PinLinksPart>();
            ImportHelper.AddInternalLink(part, value, set, Logger);
        }

        return Task.FromResult(entryIndex + 3);
    }
}

/// <summary>
/// Options for <see cref="ColInternalLinkEntryRegionParser"/>.
/// </summary>
public class ColInternalLinkEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the region tag to be handled by parser.
    /// </summary>
    public string? Tag { get; set; }
}