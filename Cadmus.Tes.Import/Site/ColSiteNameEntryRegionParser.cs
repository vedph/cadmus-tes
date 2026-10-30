using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column entry region parser for site_name. This sets the item's title
/// and adds a toponym to its <see cref="AssertedToponymsPart"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-site-name</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-name")]
public sealed class ColSiteNameEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-site_name"];

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (!CheckRequired(region, value)) return;

        // item title
        context.CurrentItem!.Title = value!;

        // toponym
        AssertedToponymsPart part =
            context.EnsurePartForCurrentItem<AssertedToponymsPart>();
        part.Toponyms.Add(new AssertedToponym
        {
            Name = new AssertedProperName
            {
                Pieces =
                [
                    new ProperNamePiece
                    {
                        Type = "name",
                        Value = value!
                    }
                ]
            }
        });
    }
}
