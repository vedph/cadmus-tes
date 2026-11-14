using Cadmus.Core;
using Cadmus.Import.Excel;
using Cadmus.Import.Proteus;
using Cadmus.Tes.Import.Resource;
using Cadmus.Tes.Parts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Tes.Import.Test;

public sealed class ResourceRowEntryRegionParserTest
{
    [Fact]
    public async Task Parse_AddsItemWithResourcesPartAndResource()
    {
        ResourceRowEntryRegionParser parser = new();
        parser.Configure(new ResourceRowEntryRegionParserOptions
        {
            PartRoleId = "site-mark",
            ResourceType = "funerary-evidence"
        });
        CadmusEntrySetContext context = TestHelper.CreateContext(false);

        await TestHelper.ParseRowAsync(parser, context);

        IItem item = Assert.Single(context.Items);
        Assert.Equal("resource", item.FacetId);
        SiteResourcesPart part = item.Parts.OfType<SiteResourcesPart>().Single();
        Assert.Equal("site-mark", part.RoleId);
        Assert.Equal("funerary-evidence", Assert.Single(part.Resources).Type);
    }
}

public sealed class ColResPropertyEntryRegionParserTest
{
    private static ColResPropertyEntryRegionParser GetParser(
        ResourceProperty property, string? thesaurusId = "res@en")
    {
        ColResPropertyEntryRegionParser parser = new();
        parser.Configure(new ColResPropertyEntryRegionParserOptions
        {
            Tag = "col-x",
            ResourceRoleId = "site-res",
            Property = property,
            ThesaurusId = thesaurusId
        });
        return parser;
    }

    [Fact]
    public async Task Parse_Eid_SetsEid()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnAsync(GetParser(ResourceProperty.Eid, null),
            context, "col-x", "R1");

        Assert.Equal("R1", TestHelper.GetResource(context).Eid);
    }

    [Fact]
    public async Task Parse_TypeAndTag_MapLabels()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnAsync(GetParser(ResourceProperty.Type),
            context, "col-x", "Mine");
        await TestHelper.ParseColumnAsync(GetParser(ResourceProperty.Tag),
            context, "col-x", "high accuracy");

        SiteResource resource = TestHelper.GetResource(context);
        Assert.Equal("mine", resource.Type);
        Assert.Equal("high", resource.Tag);
    }

    [Fact]
    public async Task Parse_Features_MapsDistinctLabels()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnAsync(GetParser(ResourceProperty.Features),
            context, "col-x", "mine; quarry;mine");

        Assert.Equal(["mine", "quarry"], TestHelper.GetResource(context).Features);
    }

    [Fact]
    public async Task Parse_NoThesaurus_Ignored()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnAsync(GetParser(ResourceProperty.Type, null),
            context, "col-x", "mine");

        Assert.Equal("", TestHelper.GetResource(context).Type);
    }
}

public sealed class ColResNoteEntryRegionParserTest
{
    private static ColResNoteEntryRegionParser GetParser(string tag,
        string heading)
    {
        ColResNoteEntryRegionParser parser = new();
        parser.Configure(new ColResNoteEntryRegionParserOptions
        {
            Tag = tag,
            ResourceRoleId = "site-res",
            Heading = heading
        });
        return parser;
    }

    [Fact]
    public async Task Parse_AppendsToResourceNote()
    {
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnAsync(GetParser("col-a", "Evidence Note"),
            context, "col-a", " first\nline ");
        await TestHelper.ParseColumnAsync(GetParser("col-b", "Certainty Note"),
            context, "col-b", "second");

        Assert.Equal("### Evidence Note\n\nfirst\nline\n\n" +
            "### Certainty Note\n\nsecond", TestHelper.GetResource(context).Note);
    }

    [Fact]
    public async Task Parse_NoResource_Ignored()
    {
        CadmusEntrySetContext context = TestHelper.CreateContext();

        await TestHelper.ParseColumnAsync(GetParser("col-a", "H"),
            context, "col-a", "x");

        Assert.Empty(context.CurrentItem!.Parts);
    }
}

