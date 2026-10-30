using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// Place column links entry region parser. This targets the item's
/// <see cref="PinLinksPart"/> with the ancient and modern names of the site
/// of origin, and its Pleiades ID. Names are collected, and when the Pleiades
/// ID column is reached, an external link with the Pleiades ID is added for
/// each of them, labelled with the name. If there is no Pleiades ID, the
/// names are preserved as toponym links, whose GID is the name itself.
/// <para>Tag: <c>entry-region-parser.tes.col-place-links</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-place-links")]
public sealed class ColPlaceLinksEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// The context data key for the ancient name of the site of origin.
    /// </summary>
    public const string ANCIENT_KEY = "place-origin-a";

    /// <summary>
    /// The context data key for the modern name of the site of origin.
    /// </summary>
    public const string MODERN_KEY = "place-origin-m";

    private const string ORIGIN_ANCIENT_TAG = "col-site_of_origin_(ancient_name)";
    private const string ORIGIN_MODERN_TAG = "col-site_of_origin_(modern_name)";
    private const string ORIGIN_PLEIADES_TAG = "col-pleiades_id";

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags =>
    [
        ORIGIN_ANCIENT_TAG,
        ORIGIN_MODERN_TAG,
        ORIGIN_PLEIADES_TAG,
    ];

    private static void AddLink(PinLinksPart part, string scope, string gid,
        string label)
    {
        part.Links.Add(new AssertedCompositeId
        {
            Tag = "origin",
            Scope = scope,
            Target = new PinTarget
            {
                Gid = gid,
                Label = label
            }
        });
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
            // names: store them for the Pleiades ID column
            case ORIGIN_ANCIENT_TAG:
                if (CheckRequired(region, value)) context.Data[ANCIENT_KEY] = value!;
                return;
            case ORIGIN_MODERN_TAG:
                if (CheckRequired(region, value)) context.Data[MODERN_KEY] = value!;
                return;
        }

        // Pleiades ID: add links
        string? ancient = context.GetData(ANCIENT_KEY) as string;
        string? modern = context.GetData(MODERN_KEY) as string;
        if (value == null && ancient == null && modern == null) return;

        PinLinksPart part = context.EnsurePartForCurrentItem<PinLinksPart>();

        if (value == null)
        {
            Logger?.LogWarning("No Pleiades ID for site of origin at region " +
                "{Region}: names stored as toponyms", region);
            if (ancient != null) AddLink(part, "toponym-ancient", ancient, ancient);
            if (modern != null) AddLink(part, "toponym-modern", modern, modern);
            return;
        }

        if (ancient == null && modern == null)
        {
            AddLink(part, "pleiades", value, value);
            return;
        }
        if (ancient != null) AddLink(part, "pleiades", value, ancient);
        if (modern != null) AddLink(part, "pleiades", value, modern);
    }
}
