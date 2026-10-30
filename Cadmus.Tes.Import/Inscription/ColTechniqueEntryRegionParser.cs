using Cadmus.Epigraphy.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// TES column execution type 1 and 2 entry region parser. This targets
/// the item's <see cref="EpiTechniquePart"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-technique</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-technique")]
public sealed class ColTechniqueEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-execution_type_1", "col-execution_type_2"];

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

        string id = ImportHelper.GetThesaurusEntryId(context, region,
            "epi-technique-types@en", value, Logger);

        EpiTechniquePart part = context.EnsurePartForCurrentItem<EpiTechniquePart>();
        if (!part.Techniques.Contains(id)) part.Techniques.Add(id);
    }
}
