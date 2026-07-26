using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using Cadmus.Epigraphy.Parts;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column material support entry region parser.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-support")]
public sealed class ColSupportEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-material", "col-object_type"];

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
            Logger?.LogError("{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            EpiSupportPart part = ctx.EnsurePartForCurrentItem<EpiSupportPart>();

            switch (region.Tag)
            {
                case "col-material":
                    string id = ImportHelper.GetThesaurusId(ctx, region,
                        "epi-support-materials", value, Logger);
                    if (id == null)
                    {
                        Logger?.LogError("Unknown category label for {Tag}: \"{Label}\" " +
                            "at region {Region}", region.Tag, value, region);
                        break;
                    }
                    part.Material = id;
                    break;

                case "col-object_type":
                    string objectTypeId = ImportHelper.GetThesaurusId(ctx, region,
                        "epi-support-object-types", value, Logger);
                    if (objectTypeId == null)
                    {
                        Logger?.LogError("Unknown category label for {Tag}: \"{Label}\" " +
                            "at region {Region}", region.Tag, value, region);
                        break;
                    }
                    part.ObjectType = objectTypeId;
                    break;
            }
        }

        return entryIndex + 3;
    }
}
