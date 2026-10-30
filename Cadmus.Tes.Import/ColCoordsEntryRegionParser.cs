using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core;
using Proteus.Core.Regions;
using System;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES coordinates entry region parser. This targets <c>ycoord</c>,
/// <c>xcoord</c> and <c>coord_certainty</c> (in this order), building a
/// location labelled with the item's title, which is added to the item's
/// <see cref="AssertedLocationsPart"/>; or, when
/// <see cref="ColCoordsEntryRegionParserOptions.ResourceRoleId"/> is set, is
/// assigned to the current resource in the <see cref="SiteResourcesPart"/>
/// with that role (labelled with the resource EID if any, else with the
/// item's title). The certainty becomes the location's assertion rank.
/// <para>Tag: <c>entry-region-parser.tes.col-coords</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-coords")]
public sealed class ColCoordsEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColCoordsEntryRegionParserOptions>
{
    private const string Y_KEY = "ycoord";
    private const string LOCATION_KEY = "coords-location";

    private ColCoordsEntryRegionParserOptions _options = new();

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags =>
    [
        "col-ycoord", "col-xcoord", "col-coord_certainty"
    ];

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColCoordsEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    private void AddLocation(CadmusEntrySetContext context, EntryRegion region,
        double latitude, double longitude)
    {
        AssertedLocation location;

        if (_options.ResourceRoleId != null)
        {
            SiteResource? resource = ImportHelper.GetCurrentResource(context,
                _options.ResourceRoleId, region, Logger);
            if (resource == null) return;

            location = new AssertedLocation
            {
                Value = new GeoLocation
                {
                    Label = resource.Eid ?? context.CurrentItem!.Title,
                    Latitude = latitude,
                    Longitude = longitude
                }
            };
            resource.Location = location;
        }
        else
        {
            location = new AssertedLocation
            {
                Value = new GeoLocation
                {
                    Label = context.CurrentItem!.Title,
                    Latitude = latitude,
                    Longitude = longitude
                }
            };
            context.EnsurePartForCurrentItem<AssertedLocationsPart>()
                .Locations.Add(location);
        }

        // keep the location created for this row for its certainty
        context.Data[LOCATION_KEY] = location;
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
            // store y for later (y comes first)
            case "col-ycoord":
                if (value == null) break;
                if (ImportHelper.TryParseDecimal(value, out double y))
                {
                    context.Data[Y_KEY] = y;
                }
                else
                {
                    Logger?.LogError("Invalid y coordinate \"{Value}\" " +
                        "at region {Region}", value, region);
                }
                break;

            // create the location from y + x
            case "col-xcoord":
                bool hasY = context.Data.TryGetValue(Y_KEY, out object? yv);
                if (value == null)
                {
                    if (hasY)
                    {
                        Logger?.LogError("Missing x coordinate for y " +
                            "coordinate {YCoord} at region {Region}", yv, region);
                    }
                    break;
                }
                if (!hasY)
                {
                    Logger?.LogError("Missing y coordinate for x coordinate " +
                        "{XCoord} at region {Region}", value, region);
                    break;
                }
                if (!ImportHelper.TryParseDecimal(value, out double x))
                {
                    Logger?.LogError("Invalid x coordinate \"{Value}\" " +
                        "at region {Region}", value, region);
                    break;
                }
                AddLocation(context, region, (double)yv!, x);
                break;

            // add rank to the location created for this row
            case "col-coord_certainty":
                if (value == null) break;
                if (context.Data.TryGetValue(LOCATION_KEY, out object? loc) &&
                    loc is AssertedLocation location)
                {
                    ImportHelper.SetRank(location,
                        ImportHelper.LabelToRank(value, Logger));
                }
                else
                {
                    Logger?.LogWarning("Coordinate certainty \"{Certainty}\" " +
                        "without coordinates at region {Region}",
                        value, region);
                }
                break;
        }
    }
}

/// <summary>
/// Options for <see cref="ColCoordsEntryRegionParser"/>.
/// </summary>
public class ColCoordsEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the role ID of the <see cref="SiteResourcesPart"/> whose
    /// current resource location is the target. When set, this replaces the
    /// locations part target.
    /// </summary>
    public string? ResourceRoleId { get; set; }
}
