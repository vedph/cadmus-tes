using Cadmus.Import.Proteus;
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
/// adds a new item to it, with a <see cref="SiteResourcesPart"/> having
/// the configured role ID and a new resource for the row. Column parsers then
/// fill this resource. This is used for all resources in the site, each time
/// with a specific part role ID. The item will then get the ID of its parent
/// site (see <see cref="ColParentItemIdEntryRegionParser"/>).
/// <para>Tag: <c>entry-region-parser.tes.row-resource</c>.</para>
/// </summary>
/// <seealso cref="RowEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.row-resource")]
public sealed class ResourceRowEntryRegionParser :
    RowEntryRegionParser, IEntryRegionParser,
    IConfigurable<ResourceRowEntryRegionParserOptions>
{
    private ResourceRowEntryRegionParserOptions _options = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceRowEntryRegionParser"/>
    /// class.
    /// </summary>
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
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="entryRegionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryIndex">The index of the first entry to parse.</param>
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
        int next = ParseRow(entrySet, entryIndex, entryRegions,
            entryRegionIndex);

        // if a new item was created for the row, add the resources part with
        // the configured role ID to it, with a new resource for this row
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        if (next > entryIndex && ctx.CurrentItem != null)
        {
            ctx.EnsurePartForCurrentItem<SiteResourcesPart>(_options.PartRoleId)
                .Resources.Add(new SiteResource
                {
                    Type = _options.ResourceType ?? ""
                });
        }

        return Task.FromResult(next);
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

    /// <summary>
    /// Gets or sets the optional default type ID of each resource. This is
    /// used when all the resources have the same type (e.g.
    /// <c>funerary-evidence</c> for markers), so that there is no type column.
    /// </summary>
    public string? ResourceType { get; set; }
}
