using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Parts;
using Fusi.Antiquity.Chronology;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES periods columns entry region parser. This handles the boolean
/// columns <c>period_1</c> ... <c>period_N</c>, each corresponding to a bin
/// of years: the bins whose cell is <c>YES</c> are collected, and when the
/// last period column is reached, they are converted into one approximate
/// date range for each run of consecutive bins (see
/// <see cref="DateBinsConverter"/>). Ranges are added to the target, which is
/// either the item's <see cref="AssertedHistoricalDatesPart"/>, or the dates
/// of the current resource in the <see cref="SiteResourcesPart"/> with role
/// <see cref="ColPeriodsEntryRegionParserOptions.ResourceRoleId"/>, or a
/// chronology block in the text of the item's <see cref="CommentPart"/>.
/// <para>Optionally, this also handles a certainty column (following the
/// periods), whose value is used as the assertion rank of all the dates
/// added to the item's <see cref="AssertedHistoricalDatesPart"/>.</para>
/// <para>Tag: <c>entry-region-parser.tes.col-periods</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-periods")]
public sealed class ColPeriodsEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColPeriodsEntryRegionParserOptions>
{
    private const string PERIODS_KEY = "periods";
    private const string TAG_PREFIX = "col-period_";

    private ColPeriodsEntryRegionParserOptions _options = new();

    /// <summary>
    /// Gets the tags of the regions that this parser can handle
    /// (<c>col-period_1</c> ... <c>col-period_N</c> plus the optional
    /// certainty tag).
    /// </summary>
    public string[] RegionTags
    {
        get
        {
            List<string> tags = [.. Enumerable.Range(1, _options.PeriodCount)
                .Select(i => TAG_PREFIX + i.ToString(CultureInfo.InvariantCulture))];
            if (_options.CertaintyTag != null) tags.Add(_options.CertaintyTag);
            return [.. tags];
        }
    }

    /// <summary>
    /// Gets a value indicating whether the value passed to
    /// <see cref="ParseValue" /> must be lowercased.
    /// </summary>
    protected override bool LowercaseValue => true;

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColPeriodsEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    private List<AssertedHistoricalDate> BuildDates(List<int> bins)
    {
        DateBinsConverter converter = new()
        {
            BaseYear = _options.BaseYear,
            BinSize = _options.BinSize
        };

        return [.. converter.Convert(bins).Select(r => new AssertedHistoricalDate
        {
            A = new Datation(r.Start) { IsApproximate = true },
            B = new Datation(r.End) { IsApproximate = true }
        })];
    }

    private void AddDates(CadmusEntrySetContext context, EntryRegion region,
        List<AssertedHistoricalDate> dates)
    {
        switch (_options.Target)
        {
            case PeriodsTarget.Comment:
                StringBuilder sb = new();
                foreach (AssertedHistoricalDate date in dates)
                    sb.Append("- ").Append(date).Append('\n');

                CommentPart comment = context.EnsurePartForCurrentItem<CommentPart>(
                    _options.PartRoleId);
                comment.Text = ImportHelper.AppendToNote(comment.Text,
                    sb.ToString().TrimEnd(), _options.Heading);
                break;

            default:
                if (_options.ResourceRoleId != null)
                {
                    SiteResource? resource = ImportHelper.GetCurrentResource(
                        context, _options.ResourceRoleId, region, Logger);
                    if (resource == null) return;
                    resource.Dates ??= [];
                    resource.Dates.AddRange(dates);
                }
                else
                {
                    context.EnsurePartForCurrentItem<AssertedHistoricalDatesPart>(
                        _options.PartRoleId).Dates.AddRange(dates);
                }
                break;
        }
    }

    private void ParseCertainty(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (value == null) return;

        AssertedHistoricalDatesPart? part = context.CurrentItem!.Parts
            .OfType<AssertedHistoricalDatesPart>()
            .FirstOrDefault(p => _options.PartRoleId == null ||
                p.RoleId == _options.PartRoleId);

        if (part == null || part.Dates.Count == 0)
        {
            Logger?.LogWarning("Chronology certainty \"{Certainty}\" without " +
                "any date at region {Region}", value, region);
            return;
        }

        short rank = ImportHelper.LabelToRank(value, Logger);
        foreach (AssertedHistoricalDate date in part.Dates)
            ImportHelper.SetRank(date, rank);
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
        if (region.Tag == _options.CertaintyTag)
        {
            ParseCertainty(context, region, value);
            return;
        }

        int n = int.Parse(region.Tag![TAG_PREFIX.Length..],
            CultureInfo.InvariantCulture);

        // cache YES period numbers until we get to the last one
        List<int> bins = context.GetData(PERIODS_KEY) as List<int> ?? [];
        if (ImportHelper.IsYes(value)) bins.Add(n);
        else if (value != null && value != "no")
        {
            Logger?.LogWarning("Unexpected value \"{Value}\" for period " +
                "at region {Region}", value, region);
        }

        if (n < _options.PeriodCount)
        {
            context.Data[PERIODS_KEY] = bins;
            return;
        }

        // last period: consume bins
        context.Data.Remove(PERIODS_KEY);
        if (bins.Count > 0) AddDates(context, region, BuildDates(bins));
    }
}

/// <summary>
/// Target of <see cref="ColPeriodsEntryRegionParser"/>.
/// </summary>
public enum PeriodsTarget
{
    /// <summary>
    /// Dates in an <see cref="AssertedHistoricalDatesPart"/> or in a
    /// resource.
    /// </summary>
    Dates = 0,

    /// <summary>
    /// A chronology block in a <see cref="CommentPart"/>.
    /// </summary>
    Comment
}

/// <summary>
/// Options for <see cref="ColPeriodsEntryRegionParser"/>.
/// </summary>
public class ColPeriodsEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the count of period columns. Default is 22.
    /// </summary>
    public int PeriodCount { get; set; } = 22;

    /// <summary>
    /// Gets or sets the base year of the first period. Default is -1000.
    /// </summary>
    public int BaseYear { get; set; } = -1000;

    /// <summary>
    /// Gets or sets the size of each period in years. Default is 25.
    /// </summary>
    public int BinSize { get; set; } = 25;

    /// <summary>
    /// Gets or sets the target. Default is dates.
    /// </summary>
    public PeriodsTarget Target { get; set; }

    /// <summary>
    /// Gets or sets the optional role ID of the target part.
    /// </summary>
    public string? PartRoleId { get; set; }

    /// <summary>
    /// Gets or sets the role ID of the <see cref="SiteResourcesPart"/> whose
    /// current resource dates are the target, when <see cref="Target"/> is
    /// <see cref="PeriodsTarget.Dates"/>. When set, this replaces the dates
    /// part target.
    /// </summary>
    public string? ResourceRoleId { get; set; }

    /// <summary>
    /// Gets or sets the heading of the chronology block when
    /// <see cref="Target"/> is <see cref="PeriodsTarget.Comment"/>.
    /// </summary>
    public string? Heading { get; set; } = "Chronology";

    /// <summary>
    /// Gets or sets the optional tag of the certainty column, whose value
    /// is used as the rank of all the dates in the item's dates part.
    /// </summary>
    public string? CertaintyTag { get; set; }
}
