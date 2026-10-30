using Proteus.Core;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// Options for <see cref="ColResAccuracyEntryRegionParser"/> and other parsers
/// related to site features.
/// </summary>
public class ColResEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the tag of the region to be parsed.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the thesaurus ID for the values.
    /// </summary>
    public string? ThesaurusId { get; set; }
}
