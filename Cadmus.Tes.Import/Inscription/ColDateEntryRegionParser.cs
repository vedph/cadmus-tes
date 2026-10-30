using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System.Text.RegularExpressions;

namespace Cadmus.Tes.Import.Inscription;

/// <summary>
/// TES column date entry region parser. This handles the not-before and
/// not-after columns (in this order), each having a year (negative if BC),
/// building a date added to the item's <see cref="AssertedHistoricalDatesPart"/>:
/// a single point when the years are equal, else a range.
/// <para>Tag: <c>entry-region-parser.tes.col-date</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-date")]
public sealed partial class ColDateEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    private const string A_KEY = "col-date-a";
    private const string A_TAG = "col-date_notbefore";

    [GeneratedRegex(@"-(\d+)")]
    private static partial Regex BcRegex();

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => [A_TAG, "col-date_notafter"];

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        CheckRequired(region, value);

        // not before: store it for later
        if (region.Tag == A_TAG)
        {
            if (value != null) context.Data[A_KEY] = value;
            return;
        }

        // not after: add date if there is any term (A or B)
        string? a = context.GetData(A_KEY) as string;
        if (a == null && value == null) return;

        string text;
        // if A = B then this is a single date
        if (a == value) text = a!;
        // if there is B only it's --> B
        else if (a == null) text = $"-- {value}";
        // if there is A only it's A -->
        else if (value == null) text = $"{a} -- ";
        // if there are both A and B it's A --> B
        else text = $"{a} -- {value}";

        // replace negative number with number plus BC
        text = BcRegex().Replace(text, "$1 BC");

        // parse the date and add it to the part
        AssertedHistoricalDate? date = AssertedHistoricalDate.Parse(text);
        if (date is null)
        {
            Logger?.LogError("Invalid date \"{Date}\" at region {Region}",
                text, region);
            return;
        }
        context.EnsurePartForCurrentItem<AssertedHistoricalDatesPart>()
            .Dates.Add(date);
    }
}
