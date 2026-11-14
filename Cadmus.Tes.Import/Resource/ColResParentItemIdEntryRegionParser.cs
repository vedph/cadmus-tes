using Cadmus.Core;
using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Resource;

/// <summary>
/// TES column parent item ID entry region parser for resources. This is a
/// <see cref="ColParentItemIdEntryRegionParser"/> which merges the row into
/// the item of the site whose EID is in the cell. As merging replaces parts
/// with the same type and role, when several rows target the same site their
/// <see cref="SiteResourcesPart"/>'s are accumulated into a single part, so
/// that each row adds its resource to the resources of the previous rows.
/// <para>Tag: <c>entry-region-parser.tes.col-res-parent-item-id</c>.</para>
/// </summary>
/// <seealso cref="ColParentItemIdEntryRegionParser" />
[Tag("entry-region-parser.tes.col-res-parent-item-id")]
public sealed class ColResParentItemIdEntryRegionParser :
    ColParentItemIdEntryRegionParser
{
    // resources parts accumulated across rows, keyed by item ID + role ID
    private readonly Dictionary<string, SiteResourcesPart> _resParts = [];

    /// <summary>
    /// Starts the parser.
    /// </summary>
    public override Task StartAsync()
    {
        _resParts.Clear();
        return base.StartAsync();
    }

    /// <summary>
    /// Accumulates the resources parts of the current item into the parts
    /// of the previous rows targeting the same parent item.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="item">The current item.</param>
    protected override void OnParentItemResolved(CadmusEntrySetContext context,
        IItem item)
    {
        for (int i = 0; i < item.Parts.Count; i++)
        {
            if (item.Parts[i] is not SiteResourcesPart part) continue;

            string key = item.Id + "|" + part.RoleId;
            if (_resParts.TryGetValue(key, out SiteResourcesPart? previous))
            {
                previous.Resources.AddRange(part.Resources);
                item.Parts[i] = previous;
            }
            else
            {
                _resParts[key] = part;
            }
        }
    }
}
