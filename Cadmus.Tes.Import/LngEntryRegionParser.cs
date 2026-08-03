using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column inscription language entry region parser. This targets
/// CategoriesPart:ins-lng.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-lng")]
public sealed class ColLngEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-language"];

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
            Logger?.LogError("Language column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "Language column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        HashSet<string> ids = [];
        foreach (string label in ImportHelper.GetValueList(value, false, ['|']))
        {
            string id = ImportHelper.GetThesaurusId(ctx, region, "categories_ins-lng@en",
                label, Logger);
            if (id == null)
            {
                Logger?.LogError("Unknown language label for {Tag}: \"{Label}\" " +
                    "at region {Region}", region.Tag, label, region);
                continue;
            }
            ids.Add(id);
        }

        if (ids.Count > 0)
        {
            CategoriesPart part = ctx.EnsurePartForCurrentItem<CategoriesPart>(
                "ins-lng");
            foreach (string id in ids) part.Categories.Add(id);
        }

        return entryIndex + 3;
    }
}