public sealed class ColResFlagsEntryRegionParserTest
{
    private static ColResFlagsEntryRegionParser GetParser()
    {
        ColResFlagsEntryRegionParser parser = new();
        parser.Configure(new ColResFlagsEntryRegionParserOptions
        {
            ResourceRoleId = "site-mark",
            Flags = new Dictionary<string, string>
            {
                ["col-jewelry"] = "jewelry",
                ["col-exotic_good"] = "exotic-good"
            }
        });
        return parser;
    }

    [Fact]
    public async Task Parse_Yes_AddsFeatures()
    {
        CadmusEntrySetContext context =
            TestHelper.CreateResourceContext("site-mark");

        await TestHelper.ParseColumnsAsync(GetParser(), context,
            ("col-jewelry", "YES"), ("col-exotic_good", "NO"));

        Assert.Equal(["jewelry"], TestHelper.GetResource(context).Features);
    }

    [Fact]
    public async Task Parse_NoYes_NoFeatures()
    {
        CadmusEntrySetContext context =
            TestHelper.CreateResourceContext("site-mark");

        await TestHelper.ParseColumnsAsync(GetParser(), context,
            ("col-jewelry", null), ("col-exotic_good", "maybe"));

        Assert.Null(TestHelper.GetResource(context).Features);
    }
}

public sealed class ColResPeriodsEntryRegionParserTest
{
    [Fact]
    public async Task Parse_Yes_AddsDatesToResource()
    {
        ColResPeriodsEntryRegionParser parser = new();
        parser.Configure(new ColResPeriodsEntryRegionParserOptions
        {
            ResourceRoleId = "site-res",
            PeriodCount = 3
        });
        Assert.Equal(["col-period_1", "col-period_2", "col-period_3"],
            parser.RegionTags);
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();

        await TestHelper.ParseColumnsAsync(parser, context,
            ("col-period_1", "YES"), ("col-period_2", "YES"),
            ("col-period_3", null));

        SiteResource resource = TestHelper.GetResource(context);
        var date = Assert.Single(resource.Dates!);
        Assert.Equal(-1000, date.A!.Value);
        Assert.Equal(-951, date.B!.Value);
        Assert.True(date.A.IsApproximate);
        // no dates part is added to the item
        Assert.Single(context.CurrentItem!.Parts);
    }
}

public sealed class ColResParentItemIdEntryRegionParserTest
{
    private static ColResParentItemIdEntryRegionParser GetParser()
    {
        ColResParentItemIdEntryRegionParser parser = new();
        parser.Configure(new ColParentItemIdEntryRegionParserOptions
        {
            Tag = "col-site_id",
            ItemIdsSet = "item-ids-s"
        });
        return parser;
    }

    [Fact]
    public async Task Parse_SameParent_AccumulatesResources()
    {
        ColResParentItemIdEntryRegionParser parser = GetParser();
        await parser.StartAsync();

        // row 1
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();
        TestHelper.AddItemIdsSet(context, "S1", "item-ids-s");
        await TestHelper.ParseColumnAsync(parser, context, "col-site_id", "S1");
        TestHelper.GetResource(context).Eid = "R1";
        SiteResourcesPart first = context.CurrentItem!.Parts
            .OfType<SiteResourcesPart>().Single();
        Assert.Equal("item-1", context.CurrentItem.Id);
        Assert.Equal("Parent", context.CurrentItem.Title);

        // row 2
        context = TestHelper.CreateResourceContext();
        TestHelper.AddItemIdsSet(context, "S1", "item-ids-s");
        await TestHelper.ParseColumnAsync(parser, context, "col-site_id", "S1");
        // the current resource is still the one of this row
        TestHelper.GetResource(context).Eid = "R2";

        SiteResourcesPart second = context.CurrentItem!.Parts
            .OfType<SiteResourcesPart>().Single();
        Assert.Same(first, second);
        Assert.Equal(["R1", "R2"], second.Resources.Select(r => r.Eid));
    }

    [Fact]
    public async Task Parse_UnknownParent_SkipsRow()
    {
        ColResParentItemIdEntryRegionParser parser = GetParser();
        CadmusEntrySetContext context = TestHelper.CreateResourceContext();
        TestHelper.AddItemIdsSet(context, "S1", "item-ids-s");

        await TestHelper.ParseColumnAsync(parser, context, "col-site_id", "S9");

        Assert.Empty(context.Items);
        Assert.True(ImportHelper.IsRowSkipped(context));
    }
}
