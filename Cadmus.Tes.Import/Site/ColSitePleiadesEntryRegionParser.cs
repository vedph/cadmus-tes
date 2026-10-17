using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Cadmus.Refs.Bricks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-site-pleiades")]
public sealed class ColSitePleiadesEntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    private readonly Regex _digitsRegex = new(@"^\d+$");

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-pleiades_ref"];

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
                "__TAG__ column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            // if value contains digits only, it's a Pleiades ID to be stored
            // in external links (PinLinksPart); else it's a label to be
            // temporarily stored in MetadataPart (warn)
            if (_digitsRegex.IsMatch(value))
            {
                PinLinksPart? part = ctx.EnsurePartForCurrentItem<PinLinksPart>();
                part.Links.Add(new AssertedCompositeId
                {
                    Scope = "pleiades",
                    Target = new PinTarget
                    {
                        Gid = value
                    }
                });
            }
            else
            {
                MetadataPart? part = ctx.EnsurePartForCurrentItem<MetadataPart>();
                part.Metadata.Add(new Metadatum
                {
                    Name = "pleiades_label",
                    Value = value
                });
                Logger?.LogWarning(
                    "{Tag} column with non-numeric value {Value} at region {Region}",
                    region.Tag, value, region);
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}
