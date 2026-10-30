using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column site ID entry region parser. This column contains the EID
/// of the parent site item of the resource item being imported. This EID is
/// used to fetch the corresponding item ID so that the item being created
/// for this row has the same ID of the parent site item. When exporting items
/// into database, this will merge parts of the resource item into the parent
/// site item.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-parent-site-id")]
public sealed class ColParentSiteIdEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-site-id"];

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

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        // there must be a value, else it's an error
        if (string.IsNullOrEmpty(value))
        {
            Logger?.LogError(
                "{Tag} column without any value at region {Region}",
                region.Tag, region);
        }
        else
        {
            // get the item IDs set
            ExportedItemIdSet set = ImportHelper.GetItemIdsSet(ctx);

            // get the parent site item ID from EID
            ExportedItemId? id = set.Get(value);
            if (id == null)
            {
                Logger?.LogError(
                    "{Tag} column with unknown EID {Eid} at region {Region}",
                    region.Tag, value, region);

            }
            else
            {
                // set the parent site item in the context
                ctx.CurrentItem.Id = id.Id;
                ctx.CurrentItem.Title = id.Title;
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}
