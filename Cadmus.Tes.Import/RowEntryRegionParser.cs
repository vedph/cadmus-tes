using Cadmus.Core;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES row entry region parser. This resets the context and adds a new item
/// to it.
/// <para>Tag: <c>entry-region-parser.tes.row</c>.</para>
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
/// <remarks>
/// Initializes a new instance of the <see cref="RowEntryRegionParser"/>
/// class.
/// </remarks>
/// <param name="logger">The logger.</param>
[Tag("entry-region-parser.tes.row")]
public sealed class RowEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    private bool _first = false;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["row"];

    public override async Task RegionChangedAsync(string tag, int index,
        bool entered, IEntrySetContext context)
    {
        await base.RegionChangedAsync(tag, index, entered, context);

        if (tag == "row" && entered)
        {
            _first = true;
        }
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="entryRegionIndex" />
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

        // create the target item only for the first row
        if (!_first) return Task.FromResult(entryIndex);

        entrySet.Context.Reset();

        // find the first row-start command
        DecodedCommandEntry? row = null;
        EntryRegion region = entryRegions[entryRegionIndex];
        for (int i = region.Range.Start.Entry; i <= region.Range.End.Entry; i++)
        {
            if (entrySet.Entries[i] is DecodedCommandEntry cmd &&
                cmd.Name == "row-start")
            {
                row = cmd;
                break;
            }
        }
        if (row == null)
        {
            Logger?.LogError("Row command not found in region {Region}",
                region);
            throw new InvalidOperationException(
                "Row command not found in region " + region);
        }

        // log row's Y
        int y = int.Parse(row.GetArgument("y")!, CultureInfo.InvariantCulture);
        Logger?.LogInformation("-- ROW: {Row}", y);

        // add item for the row
        Item item = new()
        {
            FacetId = "inscription",
            CreatorId = "zeus",
            UserId = "zeus"
        };
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        ctx.Items.Clear();
        ctx.Items.Add(item);

        _first = false;
        return Task.FromResult(entryIndex + 1);
    }
}
