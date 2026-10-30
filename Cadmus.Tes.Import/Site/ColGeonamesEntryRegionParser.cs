using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column site GeoNames entry region parser. This adds an external
/// link with the GeoNames ID to the item's <see cref="PinLinksPart"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-site-geonames</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-geonames")]
public sealed class ColSiteGeonamesEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-geonames_ref"];

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (value == null) return;

        PinLinksPart part = context.EnsurePartForCurrentItem<PinLinksPart>();
        part.Links.Add(new AssertedCompositeId
        {
            Scope = "geonames",
            Target = new PinTarget
            {
                Gid = value,
                Label = context.CurrentItem!.Title
            }
        });
    }
}
