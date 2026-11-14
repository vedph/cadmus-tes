using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System;
using System.Linq;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES-specific import helpers. Generic helpers are in
/// <see cref="Cadmus.Import.Excel.ImportHelper"/>.
/// </summary>
public static class TesImportHelper
{
    /// <summary>
    /// Gets the resource being built for the current row, i.e. the last
    /// resource in the <see cref="SiteResourcesPart"/> with the specified role
    /// in the current item. This resource is added by the row parser
    /// (<see cref="Resource.ResourceRowEntryRegionParser"/>).
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="roleId">The role ID of the resources part.</param>
    /// <param name="region">The region being parsed.</param>
    /// <param name="logger">The optional logger.</param>
    /// <returns>Resource or null if not found.</returns>
    public static SiteResource? GetCurrentResource(CadmusEntrySetContext context,
        string? roleId, EntryRegion region, ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(region);

        SiteResourcesPart? part = context.CurrentItem?.Parts
            .OfType<SiteResourcesPart>()
            .FirstOrDefault(p => roleId == null || p.RoleId == roleId);

        if (part == null || part.Resources.Count == 0)
        {
            logger?.LogError("No resource for {Tag} in resources part " +
                "{RoleId} at region {Region}", region.Tag, roleId, region);
            return null;
        }
        return part.Resources[^1];
    }
}
