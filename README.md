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

The CLI app `tes-tool` in this solution is used to import inscriptions from an Excel file via the Proteus framework. Source Excel files have the following columns (▶️ is the mapping into the above model for inscription items; \* marks a column which is always filled with a value):

(1) **A** (`ID`)\*: the ISicily inscription ID (e.g. `ISic000822`) ▶️ `links`: add an external metadata link.

(2) **B** (`Date notBefore`)\*: a numeric value representing a year for the from-date, negative if BC. This is imported together with C.

(3) **C** (`Date notAfter`)\*: a numeric value representing a year for the to-date, negative if BC. ▶️ `dates` as a range B-C.

(4) **D** (`Site of origin (ancient name)`)\*: the ancient name of the site of origin (e.g. `Syracusae`). This is imported together with E and F.

(5) **E** (`Site of origin (modern name)`)\*: the modern name of the site of origin (e.g. `Siracusa`). This is imported together with E and F.

(6) **F** (`Pleiades ID`): the Pleiades ID of the site of origin (e.g. `places/579570`) ▶️ `links`: add an external metadata link for the ancient name and another one for the modern name, both referring to the same Pleiades ID.

(7) **F** (`Origin latitude`)\*: latitude. This is imported together with G.

(8) **G** (`Origin longitude`)\*: longitude ▶️ `locations` together with F. F-G are the first location which refers to origin.

(9) **H** (`Provenance latitude`): latitude. This is imported together with I.

(10) **I** (`Provenance longitude`): longitude ▶️ `locations` together with H. H-I are the second location which refers to provenance.

(11) **J** (`Material`)\*: ▶️ `support`.`material` mapped to thesaurus 📚 `epi-support-materials`.

(12) **K** (`Object type`): when not specified the value is `N/A`. ▶️ `support`.`objectType` mapped to thesaurus 📚 `epi-support-object-types`.

(13) **L** (`Type`): when not specified the value is `N/A`. ▶️ `categories:ins-fn` mapped to thesaurus 📚 `categories_ins-fn`.

(14) **M** (`Execution type 1`): e.g. `chiselled` ▶️ `technique`.`techniques` mapped to thesaurus 📚 `epi-technique-types`.

(15) **N** (`Execution type 2`): as for M.

(16) **O** (`Language`)\*: ▶️ `categories:ins-lng` mapped to thesaurus 📚 `categories_ins-lng`.

(17) **P** (`Repository name`): repository name (e.g. `Antiquarium di Megara Hyblaia`) ▶️ `metadata`.`preservation-place`.

(18) **Q** (`Inventory number`): inventory number (e.g. `104387`) ▶️ `metadata`.`inventory-nr`.

(19) **R** (`Edition (interpretive)`): text (Leiden) ▶️ text splitting lines at ??.

List of target thesauri:

- [categories part](https://github.com/vedph/cadmus-general/blob/master/docs/categories.md):
  - `categories_ins-fn`
  - `categories_ins-lng`
- [epigraphic support part](https://github.com/vedph/cadmus-epigraphy/blob/master/docs/epi-support.md):
  - `epi-support-materials`
  - `epi-support-object-types`
- [epigraphic technique part](https://github.com/vedph/cadmus-epigraphy/blob/master/docs/epi-technique.md):
  - `epi-technique-types`

### History

- 2026-06-10: updated packages, with adjustments for Proteus-related components.
- 2026-03-23: added literary sources.
- 2026-03-22:
  - updated packages.
  - added `settings` to profile for facet editing.
- 2026-03-19: updated packages.
- 2026-03-12: updated packages.
- 2026-03-10: updated packages.

### 0.0.3

- 2026-03-03: updated packages.
