using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column date B entry region parser. This just caches the B term for
/// later use (when dealing with B).
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-date-b")]
public sealed class ColDateBEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-date_notafter"];

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
    protected override int DoParse(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError("col-date-a column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "col-date-a column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        // add date if there is any term (A or B)
        string? a = ctx.GetData<string>("col-date-a");
        if (a != null || !string.IsNullOrEmpty(value))
        {
            string text;
            // if A = B then this is a single date
            if (a == value)
            {
                text = a!;
            }
            else
            {
                // if there is B only it's --> B
                if (string.IsNullOrEmpty(a)) text = $"-- {value}";
                // if there is A only it's A -->
                else if (string.IsNullOrEmpty(value)) text = $"{a} -- ";
                // if there are both A and B it's A --> B
                else text = $"{a} -- {value}";
            }

            AssertedHistoricalDate date = AssertedHistoricalDate.Parse(text)!;

            AssertedHistoricalDatesPart part =
                ctx.EnsurePartForCurrentItem<AssertedHistoricalDatesPart>();
            part.Dates.Add(date);
        }

        return entryIndex + 3;
    }
}
