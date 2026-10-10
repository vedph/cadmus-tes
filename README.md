# Cadmus TES

Backend for the Cadmus TES project.

🐋 Quick Docker image build:

```sh
docker buildx create --use

docker buildx build . --platform linux/amd64,linux/arm64,windows/amd64 -t vedph2020/cadmus-tes-api:0.0.4 -t vedph2020/cadmus-tes-api:latest --push
```

(replace with the current version).

## TES Parts

### SiteResourcesPart

This part lists the resources present in a site. Each resource can contain tag, type, any number of features, location, date, counts, and a free note.

- `resources` (`SiteResource[]`):
  - `eid` (`string`): the resource's entity identifier.
  - `type`\* (`string`, 📚 `site-resource-types`): the type of the resource, e.g. "quarry", "mine", "water source", etc.
  - `tag` (`string`, 📚 `site-resource-tags`): an optional tag or label associated with the object, e.g. "cava A", "pozzo B".
  - `features` (`string[]`, 📚 `site-resource-features` hierarchical): the list of features for this resource, including evidences in their own branch.
  - `location` (`AssertedLocation`: see [brick](https://github.com/vedph/cadmus-bricks-shell-v3/blob/master/projects/myrmidon/cadmus-geo-location/README.md) and its [demo](https://cadmus-bricks-v3.fusi-soft.com/geo/geo-location-editor)):
    - `eid` (`string`)
    - `label`\* (`string`)
    - `latitude`\* (`double`)
    - `longitude`\* (`double`)
    - `altitude` (`double`) in mt.
    - `radius` (`double`): uncertainty radius in mt.
    - `geometry` (`string`) in WKT.
  - `date` (`AssertedHistoricalDate`):
    - `tag` (`string` 📚 `asserted-historical-date-tags`)
    - `a`\* (🧱 [Datation](https://github.com/vedph/cadmus-bricks/blob/master/docs/datation.md)):
      - `value`\* (`int`): the numeric value of the point. Its interpretation depends on other points properties: it may represent a year or a century, or a span between two consecutive Gregorian years.
      - `isCentury` (`boolean`): true if value is a century number; false if it's a Gregorian year.
      - `isSpan` (`boolean`): true if the value is the first year of a pair of two consecutive years. This is used for calendars which span across two Gregorian years, e.g. 776/5 BC.
      - `slide` (`int`): a "slide" delta to be added to value. For instance, value=1230 and slide=10 means 1230-1240; this is not a range in the sense of `HistoricalDate` with its A and B points; it's just a relatively undeterminated point, allowed to move between 1230 and 1240. This means that we can still have a range, like A=1230-1240 and B=1290. A slide is represented by the end year/century value prefixed by `:` in its parsable string. So, we can have strings like `1230:1240--1290` for range A=1230-1240 and B=1290, or even `1230:1240--1290:1295`; all combinations are possible. With negative (BC) values we have e.g. `810:805 BC` implying slide=5.
      - `month` (`short`): the month number (1-12) or 0.
      - `day` (`short`): the day number (1-31) or 0.
      - `isApproximate` (`boolean`): true if the point is approximate ("about").
      - `isDubious` (`boolean`): true if the point is dubious ("perhaphs").
      - `hint` (`string`): a short textual hint used to better explain or motivate the datation point.
    - `b` (🧱 [Datation](https://github.com/vedph/cadmus-bricks/blob/master/docs/datation.md))
    - `assertion` (`Assertion`):
      - `tag` (`string`)
      - `rank` (`short`)
      - `references` (🧱 [DocReference[]](https://github.com/vedph/cadmus-bricks/blob/master/docs/doc-reference.md)):
        - `type` (`string` 📚 `doc-reference-types`)
        - `tag` (`string` 📚 `doc-reference-tags`)
        - `citation` (`string`)
        - `note` (`string`)
  - `counts` ([DecoratedCount[]](https://github.com/vedph/cadmus-general/blob/master/docs/decorated-counts.md)): collection of decorated counts associated with the resource, e.g. the estimated income in talents, the number of marble blocks found in a quarry, etc.
    - `id`\* (`string` 📚 `site-resource-count-ids`)
    - `tag` (`string` 📚 `site-resource-count-tags`)
    - `value`\* (`int`)
    - `note` (`string`)
  - `note` (`string`)

## Parts Matrix

| part         | inscription    | lit. source | artifact                 | site                 | cult                           | iconography |
| ------------ | -------------- | ----------- | ------------------------ | -------------------- | ------------------------------ | ----------- |
| categories   | ins-fn ins-lng |             | art-type art-mat art-ctx | site-type site-feats | cult-type cult-gods cult-feats | ico-sub     |
| comment      | X              |             | X                        | X                    | X                              | X           |
| dates        | X              |             | X                        | X                    |                                |             |
| fragments    | X              |             |                          |                      |                                |             |
| links        | X              | X           | X                        | X                    | X                              | X           |
| locations    | X              |             | X                        | X                    |                                |             |
| metadata     | X              | X           | X                        | X                    | X                              | X           |
| support      | X              |             |                          |                      |                                |             |
| techniques   | X              |             |                          |                      |                                |             |
| toponyms     |                |             |                          | X                    |                                |             |
| note         | X transl       | X transl    | X                        | X                    | X                              | X           |
| references   | X              | X           | X                        | X                    | X                              | X           |
| resources    |                |             |                          | site-res site-prod   |                                |             |
| states       | X              |             |                          |                      |                                |             |
| scripts      | X              |             |                          |                      |                                |             |
| signs        | X              |             |                          |                      |                                |             |
| text         | X              | X           |                          |                      |                                |             |
| apparatus=   | X              | X           |                          |                      |                                |             |
| comment=     | X              | X           |                          |                      |                                |             |
| chronology=  | X              | X           |                          |                      |                                |             |
| orthography= | X              |             |                          |                      |                                |             |
| ligatures=   | X              |             |                          |                      |                                |             |
| links=       | X              | X           |                          |                      |                                |             |

## TES Import

The CLI app `tes-tool` in this solution is used to import entities from an Excel file via the Proteus framework. Source Excel files have the following columns (▶️ is the mapping into the above model for inscription items; \* marks a column which is always filled with a value) for each entity type.

To import:

1. fire the API without seeding any items (set item seed count=0 in `appsettings.json`) to create an empty TES MongoDB database.
2. run the import command (change the path to your import file):

```sh
./tes-tool import c:/users/dfusi/desktop/tes/import.json
```

3. index the imported database:

```sh
./tes-tool index cadmus-tes D:/Projects/Cadmus/Tes/CadmusTes/CadmusTesApi/wwwroot/seed-profile.json
```

### Inscription

(1) **A** (`ID`)\*: the ISicily inscription ID (e.g. `ISic000822`) ▶️ `MetadataPart.metadata`: add metadata and set as item's title.

(2) **B** (`Date notBefore`)\*: a numeric value representing a year for the from-date, negative if BC. This is imported together with C.

(3) **C** (`Date notAfter`)\*: a numeric value representing a year for the to-date, negative if BC. ▶️ `AssertedHistoricalDatesPart.dates` as a range B-C.

(4) **D** (`Site of origin (ancient name)`)\*: the ancient name of the site of origin (e.g. `Syracusae`). This is imported together with E and F.

(5) **E** (`Site of origin (modern name)`)\*: the modern name of the site of origin (e.g. `Siracusa`). This is imported together with D and F.

(6) **F** (`Pleiades ID`): the Pleiades ID of the site of origin (e.g. `places/579570`) ▶️ `PinLinksPart.links`: add an external metadata link for the ancient name and another one for the modern name, both referring to the same Pleiades ID.

(7) **G** (`Origin latitude`)\*: latitude. This is imported together with H.

(8) **H** (`Origin longitude`)\*: longitude ▶️ ``AssertedLocationsPart.locations` together with G. G-H are the first location which refers to origin.

(9) **I** (`Provenance latitude`): latitude. This is imported together with J.

(10) **J** (`Provenance longitude`): longitude ▶️ `AssertedLocationsPart.locations` together with I. I-J are the second location which refers to provenance.

(11) **K** (`Material`)\*: ▶️ `EpiSupportPart.material` mapped to thesaurus 📚 `epi-support-materials`.

(12) **L** (`Object type`): when not specified the value is `N/A`. ▶️ `EpiSupportPart.support.objectType` mapped to thesaurus 📚 `epi-support-object-types`.

(13) **M** (`Type`): when not specified the value is `N/A`. ▶️ `CategoriesPart.categories:ins-fn` mapped to thesaurus 📚 `categories_ins-fn`.

(14) **N** (`Execution type 1`): e.g. `chiselled` ▶️ `EpiTechniquePart.techniques` mapped to thesaurus 📚 `epi-technique-types`.

(15) **O** (`Execution type 2`): as for N.

(16) **P** (`Language`)\*: ▶️ `CategoriesPart.categories:ins-lng` mapped to thesaurus 📚 `categories_ins-lng`.

(17) **Q** (`Repository name`): repository name (e.g. `Antiquarium di Megara Hyblaia`) ▶️ `MetadataPart.metadata.preservation-place`.

(18) **R** (`Inventory number`): inventory number (e.g. `104387`) ▶️ `MetadataPart.metadata.inventory-nr`.

(19) **S** (`Edition (interpretive)`): text (Leiden) ▶️ multiline text.

List of target thesauri:

- [categories part](https://github.com/vedph/cadmus-general/blob/master/docs/categories.md):
  - `categories_ins-fn`
  - `categories_ins-lng`
- [epigraphic support part](https://github.com/vedph/cadmus-epigraphy/blob/master/docs/epi-support.md):
  - `epi-support-materials`
  - `epi-support-object-types`
- [epigraphic technique part](https://github.com/vedph/cadmus-epigraphy/blob/master/docs/epi-technique.md):
  - `epi-technique-types`

### Site

(1) **A** (`id`) ❌

(2) **B** (`site_id`)\*: the site ID (e.g. `CASTI_01`) ▶️ `metadata` (`MetadataPart`): add metadata and set as item's title.

(3) **C** (`site_name`)\*: the title (e.g. `Castiglione di Ragusa`) ▶️ `item`.`title` and an entry in `toponyms` (`AssertedToponymsPart`). Currently we can use a single name part with a generic designation (`name`) if we do not want to introduce further structure.

(4) **D** (`archaeological_site_type`): ▶️ `categories` (`CategoriesPart:site-type`); entry value mapped to thesaurus 📚 `categories_site-type`.

(5) **E** (`data_accuracy`) ❌

(6-8) **F** (`ycoord`) + **G** (`xcoord`) + **H** (`coord_certainty`): ▶️ add entry to `GeoAssertedLocations.locations`: `latitude`, `longitude`, `assertion.rank`.

(9) **I** (`pleiades_ref`) ▶️ add link with scope=`pleiades` (`PinLinksPart`); the value here is just the name??.

(10) **J** (`geonames_ref`) ▶️ as `I` for `geonames`; the value here is an URL.

(11) **K** (`cults`) ▶️ binary feature (`CategoriesPart:site-feats`): if `YES` map to thesaurus 📚 `categories_site-feats`.`cults`.

(12) **L** (`resources`): as `K` mapped to `resources`.

(13) **M>** (`storage`) as `K` mapped to `storage`.

(14) **N** (`production`) as `K` mapped to `production`.

(15) **O** (`status_marker`) as `K` mapped to `status-marker`.

(16) **P** (`figurative_representations`) as `K` mapped to `representations-figurative`.

(17) **Q** (`inscriptions`) as `K` mapped to `inscriptions`.

(18-39) **R-AM** (`period_1` to `period_22`): these represent 25-years bins each, starting with -1000: so from -1000+25*(n-1) to -1000+25*n-1. Each bin covered has YES. There can be a single sequence of consecutive bins but also multiple non-consecutive sequences. For each sequence, compute the resulting range as that starting with the bin with the minimum value and ending with the bin with the maximum value. ▶️ `HistoricalDatesPart`, one date per computed range.

(40) **AN** (`source_ids`) ❌

(41) **AO** (`source_locator`) ❌

(42) **AP** (`evidence_note`) ❌

(43) **AQ** (`certainty_note`) ❌

(44) **AR** (`review_status`) ❌

### Cult

(1) **A** (`id`).

(2) **B** (`site_id`).

(3) **C** (`site_name`).

(4) **D** (`data_accuracy`).

(5) **E** (`cult_element_type`).

(6) **F** (`cult_element_type_certainty`).

(7) **G** (`cult_element_id`).

(8) **H** (`ycoord`).

(9) **I** (`xcoord`).

(10) **J** (`coord_certainty`).

(11) **K** (`related_cult_element_id`).

(12) **L** (`relation_type`).

(13) **M>** (`relation_certainty`).

(14) **N** (`divinity`).

(15) **O** (`divinity_certainty`).

(16) **P** (`animal_sacrifices`).

(17) **Q** (`food_consumption`).

(18) **R** (`beverage_consumption`).

(19) **S** (``).

(20) **T** (``).

(21) **U** (``).

(22) **V** (``).

(23) **W** (``).

(24) **X** (``).

(25) **Y** (``).

(26) **Z** (``).

(27) **AA** (``).

(28) **AB** (``).

(29) **AC** (``).

(30) **AD** (``).

(31) **AE** (``).

(32) **AF** (``).

(33) **AG** (``).

(34) **AH** (``).

(35) **AI** (``).

(36) **AJ** (``).

(37) **AK** (``).

(38) **AL** (``).

(39) **AM** (``).

(40) **AN** (``).

(41) **AO** (``).

(42) **AP** (``).

(43) **AQ** (``).

(44) **AR** (``).

(45) **AS** (``).

(46) **AT** (``).

(47) **AU** (``).

(48) **AV** (``).

(49) **AW** (``).

(50) **AX** (``).

(51) **AY** (``).

### Artifact

(1) **A** (``).

(2) **B** (``).

(3) **C** (``).

(4) **D** (``).

(5) **E** (``).

(6) **F** (``).

(7) **G** (``).

(8) **H** (``).

(9) **I** (``).

(10) **J** (``).

(11) **K** (``).

(12) **L** (``).

(13) **M>** (``).

(14) **N** (``).

(15) **O** (``).

(16) **P** (``).

(17) **Q** (``).

(18) **R** (``).

(19) **S** (``).

(20) **T** (``).

(21) **U** (``).

(22) **V** (``).

(23) **W** (``).

(24) **X** (``).

(25) **Y** (``).

(26) **Z** (``).

(27) **AA** (``).

(28) **AB** (``).

(29) **AC** (``).

(30) **AD** (``).

(31) **AE** (``).

(32) **AF** (``).

(33) **AG** (``).

(34) **AH** (``).

(35) **AI** (``).

(36) **AJ** (``).

(37) **AK** (``).

(38) **AL** (``).

(39) **AM** (``).

(40) **AN** (``).

(41) **AO** (``).

(42) **AP** (``).

(43) **AQ** (``).

(44) **AR** (``).

(45) **AS** (``).

(46) **AT** (``).

(47) **AU** (``).

### Code Template

Template for region parser:

- `__TAG__`: the region tag.

```cs
using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser. This targets TODO.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-__TAG__")]
public sealed class Col__TAG__EntryRegionParser :
    EntryRegionParser, IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-__TAG__"];

    /// <summary>
    /// Parses the region of entries at <paramref name="regionIndex" />
    /// in the specified <paramref name="entryRegions" />.
    /// </summary>
    /// <param name="entrySet">The entries set.</param>
    /// <param name="entryRegions">The regions.</param>
    /// <param name="entryRegionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    protected override Task<int> DoParseAsync(EntrySet entrySet, int entryIndex,
        IReadOnlyList<EntryRegion> entryRegions, int entryRegionIndex)
    {
        ArgumentNullException.ThrowIfNull(entrySet);
        ArgumentNullException.ThrowIfNull(entryRegions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)entrySet.Context;
        EntryRegion region = entryRegions[entryRegionIndex];

        if (ctx.CurrentItem == null)
        {
            Logger?.LogError("__TAG__ column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "__TAG__ column without any item at region " + region);
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        // TODO

        return Task.FromResult(entryIndex + 3);
    }   
}
```
