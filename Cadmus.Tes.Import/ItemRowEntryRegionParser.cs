using Fusi.Tools.Configuration;
using Proteus.Core;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES generic row entry region parser. This resets the context and adds
/// to it a new item with the configured facet.
/// <para>Tag: <c>entry-region-parser.tes.row-item</c>.</para>
/// </summary>
/// <seealso cref="RowEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.row-item")]
public sealed class ItemRowEntryRegionParser : RowEntryRegionParser,
    IEntryRegionParser, IConfigurable<ItemRowEntryRegionParserOptions>
{
    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ItemRowEntryRegionParserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ConfigureBaseOptions(options);
        FacetId = options.FacetId ?? "";
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="entryRegionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryIndex">The index of the first entry to parse.</param>
    /// <param name="entryRegions">The regions.</param>
    /// <param name="entryRegionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    protected override Task<int> DoParseAsync(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        return Task.FromResult(ParseRow(entrySet, entryIndex, entryRegions,
            entryRegionIndex));
    }
}

/// <summary>
/// Options for <see cref="ItemRowEntryRegionParser"/>.
/// </summary>
public class ItemRowEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the facet ID of the items to create (e.g. <c>cult</c>).
    /// </summary>
    public string? FacetId { get; set; }
}
