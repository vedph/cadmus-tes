using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// TES inscription coordinates entry region parser. This handles the origin
/// and provenance latitude/longitude pairs, each building a location in the
/// item's <see cref="AssertedLocationsPart"/>, tagged with <c>origin</c> or
/// <c>provenance</c>. The origin location is labelled with the ancient name
/// of the site of origin when available.
/// <para>Tag: <c>entry-region-parser.tes.col-ins-coords</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-ins-coords")]
public sealed class ColCoordsEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
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

    private void AddLocation(CadmusEntrySetContext context, EntryRegion region,
        string? value, string tag, string label)
    {
        string latKey = tag + "_latitude";
        bool hasLat = context.Data.TryGetValue(latKey, out object? lat);

        if (value == null)
        {
            if (hasLat)
            {
                Logger?.LogError("Missing {Tag} longitude for latitude " +
                    "{Latitude} at region {Region}", tag, lat, region);
            }
            return;
        }
        if (!hasLat)
        {
            Logger?.LogError("Missing {Tag} latitude for longitude " +
                "{Longitude} at region {Region}", tag, value, region);
            return;
        }
        if (!ImportHelper.TryParseDecimal(value, out double lng))
        {
            Logger?.LogError("Invalid {Tag} longitude \"{Value}\" at region " +
                "{Region}", tag, value, region);
            return;
        }

        context.EnsurePartForCurrentItem<AssertedLocationsPart>().Locations.Add(
            new AssertedLocation
            {
                Tag = tag,
                Value = new GeoLocation
                {
                    Label = label,
                    Latitude = (double)lat!,
                    Longitude = lng,
                }
            });
    }

    private void StoreLatitude(CadmusEntrySetContext context, EntryRegion region,
        string? value, string tag)
    {
        if (value == null) return;

        if (ImportHelper.TryParseDecimal(value, out double lat))
        {
            context.Data[tag + "_latitude"] = lat;
        }
        else
        {
            Logger?.LogError("Invalid {Tag} latitude \"{Value}\" at region " +
                "{Region}", tag, value, region);
        }
    }

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        switch (region.Tag)
        {
            case "col-origin_latitude":
                if (CheckRequired(region, value))
                    StoreLatitude(context, region, value, "origin");
                break;

            case "col-origin_longitude":
                CheckRequired(region, value);
                AddLocation(context, region, value, "origin",
                    context.GetData(ColPlaceLinksEntryRegionParser.ANCIENT_KEY)
                        as string ?? "origin");
                break;

            case "col-provenance_latitude":
                StoreLatitude(context, region, value, "provenance");
                break;

            case "col-provenance_longitude":
                AddLocation(context, region, value, "provenance", "provenance");
                break;
        }
    }
}
