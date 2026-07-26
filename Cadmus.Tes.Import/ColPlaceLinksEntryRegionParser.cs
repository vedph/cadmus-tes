using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import;

/// <summary>
/// Place column links entry region parser. This targets PinLinksPart.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-place-links")]
public sealed class ColPlaceLinksEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    private const string ORIGIN_ANCIENT_TAG = "col-site_of_origin_(ancient_name)";
    private const string ORIGIN_MODERN_TAG = "col-site_of_origin_(modern_name)";
    private const string ORIGIN_PLEIADES = "col-pleiades_id";

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags =>
    [
        ORIGIN_ANCIENT_TAG,
        ORIGIN_MODERN_TAG,
        ORIGIN_PLEIADES,
    ];

    /// <summary>
    /// Parses the region of entries at <paramref name="regionIndex" />
    /// in the specified <paramref name="regions" />.
    /// </summary>
    /// <param name="set">The entries set.</param>
    /// <param name="regions">The regions.</param>
    /// <param name="regionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    protected override int DoParse(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError("ID column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "ID column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            // if ancient name, store it for later use
            if (region.Tag == ORIGIN_ANCIENT_TAG)
                ctx.Data["place-origin-a"] = value;

            // add link
            PinLinksPart part = ctx.EnsurePartForCurrentItem<PinLinksPart>();
            part.Links.Add(new AssertedCompositeId
            {
                Tag = "origin",
                Scope = region.Tag == ORIGIN_PLEIADES
                    ? "pleiades"
                    : region.Tag == ORIGIN_ANCIENT_TAG?
                        "toponym-ancient" : "toponym-modern",
                Target = new PinTarget
                {
                    Gid = value,
                    // for Pleiades ID use the ancient name as label, if any
                    Label = region.Tag == ORIGIN_PLEIADES
                            ? ctx.GetData<string>("place-origin-a") ?? value
                            : value
                }
            });
        }

        return entryIndex + 3;
    }
}
