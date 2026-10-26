using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser. This targets ycoord, xcoord,
/// coord_certainty.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-coords")]
public sealed class ColCoordsEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags =>
    [
        "col-ycoord", "col-xcoord", "col-coord_certainty"
    ];

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

                // add rank to existing location part (error if not found)
                case "col-coord_certainty":
                    AssertedLocationsPart? existingPart = ctx.CurrentItem.Parts
                        .OfType<AssertedLocationsPart>().FirstOrDefault();
                    if (existingPart == null)
                    {
                        Logger?.LogError(
                            "Missing location part for coordinate certainty " +
                            "{Certainty} at region {Region}",
                            value, region);
                    }
                    else
                    {
                        existingPart.Locations.Last().Assertion = new Assertion
                        {
                            Rank = ImportHelper.LabelToRank(value, Logger)
                        };
                    }
                    break;
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}
