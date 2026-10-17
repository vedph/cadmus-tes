using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Fusi.Antiquity.Chronology;
using Cadmus.Refs.Bricks;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column categories entry region parser.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-period")]
public sealed class ColSitePeriodEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    private const string SITE_PERIODS_KEY = "site-periods";

    /// <summary>
    /// Gets the tags of the regions that this parser can handle (period1-22).
    /// </summary>
    public string[] RegionTags =>
        [.. Enumerable.Range(1, 22).Select(i => $"col-period_-{i}")];

    private static int ParseTagNumber(string tag)
    {
        string numberPart = tag["col-period_-".Length..];
        return int.Parse(numberPart, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="regionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryRegions">The regions.</param>
    /// <param name="entryRegionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    protected override Task<int> DoParseAsync(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError(
                "{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, true);

        if (value == "yes")
        {
            // cache period numbers until we get to the last one
            List<int>? existing = ctx.GetData(SITE_PERIODS_KEY) as List<int>;
            List<int> numbers = existing ?? [];
            numbers.Add(ParseTagNumber(region.Tag!));

            if (numbers[^1] == 22)
            {
                // last period, create the part
                AssertedHistoricalDatesPart part =
                    ctx.EnsurePartForCurrentItem<AssertedHistoricalDatesPart>();

                // calculate all ranges and store them as approximate range dates
                DateBinsConverter converter = new();
                IList<(int Start, int End)> ranges = converter.Convert(numbers);

                foreach (var (Start, End) in ranges)
                {
                    part.Dates.Add(new AssertedHistoricalDate
                    {
                        A = new Datation(Start)
                        {
                            IsApproximate = true
                        },
                        B = new Datation(End)
                        {
                            IsApproximate = true
                        }
                    });
                }
                // consumed, remove from context
                ctx.Data.Remove(SITE_PERIODS_KEY);
            }
            else
            {
                ctx.Data[SITE_PERIODS_KEY] = numbers;
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}