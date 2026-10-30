using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES column entry region parser for a property of the current resource,
/// i.e. the last resource of the <see cref="SiteResourcesPart"/> with
/// the configured role, as created by <see cref="ResourceRowEntryRegionParser"/>.
/// The target property can be the resource's EID, type, tag, or features.
/// Except for the EID, values are labels mapped to IDs via the configured
/// thesaurus; features can be multiple, separated by
/// <see cref="ColResPropertyEntryRegionParserOptions.Separator"/>.
/// <para>Tag: <c>entry-region-parser.tes.col-res-property</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-res-property")]
public sealed class ColResPropertyEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColResPropertyEntryRegionParserOptions>
{
    private ColResPropertyEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Configure this parser.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColResPropertyEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    private string GetId(CadmusEntrySetContext context, EntryRegion region,
        string value) =>
        ImportHelper.GetThesaurusEntryId(context, region, _options!.ThesaurusId!,
            value, Logger);

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (value == null) return;

        // thesaurus ID must be configured except for EID
        if (_options!.Property != ResourceProperty.Eid &&
            string.IsNullOrEmpty(_options.ThesaurusId))
        {
            Logger?.LogError("{Tag} column without any thesaurus configured " +
                "at region {Region}", region.Tag, region);
            return;
        }

        SiteResource? resource = ImportHelper.GetCurrentResource(context,
            _options.ResourceRoleId, region, Logger);
        if (resource == null) return;

        switch (_options.Property)
        {
            case ResourceProperty.Eid:
                resource.Eid = value;
                break;
            case ResourceProperty.Type:
                resource.Type = GetId(context, region, value);
                break;
            case ResourceProperty.Tag:
                resource.Tag = GetId(context, region, value);
                break;
            case ResourceProperty.Features:
                resource.Features ??= [];
                IList<string> labels = ImportHelper.GetValueList(value, false,
                    [_options.Separator]);
                foreach (string label in labels)
                {
                    string id = GetId(context, region, label);
                    if (!resource.Features.Contains(id))
                        resource.Features.Add(id);
                }
                break;
        }
    }
}

/// <summary>
/// The target property of <see cref="ColResPropertyEntryRegionParser"/>.
/// </summary>
public enum ResourceProperty
{
    /// <summary>The resource EID.</summary>
    Eid = 0,
    /// <summary>The resource type.</summary>
    Type,
    /// <summary>The resource tag.</summary>
    Tag,
    /// <summary>The resource features.</summary>
    Features
}

/// <summary>
/// Options for <see cref="ColResPropertyEntryRegionParser"/>.
/// </summary>
public class ColResPropertyEntryRegionParserOptions : ColEntryRegionParserOptions
{
    /// <summary>
    /// Gets or sets the role ID of the target resources part (e.g.
    /// <c>site-res</c>).
    /// </summary>
    public string? ResourceRoleId { get; set; }

    /// <summary>
    /// Gets or sets the target property.
    /// </summary>
    public ResourceProperty Property { get; set; }

    /// <summary>
    /// Gets or sets the thesaurus ID for the values. This is required for
    /// all the properties except <see cref="ResourceProperty.Eid"/>.
    /// </summary>
    public string? ThesaurusId { get; set; }

    /// <summary>
    /// Gets or sets the separator for multiple values, used for
    /// <see cref="ResourceProperty.Features"/>.
    /// </summary>
    public char Separator { get; set; } = ';';
}
