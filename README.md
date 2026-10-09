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

(1) **A** (`ID`)\*: the ISicily inscription ID (e.g. `ISic000822`) ▶️ `metadata` (`MetadataPart`): add metadata and set as item's title.

(2) **B** (`Date notBefore`)\*: a numeric value representing a year for the from-date, negative if BC. This is imported together with C.

(3) **C** (`Date notAfter`)\*: a numeric value representing a year for the to-date, negative if BC. ▶️ `dates` (`AssertedHistoricalDatesPart`) as a range B-C.

(4) **D** (`Site of origin (ancient name)`)\*: the ancient name of the site of origin (e.g. `Syracusae`). This is imported together with E and F.

(5) **E** (`Site of origin (modern name)`)\*: the modern name of the site of origin (e.g. `Siracusa`). This is imported together with D and F.

(6) **F** (`Pleiades ID`): the Pleiades ID of the site of origin (e.g. `places/579570`) ▶️ `links` (`PinLinksPart`): add an external metadata link for the ancient name and another one for the modern name, both referring to the same Pleiades ID.

(7) **G** (`Origin latitude`)\*: latitude. This is imported together with H.

(8) **H** (`Origin longitude`)\*: longitude ▶️ `locations` (`AssertedLocationsPart`) together with G. G-H are the first location which refers to origin.

(9) **I** (`Provenance latitude`): latitude. This is imported together with J.

(10) **J** (`Provenance longitude`): longitude ▶️ `locations` (`AssertedLocationsPart`) together with I. I-J are the second location which refers to provenance.

(11) **K** (`Material`)\*: ▶️ `support`.`material` (`EpiSupportPart`) mapped to thesaurus 📚 `epi-support-materials`.

(12) **L** (`Object type`): when not specified the value is `N/A`. ▶️ `support`.`objectType` (`EpiSupportPart`) mapped to thesaurus 📚 `epi-support-object-types`.

(13) **M** (`Type`): when not specified the value is `N/A`. ▶️ `categories:ins-fn` (`CategoriesPart`) mapped to thesaurus 📚 `categories_ins-fn`.

(14) **N** (`Execution type 1`): e.g. `chiselled` ▶️ `technique`.`techniques` (`EpiTechniquePart`) mapped to thesaurus 📚 `epi-technique-types`.

(15) **O** (`Execution type 2`): as for N.

(16) **P** (`Language`)\*: ▶️ `categories:ins-lng` (`CategoriesPart`) mapped to thesaurus 📚 `categories_ins-lng`.

(17) **Q** (`Repository name`): repository name (e.g. `Antiquarium di Megara Hyblaia`) ▶️ `metadata`.`preservation-place` (`MetadataPart`).

(18) **R** (`Inventory number`): inventory number (e.g. `104387`) ▶️ `metadata`.`inventory-nr` (`MetadataPart`).

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

(1) **B** (`ID`)\*: the site ID (e.g. `CASTI_01`) ▶️ `metadata` (`MetadataPart`): add metadata and set as item's title.

(2) **C** (`site_name`): the title (e.g. `Castiglione di Ragusa`).

(3) **D** (`archaeological_site_type`).

(4) **E** (`data_accuracy`).

(5) **F** (`ycoord`).

(6) **G** (`xcoord`).

(7) **H** (`coord_certainty`).

(8) **I** (`pleiades_ref`).

(9) **J** (`geonames_ref`).

(10) **K** (`cults`).

(11) **L** (`resources`).

(12) **M>** (`storage`).

(13) **N** (`production`).

(14) **O** (`status_marker`).

(15) **P** (`figurative_representations`).

(16) **Q** (`inscriptions`).

(17) **R** (`period_1`).

(18) **S** (`period_2`).

(19) **T** (`period_3`).

(20) **U** (`period_4`).

(21) **V** (`period_5`).

(22) **W** (`period_6`).

(23) **X** (`period_7`).

(24) **Y** (`period_8`).

(25) **Z** (`period_9`).

(26) **AA** (`period_10`).

(27) **AB** (`period_11`).

(28) **AC** (`period_12`).

(29) **AD** (`period_13`).

(30) **AE** (`period_14`).

(31) **AF** (`period_15`).

(32) **AG** (`period_16`).

(33) **AH** (`period_17`).

(34) **AI** (`period_18`).

(35) **AJ** (`period_19`).

(36) **AK** (`period_20`).

(37) **AL** (`period_21`).

(38) **AM** (`period_22`).

(39) **AN** (`source_ids`).

(40) **AO** (`source_locator`).

(41) **AP** (`evidence_note`).

(42) **AQ** (`certainty_note`).

(43) **AR** (`review_status`).

### Cult

(1) **B** (``).

(2) **C** (``).

(3) **D** (``).

(4) **E** (``).

(5) **F** (``).

(6) **G** (``).

(7) **H** (``).

(8) **I** (``).

(9) **J** (``).

(10) **K** (``).

(11) **L** (``).

(12) **M>** (``).

(13) **N** (``).

(14) **O** (``).

(15) **P** (``).

(16) **Q** (``).

(17) **R** (``).

(18) **S** (``).

(19) **T** (``).

(20) **U** (``).

(21) **V** (``).

(22) **W** (``).

(23) **X** (``).

(24) **Y** (``).

(25) **Z** (``).

(26) **AA** (``).

(27) **AB** (``).

(28) **AC** (``).

(29) **AD** (``).

(30) **AE** (``).

(31) **AF** (``).

(32) **AG** (``).

(33) **AH** (``).

(34) **AI** (``).

(35) **AJ** (``).

(36) **AK** (``).

(37) **AL** (``).

(38) **AM** (``).

(39) **AN** (``).

(40) **AO** (``).

(41) **AP** (``).

(42) **AQ** (``).

(43) **AR** (``).

(44) **AS** (``).

(45) **AT** (``).

(46) **AU** (``).

(47) **AV** (``).

(48) **AW** (``).

(49) **AX** (``).

(50) **AY** (``).

### Artifact

(1) **B** (``).

(2) **C** (``).

(3) **D** (``).

(4) **E** (``).

(5) **F** (``).

(6) **G** (``).

(7) **H** (``).

(8) **I** (``).

(9) **J** (``).

(10) **K** (``).

(11) **L** (``).

(12) **M>** (``).

(13) **N** (``).

(14) **O** (``).

(15) **P** (``).

(16) **Q** (``).

(17) **R** (``).

(18) **S** (``).

(19) **T** (``).

(20) **U** (``).

(21) **V** (``).

(22) **W** (``).

(23) **X** (``).

(24) **Y** (``).

(25) **Z** (``).

(26) **AA** (``).

(27) **AB** (``).

(28) **AC** (``).

(29) **AD** (``).

(30) **AE** (``).

(31) **AF** (``).

(32) **AG** (``).

(33) **AH** (``).

(34) **AI** (``).

(35) **AJ** (``).

(36) **AK** (``).

(37) **AL** (``).

(38) **AM** (``).

(39) **AN** (``).

(40) **AO** (``).

(41) **AP** (``).

(42) **AQ** (``).

(43) **AR** (``).

(44) **AS** (``).

(45) **AT** (``).

(46) **AU** (``).

(47) **AV** (``).

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
