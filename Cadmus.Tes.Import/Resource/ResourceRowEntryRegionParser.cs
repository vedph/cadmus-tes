using Cadmus.Import.Proteus;
using Cadmus.Tes.Import.Inscription;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Proteus.Core;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES row entry region parser for site resources. This resets the context and
/// adds a new item to it, with a <see cref="SiteResourcesPart"/>. This is used
/// for all resources in the site, each time with a specific part role ID.
/// <para>Tag: <c>entry-region-parser.tes.row-site</c>.</para>
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
/// <remarks>
/// Initializes a new instance of the <see cref="ResourceRowEntryRegionParser"/>
/// class.
/// </remarks>
/// <param name="logger">The logger.</param>
[Tag("entry-region-parser.tes.row-resource")]
public sealed class ResourceRowEntryRegionParser :
    RowEntryRegionParser, IEntryRegionParser,
    IConfigurable<ResourceRowEntryRegionParserOptions>
{
    private string? _partRoleId = null;

    public ResourceRowEntryRegionParser()
    {
        FacetId = "resource";
    }

    /// <summary>
    /// Applies the specified parser options to the current configuration.
    /// </summary>
    /// <param name="options">Options that provide configuration values for
    /// the parser.</param>
    /// <exception cref="ArgumentNullException">Null options</exception>
    public void Configure(ResourceRowEntryRegionParserOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _partRoleId = options.PartRoleId;
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="entryRegionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryRegions">The regions.</param>
    /// <param name="entryRegionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    protected override Task<int> DoParseAsync(EntrySet entrySet,
        int entryIndex, IReadOnlyList<EntryRegion> entryRegions,
        int entryRegionIndex)
    {
        // create the new item and set it as current
        int row = ParseRow(entrySet, entryIndex, entryRegions,
            entryRegionIndex);

        // add the resources part with the configured role ID to the item
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        ctx.EnsurePartForCurrentItem<SiteResourcesPart>(_partRoleId);

        return Task.FromResult(row);
    }
}

/// <summary>
/// Represents configuration options for parsing a resource row entry region.
/// </summary>
public class ResourceRowEntryRegionParserOptions : DisabledOptions
{
    /// <summary>
    /// Gets or sets the optional ID of the part role.
    /// </summary>
    public string? PartRoleId { get; set; }
}
