using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES boolean flag columns entry region parser for resources. Each handled
/// column is mapped to a thesaurus entry ID: when its cell contains
/// <c>YES</c>, the entry ID is added to the features of the current resource,
/// i.e. the last resource in the <see cref="SiteResourcesPart"/> with the
/// configured role (see <see cref="ResourceRowEntryRegionParser"/>).
/// <para>Tag: <c>entry-region-parser.tes.col-res-flags</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-res-flags")]
public sealed class ColResFlagsEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColResFlagsEntryRegionParserOptions>
{
    private ColResFlagsEntryRegionParserOptions _options = new();

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => [.. _options.Flags.Keys];

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColResFlagsEntryRegionParserOptions options)
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
        if (!ImportHelper.IsYes(value))
        {
            if (value != null &&
                !string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
            {
                Logger?.LogWarning("Unexpected value \"{Value}\" for flag " +
                    "at region {Region}", value, region);
            }
            return;
        }

        string id = _options.Flags[region.Tag!];
        if (_options.ThesaurusId != null)
        {
            ImportHelper.ValidateThesaurusEntryId(context, _options.ThesaurusId,
                id, Logger);
        }

        SiteResource? resource = TesImportHelper.GetCurrentResource(context,
            _options.ResourceRoleId, region, Logger);
        if (resource == null) return;

        resource.Features ??= [];
        if (!resource.Features.Contains(id)) resource.Features.Add(id);
    }
}

/// <summary>
/// Options for <see cref="ColResFlagsEntryRegionParser"/>.
/// </summary>
public class ColResFlagsEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the flags, where each key is a column region tag (e.g.
    /// <c>col-jewelry</c>) and each value is the corresponding thesaurus entry
    /// ID (e.g. <c>jewelry</c>).
    /// </summary>
    public Dictionary<string, string> Flags { get; set; } = [];

    /// <summary>
    /// Gets or sets the optional ID of the thesaurus used to validate entry
    /// IDs.
    /// </summary>
    public string? ThesaurusId { get; set; }

    /// <summary>
    /// Gets or sets the role ID of the target resources part (e.g.
    /// <c>site-mark</c>).
    /// </summary>
    public string? ResourceRoleId { get; set; }
}
