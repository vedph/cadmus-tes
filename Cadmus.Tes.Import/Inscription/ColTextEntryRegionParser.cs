using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// TES column text entry region parser. This adds each line of the cell's
/// text to the item's <see cref="TokenTextPart"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-text</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-text")]
public sealed class ColTextEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-edition_(interpretive)"];

    /// <summary>
    /// Gets a value indicating whether the value passed to
    /// <see cref="ParseValue" /> must be just trimmed, rather than filtered.
    /// This preserves its line breaks and inner whitespaces.
    /// </summary>
    protected override bool RawValue => true;

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (value == null) return;

        TokenTextPart part = context.EnsurePartForCurrentItem<TokenTextPart>(
            "base-text");
        short y = 1;
        foreach (string line in value.Replace("\r\n", "\n").Split('\n'))
        {
            part.Lines.Add(new TextLine
            {
                Y = y++,
                Text = line.Trim(),
            });
        }
    }
}
