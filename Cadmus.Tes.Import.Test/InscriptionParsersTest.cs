using Cadmus.Core;
using Cadmus.Epigraphy.Parts;
using Cadmus.General.Parts;
using Cadmus.Geo.Parts;
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Import.Inscription;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Test;

public sealed class InscriptionRowEntryRegionParserTest
{
    [Fact]
    public async Task Parse_AddsInscriptionItem()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext(false);

        await TestHelper.ParseRowAsync(new InscriptionRowEntryRegionParser(),
            context);

        IItem item = Assert.Single(context.Items);
        Assert.Equal("inscription", item.FacetId);
    }
}

public sealed class ColDateEntryRegionParserTest
{
    private static async Task<AssertedHistoricalDatesPart?> ParseAsync(
        string? a, string? b)
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();
        await TestHelper.ParseColumnsAsync(new ColDateEntryRegionParser(),
            context, ("col-date_notbefore", a), ("col-date_notafter", b));
        return context.CurrentItem!.Parts.OfType<AssertedHistoricalDatesPart>()
            .FirstOrDefault();
    }

    [Fact]
    public async Task Parse_Range_AddsRange()
    {
        AssertedHistoricalDate date = (await ParseAsync("-100", "50"))!
            .Dates.Single();
        Assert.Equal(-100, date.A!.Value);
        Assert.Equal(50, date.B!.Value);
    }

    [Fact]
    public async Task Parse_Same_AddsPoint()
    {
        AssertedHistoricalDate date = (await ParseAsync("-20", "-20"))!
            .Dates.Single();
        Assert.Equal(-20, date.A!.Value);
        Assert.Null(date.B);
    }

    [Fact]
    public async Task Parse_None_NoPart()
    {
        Assert.Null(await ParseAsync(null, null));
    }
}

public sealed class ColPlaceLinksEntryRegionParserTest
{
    private static async Task<PinLinksPart?> ParseAsync(string? ancient,
        string? modern, string? pleiades)
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();
        await TestHelper.ParseColumnsAsync(new ColPlaceLinksEntryRegionParser(),
            context,
            ("col-site_of_origin_(ancient_name)", ancient),
            ("col-site_of_origin_(modern_name)", modern),
            ("col-pleiades_id", pleiades));
        return context.CurrentItem!.Parts.OfType<PinLinksPart>()
            .FirstOrDefault();
    }

    [Fact]
    public async Task Parse_Pleiades_AddsLinkPerName()
    {
        PinLinksPart part = (await ParseAsync("Syracusae", "Siracusa",
            "places/579570"))!;

        Assert.Equal(2, part.Links.Count);
        Assert.All(part.Links, l =>
        {
            Assert.Equal("pleiades", l.Scope);
            Assert.Equal("origin", l.Tag);
            Assert.Equal("places/579570", l.Target!.Gid);
        });
        Assert.Equal(["Syracusae", "Siracusa"],
            part.Links.Select(l => l.Target!.Label));
    }

    [Fact]
    public async Task Parse_NoPleiades_AddsToponymLinks()
    {
        PinLinksPart part = (await ParseAsync("Syracusae", "Siracusa", null))!;

        Assert.Equal(["toponym-ancient", "toponym-modern"],
            part.Links.Select(l => l.Scope));
        Assert.Equal(["Syracusae", "Siracusa"],
            part.Links.Select(l => l.Target!.Gid));
    }

    [Fact]
    public async Task Parse_Nothing_NoPart()
    {
        Assert.Null(await ParseAsync(null, null, null));
    }
}

public sealed class InscriptionColCoordsEntryRegionParserTest
{
    [Fact]
    public async Task Parse_OriginAndProvenance_AddsTwoLocations()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();
        context.Data[ColPlaceLinksEntryRegionParser.ANCIENT_KEY] = "Syracusae";

        await TestHelper.ParseColumnsAsync(
            new Cadmus.Tes.Import.Inscription.ColCoordsEntryRegionParser(),
            context,
            ("col-origin_latitude", "37.07"), ("col-origin_longitude", "15,28"),
            ("col-provenance_latitude", "38"), ("col-provenance_longitude", "13"));

        AssertedLocation[] locs = [.. context.CurrentItem!.Parts
            .OfType<AssertedLocationsPart>().Single().Locations];
        Assert.Equal(2, locs.Length);
        Assert.Equal("origin", locs[0].Tag);
        Assert.Equal("Syracusae", locs[0].Value!.Label);
        Assert.Equal(15.28, locs[0].Value!.Longitude, 5);
        Assert.Equal("provenance", locs[1].Tag);
        Assert.Equal(38, locs[1].Value!.Latitude, 5);
    }

    [Fact]
    public async Task Parse_NoProvenance_AddsOrigin()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(
            new Cadmus.Tes.Import.Inscription.ColCoordsEntryRegionParser(),
            context,
            ("col-origin_latitude", "37"), ("col-origin_longitude", "15"),
            ("col-provenance_latitude", null), ("col-provenance_longitude", null));

        AssertedLocation loc = context.CurrentItem!.Parts
            .OfType<AssertedLocationsPart>().Single().Locations.Single();
        Assert.Equal("origin", loc.Value!.Label);
    }
}

public sealed class ColSupportEntryRegionParserTest
{
    [Fact]
    public async Task Parse_MaterialAndObjectType_MapsIds()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(new ColSupportEntryRegionParser(),
            context, ("col-material", "Marble"), ("col-object_type", "stele"));

        EpiSupportPart part = context.CurrentItem!.Parts.OfType<EpiSupportPart>()
            .Single();
        Assert.Equal("marble", part.Material);
        Assert.Equal("stele", part.ObjectType);
    }

    [Fact]
    public async Task Parse_NoObjectType_NoObjectType()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(new ColSupportEntryRegionParser(),
            context, ("col-material", "stone"), ("col-object_type", "N/A"));

        EpiSupportPart part = context.CurrentItem!.Parts.OfType<EpiSupportPart>()
            .Single();
        Assert.Equal("stone", part.Material);
        Assert.Null(part.ObjectType);
    }
}

public sealed class ColTechniqueEntryRegionParserTest
{
    [Fact]
    public async Task Parse_TwoColumns_AddsDistinctTechniques()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnsAsync(new ColTechniqueEntryRegionParser(),
            context, ("col-execution_type_1", "chiselled"),
            ("col-execution_type_2", "painted"),
            ("col-execution_type_1", "chiselled"));

        Assert.Equal(["chiselled", "painted"], context.CurrentItem!.Parts
            .OfType<EpiTechniquePart>().Single().Techniques);
    }
}

public sealed class ColTextEntryRegionParserTest
{
    [Fact]
    public async Task Parse_Lines_AddsTextLines()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(new ColTextEntryRegionParser(),
            context, "col-edition_(interpretive)", " Θεοῖς\r\n  Διὶ  σωτῆρι ");

        TokenTextPart part = context.CurrentItem!.Parts.OfType<TokenTextPart>()
            .Single();
        Assert.Equal("base-text", part.RoleId);
        Assert.Equal([1, 2], part.Lines.Select(l => (int)l.Y));
        Assert.Equal(["Θεοῖς", "Διὶ  σωτῆρι"], part.Lines.Select(l => l.Text));
    }
}
