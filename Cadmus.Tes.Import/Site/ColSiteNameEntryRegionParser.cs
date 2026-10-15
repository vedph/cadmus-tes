using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Site;

/// <summary>
/// TES column categories entry region parser for site_name.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-name")]
public sealed class ColSiteNameEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-site_name"];

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
            Logger?.LogError("site_name column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "site_name column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);
        if (string.IsNullOrEmpty(value))
        {
            Logger?.LogWarning("site_name column with empty value at region {Region}",
                region);
            return Task.FromResult(entryIndex + 3);
        }

        // item title
        ctx.CurrentItem.Title = value;

        // toponym
        AssertedToponymsPart part = ctx.EnsurePartForCurrentItem<AssertedToponymsPart>();
        part.Toponyms.Add(new AssertedToponym
        {
            Name = new AssertedProperName
            {
                Pieces =
                [
                    new ProperNamePiece
                    {
                        Type = "name",
                        Value = value
                    }
                ]
            }
        });

        return Task.FromResult(entryIndex + 3);
    }
}
