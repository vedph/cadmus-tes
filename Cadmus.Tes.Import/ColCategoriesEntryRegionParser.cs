using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser for any thesaurus.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-categories")]
public sealed class ColCategoriesEntryRegionParser :
    EntryRegionParser, IEntryRegionParser, IConfigurable<ColCategoriesEntryRegionParserOptions>
{
    private ColCategoriesEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null? [_options.Tag] : [];

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColCategoriesEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
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
                "archaeological_site_type column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "archaeological_site_type column without any item at region " +
                region);
        }

        // nothing to do if no thesaurus is configured
        if (_options?.ThesaurusId == null)
        {
            Logger?.LogError(
                "{Tag} column with no thesaurus configured " +
                "at region {Region}", _options?.Tag, region);
            return Task.FromResult(entryIndex + 3);
        }

        // get the value, nothing to do if empty
        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);
        if (string.IsNullOrEmpty(value))
        {
            return Task.FromResult(entryIndex + 3);
        }

        CategoriesPart? part = null;

        // if multiple values are allowed, split them
        if (_options.AllowMultipleValues == true)
        {
            string[] values = value.Split(_options.Separator,
                StringSplitOptions.RemoveEmptyEntries);
            foreach (string v in values)
            {
                string? id = ImportHelper.GetThesaurusEntryId(
                    ctx, region, _options!.ThesaurusId, v.Trim(), Logger);
                if (id != null)
                {
                    // create categories part if required
                    part ??= ctx.EnsurePartForCurrentItem<CategoriesPart>(
                        _options.PartRoleId);
                    // add category entry ID
                    part.Categories.Add(id);
                }
            }
        }
        else
        {
            string? id = ImportHelper.GetThesaurusEntryId(
                ctx, region, _options!.ThesaurusId, value, Logger);
            if (id != null)
            {
                // create categories part if required
                part ??= ctx.EnsurePartForCurrentItem<CategoriesPart>(
                    _options.PartRoleId);
                // add category entry ID
                part.Categories.Add(id);
            }
        }

        return Task.FromResult(entryIndex + 3);
    }
}

/// <summary>
/// Options for <see cref="ColCategoriesEntryRegionParser"/>.
/// </summary>
public class ColCategoriesEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the region tag to handle.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the ID of the thesaurus to use.
    /// </summary>
    public string? ThesaurusId { get; set; }

    /// <summary>
    /// Gets or sets a value telling whether more than a single value is allowed
    /// in the parsed region. When true, the values are split by
    /// <see cref="Separator"/>.
    /// </summary>
    public bool AllowMultipleValues { get; set; }

    /// <summary>
    /// Gets or sets the separator character for multiple values.
    /// </summary>
    public char Separator { get; set; } = ';';

    /// <summary>
    /// Gets or sets the optional role ID of the target categories part.
    /// </summary>
    public string? PartRoleId { get; set; }
}
