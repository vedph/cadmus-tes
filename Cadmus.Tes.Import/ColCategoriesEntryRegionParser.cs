using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser for any thesaurus. This maps
/// the cell's value(s), representing thesaurus entry labels, to entry IDs
/// and adds them to the <see cref="CategoriesPart"/> with the configured role.
/// <para>Tag: <c>entry-region-parser.tes.col-categories</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-categories")]
public sealed class ColCategoriesEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColCategoriesEntryRegionParserOptions>
{
    private ColCategoriesEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColCategoriesEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
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
        if (_options!.IsRequired && !CheckRequired(region, value)) return;
        if (value == null) return;

        // nothing to do if no thesaurus is configured
        if (string.IsNullOrEmpty(_options.ThesaurusId))
        {
            Logger?.LogError("{Tag} column with no thesaurus configured " +
                "at region {Region}", region.Tag, region);
            return;
        }

        IList<string> labels = _options.AllowMultipleValues
            ? ImportHelper.GetValueList(value, false, [_options.Separator])
            : [value];

        foreach (string label in labels)
        {
            string id = ImportHelper.GetThesaurusEntryId(
                context, region, _options.ThesaurusId, label, Logger);

            CategoriesPart part = context.EnsurePartForCurrentItem<CategoriesPart>(
                _options.PartRoleId);
            if (!part.Categories.Contains(id)) part.Categories.Add(id);
        }
    }
}

/// <summary>
/// Options for <see cref="ColCategoriesEntryRegionParser"/>.
/// </summary>
public class ColCategoriesEntryRegionParserOptions : ColEntryRegionParserOptions
{
    /// <summary>
    /// Gets or sets the ID of the thesaurus to use, e.g.
    /// <c>categories_site-type@en</c>.
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

    /// <summary>
    /// Gets or sets a value indicating whether the column is required.
    /// </summary>
    public bool IsRequired { get; set; }
}
