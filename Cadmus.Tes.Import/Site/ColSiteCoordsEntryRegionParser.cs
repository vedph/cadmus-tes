using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column categories entry region parser. This targets ycoord and xcoord.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site_coords")]
public sealed class ColSiteCoordsEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-ycoord", "col-xcoord"];

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
                region.Tag + " column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            switch (region.Tag)
            {
                // store y for later (y comes first)
                case "col-ycoord":
                    ctx.Data["ycoord"] = value;
                    break;

                // create the location from y + x (get label from item's title)
                case "col-xcoord":
                    // if y is missing it's an error
                    if (!ctx.Data.TryGetValue("ycoord", out object? y) ||
                        y is not string)
                    {
                        Logger?.LogError(
                            "Missing y coordinate for x coordinate " +
                            "{XCoord} at region {Region}",
                            value, region);
                    }
                    else
                    {
                        AssertedLocationsPart part =
                            ctx.EnsurePartForCurrentItem<AssertedLocationsPart>();
                        part.Locations.Add(new AssertedLocation
                        {
                            Value = new GeoLocation
                            {
                                Latitude = double.Parse((string)y),
                                Longitude = double.Parse(value),
                                Label = ctx.CurrentItem.Title
                            }
                        });
                    }
                    break;
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}
