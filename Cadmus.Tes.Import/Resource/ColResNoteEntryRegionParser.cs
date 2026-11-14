using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using System;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES column resource note entry region parser. This appends the cell's
/// text, optionally under a heading, to the note of the current resource,
/// i.e. the last resource in the <see cref="SiteResourcesPart"/> with the
/// configured role (see <see cref="ResourceRowEntryRegionParser"/>).
/// <para>Tag: <c>entry-region-parser.tes.col-res-note</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-res-note")]
public sealed class ColResNoteEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColResNoteEntryRegionParserOptions>
{
    private ColResNoteEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Gets a value indicating whether the value passed to
    /// <see cref="ParseValue" /> must be just trimmed, rather than filtered.
    /// This preserves its line breaks and inner whitespaces.
    /// </summary>
    protected override bool RawValue => true;

    /// <summary>
    /// Configures this parser.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColResNoteEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        if (value == null) return;

        SiteResource? resource = TesImportHelper.GetCurrentResource(context,
            _options!.ResourceRoleId, region, Logger);
        if (resource == null) return;

        resource.Note = ImportHelper.AppendToNote(resource.Note, value,
            _options.Heading);
    }
}

/// <summary>
/// Options for <see cref="ColResNoteEntryRegionParser"/>.
/// </summary>
public class ColResNoteEntryRegionParserOptions : ColEntryRegionParserOptions
{
    /// <summary>
    /// Gets or sets the role ID of the target resources part (e.g.
    /// <c>site-res</c>).
    /// </summary>
    public string? ResourceRoleId { get; set; }

    /// <summary>
    /// Gets or sets the optional heading for the appended text.
    /// </summary>
    public string? Heading { get; set; }
}
