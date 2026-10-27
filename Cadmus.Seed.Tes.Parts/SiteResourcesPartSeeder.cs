using Bogus;
using Cadmus.Core;
using Cadmus.Geo.Parts;
using Cadmus.Refs.Bricks;
using Cadmus.Tes.Parts;
using Fusi.Antiquity.Chronology;
using Fusi.Tools.Configuration;
using System;
using System.Collections.Generic;

namespace Cadmus.Seed.Tes.Parts;

/// <summary>
/// Seeder for <see cref="SiteResourcesPart"/>.
/// Tag: <c>seed.it.vedph.tes.site-resources</c>.
/// </summary>
/// <seealso cref="PartSeederBase" />
[Tag("seed.it.vedph.tes.site-resources")]
public sealed class SiteResourcesPartSeeder : PartSeederBase,
    IConfigurable<SiteResourcesPartSeederOptions>
{
    private SiteResourcesPartSeederOptions? _options;

    /// <summary>
    /// Configures the object with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    public void Configure(SiteResourcesPartSeederOptions options)
    {
        _options = options;
    }

    private static AssertedHistoricalDate GetAssertedHistoricalDate(Faker f)
    {
        return new AssertedHistoricalDate(
            HistoricalDate.Parse($"{f.Random.Number(1, 500)}")!);
    }

    private static AssertedLocation GetAssertedLocation()
    {
        return new Faker<AssertedLocation>()
            .RuleFor(l => l.Tag, f => f.Random.Word())
            .RuleFor(l => l.Value, f =>
                new Faker<GeoLocation>()
                    .RuleFor(g => g.Label, f => f.Address.City())
                    .RuleFor(g => g.Latitude, f => f.Address.Latitude())
                    .RuleFor(g => g.Longitude, f => f.Address.Longitude())
                    .Generate())
            .Generate();
    }

    private List<DecoratedCount> GetCounts()
    {
        return
        [
            new()
            {
                Id = _options?.CountIds?.Count > 0
                    ? new Faker().PickRandom(_options.CountIds)
                    : Guid.NewGuid().ToString(),
                Tag = _options?.CountTags?.Count > 0
                    ? new Faker().PickRandom(_options.CountTags)
                    : null,
                Value = new Faker().Random.Number(1, 10)
            }
        ];
    }

    /// <summary>
    /// Gets a random number of document references.
    /// </summary>
    /// <param name="min">The min number of references to get.</param>
    /// <param name="max">The max number of references to get.</param>
    /// <returns>References.</returns>
    private static List<DocReference> GetDocReferences(int min, int max)
    {
        List<DocReference> refs = [];

        for (int n = 1; n <= Randomizer.Seed.Next(min, max + 1); n++)
        {
            refs.Add(new Faker<DocReference>()
                .RuleFor(r => r.Type, "book")
                .RuleFor(r => r.Tag, f => f.PickRandom(null, "tag"))
                .RuleFor(r => r.Type, "biblio")
                .RuleFor(r => r.Citation,
                    f => f.Person.LastName + " " + f.Date.Past(10).Year)
                .RuleFor(r => r.Note, f => f.Lorem.Sentence())
                .Generate());
        }

        return refs;
    }

    private List<SiteResource> GetResources(int count, Faker f)
    {
        List<SiteResource> resources = new(count);

        for (int i = 0; i < count; i++)
        {
            resources.Add(new SiteResource
            {
                Type = f.PickRandom(_options?.ResourceTypes
                    ?? ["quarry", "mine", "market"]),
                Tag = _options?.ResourceTags?.Count > 0
                    ? f.PickRandom(_options?.ResourceTags)
                    : f.Random.Word(),
                Features = [f.PickRandom(_options?.ResourceFeatures
                    ?? ["alpha", "beta", "gamma"])],
                Location = GetAssertedLocation(),
                Dates = [GetAssertedHistoricalDate(f)],
                Counts = GetCounts(),
                References = GetDocReferences(1, 3),
            });
        }

        return resources;
    }

    /// <summary>
    /// Creates and seeds a new part.
    /// </summary>
    /// <param name="item">The item this part should belong to.</param>
    /// <param name="roleId">The optional part role ID.</param>
    /// <param name="factory">The part seeder factory. This is used
    /// for layer parts, which need to seed a set of fragments.</param>
    /// <returns>A new part or null.</returns>
    /// <exception cref="ArgumentNullException">item or factory</exception>
    public override IPart? GetPart(IItem item, string? roleId,
        PartSeederFactory? factory)
    {
        ArgumentNullException.ThrowIfNull(item);

        SiteResourcesPart part = new Faker<SiteResourcesPart>()
           .RuleFor(p => p.Resources, f => GetResources(f.Random.Number(1, 3), f))
           .Generate();

        SetPartMetadata(part, roleId, item);

        return part;
    }
}

/// <summary>
/// Options for <see cref="SiteResourcesPartSeeder"/>.
/// </summary>
public sealed class SiteResourcesPartSeederOptions
{
    /// <summary>
    /// Resource type IDs to pick from.
    /// </summary>
    public IList<string>? ResourceTypes { get; set; }

    /// <summary>
    /// Resource tag IDs to pick from.
    /// </summary>
    public IList<string>? ResourceTags { get; set; }

    /// <summary>
    /// Resource feature IDs to pick from.
    /// </summary>
    public IList<string>? ResourceFeatures { get; set; }

    /// <summary>
    /// Resource count IDs to pick from.
    /// </summary>
    public IList<string>? CountIds { get; set; }

    /// <summary>
    /// Resource count tags to pick from.
    /// </summary>
    public IList<string>? CountTags { get; set; }
}
