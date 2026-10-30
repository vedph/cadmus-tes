using Cadmus.Core;
using Cadmus.Tes.Parts;
using Fusi.Tools.Configuration;
using System;
using System.Reflection;
using Xunit;

namespace Cadmus.Seed.Tes.Parts.Test;

public sealed class SiteResourcesPartSeederTest
{
    private static readonly PartSeederFactory _factory =
        TestHelper.GetFactory();
    private static readonly SeedOptions _seedOptions =
        _factory.GetSeedOptions();
    private static readonly IItem _item =
        _factory.GetItemSeeder().GetItem(1, "facet");

    [Fact]
    public void TypeHasTagAttribute()
    {
        Type t = typeof(SiteResourcesPartSeeder);
        TagAttribute? attr = t.GetTypeInfo().GetCustomAttribute<TagAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("seed.it.vedph.tes.site-resources", attr!.Tag);
    }

    [Fact]
    public void Seed_Ok()
    {
        SiteResourcesPartSeeder seeder = new();
        seeder.SetSeedOptions(_seedOptions);

        IPart? part = seeder.GetPart(_item, null, _factory);

        Assert.NotNull(part);

        SiteResourcesPart? p = part as SiteResourcesPart;
        Assert.NotNull(p);

        TestHelper.AssertPartMetadata(p!);

        Assert.InRange(p!.Resources.Count, 1, 3);
        foreach (SiteResource resource in p.Resources)
        {
            Assert.False(string.IsNullOrEmpty(resource.Type));
            Assert.NotNull(resource.Location);
            Assert.NotEmpty(resource.Features!);
            Assert.NotEmpty(resource.Dates!);
            Assert.NotEmpty(resource.Counts!);
            Assert.NotEmpty(resource.References!);
        }
    }

    [Fact]
    public void Seed_WithOptions_UsesOptions()
    {
        SiteResourcesPartSeeder seeder = new();
        seeder.SetSeedOptions(_seedOptions);
        seeder.Configure(new SiteResourcesPartSeederOptions
        {
            ResourceTypes = ["clay"],
            ResourceTags = ["survey"],
            ResourceFeatures = ["slag"],
            CountIds = ["blocks"],
            CountTags = ["estimated"]
        });

        SiteResourcesPart? p = seeder.GetPart(_item, "site-res", _factory)
            as SiteResourcesPart;

        Assert.NotNull(p);
        Assert.Equal("site-res", p!.RoleId);
        foreach (SiteResource resource in p.Resources)
        {
            Assert.Equal("clay", resource.Type);
            Assert.Equal("survey", resource.Tag);
            Assert.Equal(["slag"], resource.Features!);
            Assert.All(resource.Counts!, c =>
            {
                Assert.Equal("blocks", c.Id);
                Assert.Equal("estimated", c.Tag);
            });
        }
    }
}
