using Cadmus.Core;
using Cadmus.Geo.Parts;
using Cadmus.Refs.Bricks;
using Cadmus.Seed.Tes.Parts;
using Fusi.Antiquity.Chronology;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Parts.Test;

public class SiteResourcesPartTest
{
    private static SiteResourcesPart GetPart()
    {
        SiteResourcesPartSeeder seeder = new();
        IItem item = new Item
        {
            FacetId = "default",
            CreatorId = "zeus",
            UserId = "zeus",
            Description = "Test item",
            Title = "Test Item",
            SortKey = ""
        };
        return (SiteResourcesPart)seeder.GetPart(item, null, null)!;
    }

    private static SiteResourcesPart GetEmptyPart()
    {
        return new SiteResourcesPart
        {
            ItemId = Guid.NewGuid().ToString(),
            RoleId = "some-role",
            CreatorId = "zeus",
            UserId = "another",
        };
    }

    [Fact]
    public void Part_Is_Serializable()
    {
        SiteResourcesPart part = GetPart();

        string json = TestHelper.SerializePart(part);
        SiteResourcesPart part2 =
            TestHelper.DeserializePart<SiteResourcesPart>(json)!;

        Assert.Equal(part.Id, part2.Id);
        Assert.Equal(part.TypeId, part2.TypeId);
        Assert.Equal(part.ItemId, part2.ItemId);
        Assert.Equal(part.RoleId, part2.RoleId);
        Assert.Equal(part.CreatorId, part2.CreatorId);
        Assert.Equal(part.UserId, part2.UserId);

        Assert.Equal(part.Resources.Count, part2.Resources.Count);
    }

    [Fact]
    public void GetDataPins_NoEntries_Ok()
    {
        SiteResourcesPart part = GetPart();
        part.Resources.Clear();

        List<DataPin> pins = [.. part.GetDataPins(null)];

        Assert.Single(pins);
        DataPin pin = pins[0];
        Assert.Equal("tot-count", pin.Name);
        TestHelper.AssertPinIds(part, pin);
        Assert.Equal("0", pin.Value);
    }

    [Fact]
    public void GetDataPins_Entries_Ok()
    {
        SiteResourcesPart part = GetEmptyPart();

        for (int n = 1; n <= 3; n++)
        {
            bool odd = (n & 1) == 1;
            part.Resources.Add(new SiteResource
            {
                Eid = $"r{n}",
                Type = odd ? "t-odd" : "t-even",
                Features = [odd ? "f-odd" : "f-even"]
            });
            if (n == 1)
            {
                part.Resources[0].Location = new AssertedLocation
                {
                    Value = new GeoLocation
                    {
                        Eid = "loc1",
                        Latitude = 12.34,
                        Longitude = 56.78,
                        Label = "Location 1"
                    }
                };
                part.Resources[0].Dates =
                [
                    new AssertedHistoricalDate(
                        HistoricalDate.Parse("100 BC")!)
                ];
            }
        }

        List<DataPin> pins = [.. part.GetDataPins(null)];

        Assert.Equal(11, pins.Count);

        DataPin? pin = pins.Find(p => p.Name == "tot-count");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);
        Assert.Equal("3", pin!.Value);

        // eid's
        for (int n = 1; n <= 3; n++)
        {
            pin = pins.Find(p => p.Name == "eid" && p.Value == $"r{n}");
            Assert.NotNull(pin);
            TestHelper.AssertPinIds(part, pin!);
        }

        // types
        pin = pins.Find(p => p.Name == "type" && p.Value == "t-odd");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        pin = pins.Find(p => p.Name == "type" && p.Value == "t-even");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        // features
        pin = pins.Find(p => p.Name == "feature" && p.Value == "f-odd");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        pin = pins.Find(p => p.Name == "feature" && p.Value == "f-even");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        // loc-eid
        pin = pins.Find(p => p.Name == "loc-eid" && p.Value == "loc1");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        // loc-label
        pin = pins.Find(p => p.Name == "loc-label" && p.Value == "Location 1");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);

        // date-value
        pin = pins.Find(p => p.Name == "date-value" && p.Value == "-100");
        Assert.NotNull(pin);
        TestHelper.AssertPinIds(part, pin!);
    }
}
