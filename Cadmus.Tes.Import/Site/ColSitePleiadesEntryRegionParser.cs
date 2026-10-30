using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System.Text.RegularExpressions;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column site Pleiades entry region parser. When the value has digits
/// only, it is a Pleiades ID, added as an external link to the item's
/// <see cref="PinLinksPart"/>; else it is preserved as a
/// <c>pleiades_label</c> metadatum, and a warning is logged.
/// <para>Tag: <c>entry-region-parser.tes.col-site-pleiades</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-pleiades")]
public sealed partial class ColSitePleiadesEntryRegionParser :
    ColEntryRegionParser, IEntryRegionParser
{
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsRegex();

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-pleiades_ref"];

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

        if (DigitsRegex().IsMatch(value))
        {
            PinLinksPart part = context.EnsurePartForCurrentItem<PinLinksPart>();
            part.Links.Add(new AssertedCompositeId
            {
                Scope = "pleiades",
                Target = new PinTarget
                {
                    Gid = value,
                    Label = context.CurrentItem!.Title
                }
            });
        }
        else
        {
            MetadataPart part = context.EnsurePartForCurrentItem<MetadataPart>();
            part.Metadata.Add(new Metadatum
            {
                Type = "string",
                Name = "pleiades_label",
                Value = value
            });
            Logger?.LogWarning("Missing Pleiades identifier: {Tag} column " +
                "with non-numeric value \"{Value}\" at region {Region}",
                region.Tag, value, region);
        }
    }
}
