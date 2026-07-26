using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;


namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser. This targets TODO.
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
        "col-origin_latitude",
        "col-origin_longitude",
        "col-provenance_latitude",
        "col-provenance_longitude"
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
    protected override int DoParse(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError("coords column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "coords column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            AssertedLocationsPart part;
            double? lat;

            switch (region.Tag)
            {
                // keep latitude for later
                case "col-origin_latitude":
                    ctx.Data["origin_latitude"] = value;
                    break;

                // use latitude and longitude to create a location
                case "col-origin_longitude":
                    lat = ctx.GetData<double>("origin_latitude");
                    if (lat == null)
                    {
                        Logger?.LogError(
                            "coords column without origin latitude at region {Region}",
                            region);
                    }
                    else
                    {
                        part = ctx.EnsurePartForCurrentItem<AssertedLocationsPart>();
                        part.Locations.Add(new AssertedLocation
                        {
                            Tag = "origin",
                            Value = new GeoLocation
                            {
                                Latitude = lat.Value,
                                Longitude = double.Parse(value),
                            }
                        });
                    }
                    break;

                // keep latitude for later
                case "col-provenance_latitude":
                    ctx.Data["provenance_latitude"] = value;
                    break;

                // use latitude and longitude to create a location
                case "col-provenance_longitude":
                    lat = ctx.GetData<double>("provenance_latitude");
                    if (lat == null)
                    {
                        Logger?.LogError(
                            "coords column without provenance latitude at region {Region}",
                            region);

                    }
                    else
                    {
                        part = ctx.EnsurePartForCurrentItem<AssertedLocationsPart>();
                        part.Locations.Add(new AssertedLocation
                        {
                            Tag = "provenance",
                            Value = new GeoLocation
                            {
                                Latitude = lat.Value,
                                Longitude = double.Parse(value),
                            }
                        });
                    }
                    break;
            }
        }

        return entryIndex + 3;
    }
}
