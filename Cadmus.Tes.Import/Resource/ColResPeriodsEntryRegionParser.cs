using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using System.Collections.Generic;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES periods columns entry region parser for resources. This converts
/// the <c>YES</c> cells of the periods columns into approximate date ranges
/// (see <see cref="ColPeriodsEntryRegionParserBase{TOptions}"/>), which are
/// added to the dates of the current resource, i.e. the last resource in the
/// <see cref="SiteResourcesPart"/> with the configured role (see
/// <see cref="ResourceRowEntryRegionParser"/>).
/// <para>Tag: <c>entry-region-parser.tes.col-res-periods</c>.</para>
/// </summary>
[Tag("entry-region-parser.tes.col-res-periods")]
public sealed class ColResPeriodsEntryRegionParser :
    ColPeriodsEntryRegionParserBase<ColResPeriodsEntryRegionParserOptions>
{
    /// <summary>
    /// Adds the specified dates to the current resource.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The region of the last period column.</param>
    /// <param name="dates">The dates to add.</param>
    protected override void AddDates(CadmusEntrySetContext context,
        EntryRegion region, List<AssertedHistoricalDate> dates)
    {
        SiteResource? resource = TesImportHelper.GetCurrentResource(context,
            Options.ResourceRoleId, region, Logger);
        if (resource == null) return;

        resource.Dates ??= [];
        resource.Dates.AddRange(dates);
    }
}

/// <summary>
/// Options for <see cref="ColResPeriodsEntryRegionParser"/>.
/// </summary>
public class ColResPeriodsEntryRegionParserOptions :
    ColPeriodsEntryRegionParserOptions
{
    /// <summary>
    /// Gets or sets the role ID of the target resources part (e.g.
    /// <c>site-res</c>).
    /// </summary>
    public string? ResourceRoleId { get; set; }
}
