using Cadmus.Core;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cadmus.Tes.Import;

/// <summary>
/// VeLA row entry region parser. This resets the context and adds a new item
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
public sealed class RowEntryRegionParser() : EntryRegionParser, IEntryRegionParser
{
    public string[] RegionTags => ["row"];

    /// <summary>
    /// Parses the entries starting from <paramref name="entryIndex"/>
    /// in the specified region context.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryIndex">The index to the entry to start parsing from.
    /// </param>
    /// <param name="entryRegions">The regions which include the input entry.</param>
    /// <param name="entryRegionIndex">The index of the region being processed
    /// in <paramref name="entryRegions"/>.</param>
    /// <returns>The index to the next entry to be parsed, which can be
    /// equal to <paramref name="entryIndex"/> if this parser did not consume
    /// any entries, or to -1 to force a redirect to the default parser.</returns>
    protected override int DoParse(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

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
            UserId = "zeus",
        };
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        ctx.Items.Clear();
        ctx.Items.Add(item);

        return entryIndex + 1;
    }
}
