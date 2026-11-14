using Cadmus.Core;
using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Parts;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Test;

/// <summary>
/// Helpers for testing TES column and row entry region parsers.
/// </summary>
internal static class TestHelper
{
    public const string THESAURI = """
    [
      {
        "id": "epi-support-materials@en",
        "entries": [
          { "id": "marble", "value": "marble" },
          { "id": "stone", "value": "stone" }
        ]
      },
      {
        "id": "epi-support-object-types@en",
        "entries": [
          { "id": "stele", "value": "stele" }
        ]
      },
      {
        "id": "epi-technique-types@en",
        "entries": [
          { "id": "chiselled", "value": "chiselled" },
          { "id": "painted", "value": "painted" }
        ]
      },
      {
        "id": "res@en",
        "entries": [
          { "id": "mine", "value": "mine" },
          { "id": "quarry", "value": "quarry" },
          { "id": "high", "value": "high accuracy" }
        ]
      }
    ]
    """;

    /// <summary>
    /// Creates a context with a thesaurus map and optionally a current item.
    /// </summary>
    /// <param name="withItem">True to add a current item.</param>
    /// <returns>Context.</returns>
    public static CadmusEntrySetContext CreateContext(bool withItem = true)
    {
        ThesaurusEntryMap map = new();
        map.Load(new MemoryStream(Encoding.UTF8.GetBytes(THESAURI)));

        CadmusEntrySetContext context = new() { ThesaurusEntryMap = map };
        if (withItem)
        {
            context.Items.Add(new Item
            {
                FacetId = "test",
                Title = "Item title",
                CreatorId = "zeus",
                UserId = "zeus"
            });
        }
        return context;
    }

    /// <summary>
    /// Creates a context with a current item having a resources part with
    /// the specified role and a single resource, as the resource row parser
    /// does.
    /// </summary>
    /// <param name="roleId">The resources part role.</param>
    /// <returns>Context.</returns>
    public static CadmusEntrySetContext CreateResourceContext(
        string roleId = "site-res")
    {
        CadmusEntrySetContext context = CreateContext();
        context.EnsurePartForCurrentItem<SiteResourcesPart>(roleId)
            .Resources.Add(new SiteResource { Type = "" });
        return context;
    }

    /// <summary>
    /// Gets the last resource of the resources part of the current item.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Resource.</returns>
    public static SiteResource GetResource(CadmusEntrySetContext context) =>
        context.CurrentItem!.Parts.OfType<SiteResourcesPart>().Single()
            .Resources[^1];

    /// <summary>
    /// Adds an item IDs set with an item having the specified EID, ID
    /// <c>item-1</c>, metadata part ID <c>md-1</c>, and title <c>Parent</c>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="eid">The EID.</param>
    /// <param name="name">The set name.</param>
    public static void AddItemIdsSet(CadmusEntrySetContext context,
        string eid, string? name = null)
    {
        context.Data[ImportHelper.GetItemIdsKey(name)] = new ExportedItemIdSet(
        [
            new ExportedItemId
            {
                Id = "item-1",
                Title = "Parent",
                MetadataPartId = "md-1",
                Eids = [eid]
            }
        ]);
    }

    /// <summary>
    /// Parses a single column region with the specified tag and value.
    /// </summary>
    /// <param name="parser">The parser.</param>
    /// <param name="context">The context.</param>
    /// <param name="tag">The column region tag.</param>
    /// <param name="value">The cell value.</param>
    /// <returns>The index of the next entry.</returns>
    public static Task<int> ParseColumnAsync(IEntryRegionParser parser,
        CadmusEntrySetContext context, string tag, string? value)
    {
        List<DecodedEntry> entries =
        [
            new DecodedCommandEntry(0, 0, "col-start", "x", "1"),
            new DecodedTextEntry(0, 0, value),
            new DecodedCommandEntry(0, 0, "col-end")
        ];
        EntrySet set = new(entries, context);
        EntryRegion region = new(0,
            new EntryRange(new EntryPoint(0), new EntryPoint(2)), tag);
        return parser.ParseAsync(set, 0, [region], 0);
    }

    /// <summary>
    /// Parses a sequence of columns in the same row.
    /// </summary>
    /// <param name="parser">The parser.</param>
    /// <param name="context">The context.</param>
    /// <param name="columns">The tag and value of each column.</param>
    public static async Task ParseColumnsAsync(IEntryRegionParser parser,
        CadmusEntrySetContext context, params (string Tag, string? Value)[] columns)
    {
        foreach ((string tag, string? value) in columns)
            await ParseColumnAsync(parser, context, tag, value);
    }

    /// <summary>
    /// Parses an entered row region with the specified parser.
    /// </summary>
    /// <param name="parser">The parser.</param>
    /// <param name="context">The context.</param>
    /// <returns>The index of the next entry.</returns>
    public static async Task<int> ParseRowAsync(IEntryRegionParser parser,
        CadmusEntrySetContext context)
    {
        List<DecodedEntry> entries =
        [
            new DecodedCommandEntry(0, 0, "row-start", "y", "1"),
            new DecodedCommandEntry(0, 0, "row-end")
        ];
        EntrySet set = new(entries, context);
        EntryRegion region = new(0,
            new EntryRange(new EntryPoint(0), new EntryPoint(1)), "row");
        await parser.RegionChangedAsync("row", 0, true, context);
        return await parser.ParseAsync(set, 0, [region], 0);
    }
}
