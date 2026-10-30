using Cadmus.Core;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column parent item ID entry region parser. This column contains the EID
/// of the parent item of the row being imported, which was imported in a
/// previous pass. This EID is used to fetch the corresponding item ID from the
/// loaded item IDs set, so that the item being created for this row (and all
/// its parts) gets the same ID of the parent item. When exporting items into
/// the database with merging enabled, this will merge the parts of the row
/// item into the parent item.
/// <para>As merging replaces parts with the same type and role, when several
/// rows target the same parent, their <see cref="SiteResourcesPart"/>'s are
/// accumulated into a single part, so that each row adds its resource to the
/// resources of the previous rows.</para>
/// <para>If the parent is not found, an error is logged and the row is
/// skipped.</para>
/// <para>Tag: <c>entry-region-parser.tes.col-parent-item-id</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-parent-item-id")]
public sealed class ColParentItemIdEntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser, IConfigurable<ColParentItemIdEntryRegionParserOptions>
{
    // resources parts accumulated across rows, keyed by item ID + role ID
    private readonly Dictionary<string, SiteResourcesPart> _resParts = [];
    private ColParentItemIdEntryRegionParserOptions? _options;

    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => _options?.Tag != null ? [_options.Tag] : [];

    /// <summary>
    /// Configure this parser.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <exception cref="ArgumentNullException">Null options.</exception>
    public void Configure(ColParentItemIdEntryRegionParserOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        ConfigureBaseOptions(options);
    }

    /// <summary>
    /// Starts the parser.
    /// </summary>
    public override Task StartAsync()
    {
        _resParts.Clear();
        return base.StartAsync();
    }

    private void AccumulateResources(IItem item)
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

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        // there must be a value, else it's an error
        if (!CheckRequired(region, value))
        {
            ImportHelper.SkipRow(context);
            return;
        }

        ExportedItemIdSet? set = ImportHelper.TryGetItemIdsSet(context,
            _options!.ItemIdsSet);
        ExportedItemId? id = set != null
            ? ImportHelper.FindItemId(set, value!, Logger)
            : null;

        if (id == null)
        {
            Logger?.LogError("{Tag} column with unknown EID {Eid} at region " +
                "{Region}: row skipped", region.Tag, value, region);
            ImportHelper.SkipRow(context);
            return;
        }

        // set the parent item ID and title
        IItem item = context.CurrentItem!;
        ImportHelper.SetItemId(item, id.Id);
        item.Title = id.Title;
        AccumulateResources(item);
    }
}

/// <summary>
/// Options for <see cref="ColParentItemIdEntryRegionParser"/>.
/// </summary>
public class ColParentItemIdEntryRegionParserOptions : ColEntryRegionParserOptions
{
    /// <summary>
    /// Gets or sets the name of the item IDs set to look up, i.e. the name of
    /// the file it was loaded from without extension (e.g. <c>item-ids-s</c>).
    /// When null, the merged set of all the loaded sets is used.
    /// </summary>
    public string? ItemIdsSet { get; set; }
}
