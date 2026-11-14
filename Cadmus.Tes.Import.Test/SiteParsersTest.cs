using Cadmus.Core;
using Cadmus.General.Parts;
using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Import.Site;
using Cadmus.Tes.Parts;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Test;

public sealed class ColCoordsEntryRegionParserTest
{
    private static ColCoordsEntryRegionParser GetParser(string? roleId = null)
    {
        ColCoordsEntryRegionParser parser = new();
        parser.Configure(new ColCoordsEntryRegionParserOptions
        {
            ResourceRoleId = roleId
        });
        return parser;
    }

    [Fact]
    public async Task Parse_Coords_AddsLocationWithRank()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(GetParser(), context,
            ("col-ycoord", "37,07"), ("col-xcoord", "15.28"),
            ("col-coord_certainty", "medium"));

        AssertedLocation loc = context.CurrentItem!.Parts
            .OfType<AssertedLocationsPart>().Single().Locations.Single();
        Assert.Equal("Item title", loc.Value!.Label);
        Assert.Equal(37.07, loc.Value.Latitude, 5);
        Assert.Equal(15.28, loc.Value.Longitude, 5);
        Assert.Equal(2, loc.Assertion!.Rank);
    }

    [Fact]
    public async Task Parse_MissingX_NoLocation()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(GetParser(), context,
            ("col-ycoord", "37"), ("col-xcoord", null),
            ("col-coord_certainty", "high"));

        Assert.Empty(context.CurrentItem!.Parts);
    }

    [Fact]
    public async Task Parse_Resource_SetsResourceLocation()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();
        TestHelper.GetResource(context).Eid = "R1";

        await TestHelper.ParseColumnsAsync(GetParser("site-res"), context,
            ("col-ycoord", "37"), ("col-xcoord", "15"),
            ("col-coord_certainty", "low"));

        AssertedLocation loc = TestHelper.GetResource(context).Location!;
        Assert.Equal("R1", loc.Value!.Label);
        Assert.Equal(3, loc.Assertion!.Rank);
        Assert.DoesNotContain(context.CurrentItem!.Parts,
            p => p is AssertedLocationsPart);
    }
}

public sealed class SiteRowEntryRegionParserTest
{
    [Fact]
    public async Task Parse_AddsSiteItem()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext(false);

        await TestHelper.ParseRowAsync(new SiteRowEntryRegionParser(), context);

        IItem item = Assert.Single(context.Items);
        Assert.Equal("site", item.FacetId);
    }
}

public sealed class ColSiteNameEntryRegionParserTest
{
    [Fact]
    public async Task Parse_SetsTitleAndToponym()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColSiteNameEntryRegionParser(),
            context, "col-site_name", "Morgantina");

        Assert.Equal("Morgantina", context.CurrentItem!.Title);
        ProperNamePiece piece = context.CurrentItem.Parts
            .OfType<AssertedToponymsPart>().Single()
            .Toponyms.Single().Name!.Pieces!.Single();
        Assert.Equal("name", piece.Type);
        Assert.Equal("Morgantina", piece.Value);
    }

    [Fact]
    public async Task Parse_Empty_NoChange()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColSiteNameEntryRegionParser(),
            context, "col-site_name", null);

        Assert.Equal("Item title", context.CurrentItem!.Title);
        Assert.Empty(context.CurrentItem.Parts);
    }
}

public sealed class ColSitePleiadesEntryRegionParserTest
{
    [Fact]
    public async Task Parse_Digits_AddsLink()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColSitePleiadesEntryRegionParser(),
            context, "col-pleiades_ref", "462492");

        AssertedCompositeId link = context.CurrentItem!.Parts
            .OfType<PinLinksPart>().Single().Links.Single();
        Assert.Equal("pleiades", link.Scope);
        Assert.Equal("462492", link.Target!.Gid);
        Assert.Equal("Item title", link.Target.Label);
    }

    [Fact]
    public async Task Parse_NotDigits_AddsMetadatum()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColSitePleiadesEntryRegionParser(),
            context, "col-pleiades_ref", "near Gela");

        Metadatum m = context.CurrentItem!.Parts.OfType<MetadataPart>()
            .Single().Metadata.Single();
        Assert.Equal("pleiades_label", m.Name);
        Assert.Equal("near Gela", m.Value);
    }
}

public sealed class ColSiteGeonamesEntryRegionParserTest
{
    [Fact]
    public async Task Parse_AddsLink()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColSiteGeonamesEntryRegionParser(),
            context, "col-geonames_ref", "2523920");

        AssertedCompositeId link = context.CurrentItem!.Parts
            .OfType<PinLinksPart>().Single().Links.Single();
        Assert.Equal("geonames", link.Scope);
        Assert.Equal("2523920", link.Target!.Gid);
        Assert.Equal("Item title", link.Target.Label);
    }
}
