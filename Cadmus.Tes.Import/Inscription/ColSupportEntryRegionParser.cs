using Cadmus.Epigraphy.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// TES column material and object type entry region parser. This targets
/// the item's <see cref="EpiSupportPart"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-support</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-support")]
public sealed class ColSupportEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-material", "col-object_type"];

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (region.Tag == "col-material") CheckRequired(region, value);
        if (value == null) return;

        EpiSupportPart part = context.EnsurePartForCurrentItem<EpiSupportPart>();

        switch (region.Tag)
        {
            case "col-material":
                part.Material = ImportHelper.GetThesaurusEntryId(context, region,
                    "epi-support-materials@en", value, Logger);
                break;

            case "col-object_type":
                part.ObjectType = ImportHelper.GetThesaurusEntryId(context,
                    region, "epi-support-object-types@en", value, Logger);
                break;
        }
    }
}
