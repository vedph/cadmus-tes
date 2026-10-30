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

This part lists the resources present in a site. Each resource can contain tag, type, any number of features, location, date, counts, references and a free note.

A resource is any generic resource present in a site: a source of materials, a production facility, a storage facility, a marker, etc. Usually each resource type is represented by a `SiteResourcePart` with a specific role ID, and most thesauri change conditionally.

- `resources` (`SiteResource[]`):
  - `eid` (`string`): the resource's entity identifier.
  - `type`\* (`string`, 📚 `site-resource-types`): the type of the resource, e.g. "quarry", "mine", "water source", etc.
  - `tag` (`string`, 📚 `site-resource-tags`): an optional tag or label associated with the object, e.g. "cava A", "pozzo B".
  - `features` (`string[]`, 📚 `site-resource-features`): the list of features for this resource.
  - `location` (`AssertedLocation`: see [brick](https://github.com/vedph/cadmus-bricks-shell-v3/blob/master/projects/myrmidon/cadmus-geo-location/README.md) and its [demo](https://cadmus-bricks-v3.fusi-soft.com/geo/geo-location-editor)):
    - `eid` (`string`)
    - `label`\* (`string`)
    - `latitude`\* (`double`)
    - `longitude`\* (`double`)
    - `altitude` (`double`) in mt.
    - `radius` (`double`): uncertainty radius in mt.
    - `geometry` (`string`) in WKT.
  - `dates` (`AssertedHistoricalDate[]`):
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
  - `references` (🧱 [DocReference[]](https://github.com/vedph/cadmus-bricks/blob/master/docs/doc-reference.md)):
    - `type` (`string` 📚 `doc-reference-types`)
    - `tag` (`string` 📚 `doc-reference-tags`)
    - `citation` (`string`)
    - `note` (`string`)
  - `note` (`string`)

## Parts Matrix

> Items are columns and parts rows. Each cell contains X when the part is present in the item, or is empty when the part is not present. If multiple parts are used for the same item, the part name is suffixed with its role ID (e.g. `CategoriesPart:ins-fn.categories`) and the role ID appears in the cell. Parts suffixed with `=` indicate text layer parts.

| part         | inscription    | lit. source | artifact                          | site                                    | cult                          |
| ------------ | -------------- | ----------- | --------------------------------- | --------------------------------------- | ----------------------------- |
| categories   | ins-fn ins-lng |             | art-type art-mat art-ctx evidence | site-type site-feats evidence           | cult-type cult-feats evidence |
| comment      | X              |             | X                                 | X                                       | X                             |
| dates        | X              |             | X                                 | X                                       |                               |
| fragments    | X              |             |                                   |                                         |                               |
| links        | X              | X           | X                                 | X                                       | X                             |
| locations    | X              |             | X                                 | X                                       |                               |
| metadata     | X              | X           | X                                 | X                                       | X                             |
| support      | X              |             |                                   |                                         |                               |
| techniques   | X              |             |                                   |                                         |                               |
| toponyms     |                |             |                                   | X                                       |                               |
| note         | X transl       | X transl    | X evidence certainty ico          | X evidence certainty                    | X evidence certainty          |
| references   | X              | X           | X                                 | X                                       | X                             |
| resources    |                |             |                                   | site-res site-prod site-mark site-store |                               |
| states       | X              |             |                                   |                                         |                               |
| scripts      | X              |             |                                   |                                         |                               |
| signs        | X              |             |                                   |                                         |                               |
| text         | X              | X           |                                   |                                         |                               |
| apparatus=   | X              | X           |                                   |                                         |                               |
| comment=     | X              | X           |                                   |                                         |                               |
| chronology=  | X              | X           |                                   |                                         |                               |
| orthography= | X              |             |                                   |                                         |                               |
| ligatures=   | X              |             |                                   |                                         |                               |
| links=       | X              | X           |                                   |                                         |                               |

## TES Import

The CLI app `tes-tool` in this solution is used to import entities from an Excel file via the Proteus framework. Source Excel files have the following columns (▶️ is the mapping into the above model for inscription items; \* marks a column which is always filled with a value) for each entity type.

To import:

▶️ (1) fire the API without seeding any items (set item seed count=0 in `appsettings.json`) to create an empty TES MongoDB database.

▶️ (2) run the import commands, once per pipeline (change the path to your pipeline file), e.g. for inscriptions:

```sh
./tes-tool import-i c:/users/dfusi/desktop/tes/Import-i.json
```

Pipelines are in `tes-tool/Assets`, named `Dump-X-md.json` (Markdown dump for diagnostics) and `Import-X.json` (MongoDB), where `X` is:

| X    | source file            | target                                     | item IDs file     |
| ---- | ---------------------- | ------------------------------------------ | ----------------- |
| `i`  | `tes.xlsx`             | inscription items                          | `item-ids-i.json` |
| `s`  | `tes-sites.xlsx`       | site items                                 | `item-ids-s.json` |
| `r`  | `tes-resources.xlsx`   | `SiteResourcesPart:site-res` of sites      | -                 |
| `rp` | `tes-productions.xlsx` | `SiteResourcesPart:site-prod` of sites     | -                 |
| `rm` | `tes-markers.xlsx`     | `SiteResourcesPart:site-mark` of sites     | -                 |
| `rs` | `tes-storages.xlsx`    | `SiteResourcesPart:site-store` of sites    | -                 |
| `c`  | `tes-cults.xlsx`       | cult items                                 | `item-ids-c.json` |
| `a`  | `tes-artifacts.xlsx`   | artifact items                             | `item-ids-a.json` |

> ⚠️ Item pipelines save an `item-ids-X.json` file (in the dump folder) with the item GUIDs and their EIDs, which is used to resolve internal links in the next passes. Before running a pipeline, copy the item IDs files it needs into the pipeline folder: the CLI loads all the `item-ids*.json` files found there.

Item IDs are reused: when an item's EID is found in its own item IDs file (e.g. `item-ids-c.json` for cults), the item and its metadata part get the GUIDs from that file. So, if you run a dump before the corresponding import, and copy its item IDs file into the pipeline folder, the import will produce exactly the same GUIDs. This is required for cults, which link to other cults possibly coming later in the same file. So the recommended order is:

1. `Dump-s-md` → copy `item-ids-s.json`; `Import-s`.
2. `Import-r`, `Import-rp`, `Import-rm`, `Import-rs` (they need `item-ids-s.json` and merge their part into the existing site item).
3. `Dump-c-md` → copy `item-ids-c.json`; `Import-c` (needs `item-ids-s.json` and `item-ids-c.json`).
4. `Dump-a-md` → copy `item-ids-a.json`; `Import-a` (needs `item-ids-s.json`).
5. `Dump-i-md` → copy `item-ids-i.json`; `Import-i`.

▶️ (3) finally, index the imported database:

```sh
./tes-tool index cadmus-tes D:/Projects/Cadmus/Tes/CadmusTes/CadmusTesApi/wwwroot/seed-profile.json
```

In what follows:

- ❌ = ignored.
- ▶️ = target part type ID or target item property.
- 📚 = ID of thesaurus. The default behavior for mapping a cell text to a thesaurus is assuming a single value and assuming it represents the entry label; this is filtered and converted to the corresponding entry ID. Error is signaled on no match.
- **asterisk** \* marks a required column. If not specified, the cell can be null/empty and is just skipped. If a cell belonging to a required column has no value, an error is logged and no data stored for that cell.
- **colon** after a part name specifies the part's role ID (e.g. `CategoriesPart:ins-fn.categories`).
- **internal link** means an internal link in a `PinLinksPart` part, where the target has these properties:
  - gid=`P` + GUID of the MetadataPart of the target item.
  - label=metadata `eid` value + `|` + target item GUID + ` (metadata)`
  - itemId=target item GUID
  - partId=target item's MetadataPart GUID
  - partTypeId: `it.vedph.metadata`
  - name: `eid`
  - value: `alpha`

This implies that you need a mapping between items EIDs, which are the value of the (first) metadatum `eid` in the `MetadataPart` of each item, and the GUIDs of the item and of its metadata part.

### Import Overview

Essentially, the Proteus conversion pipeline gets a sequence of "decoded entries", which are the minimum unit defined for the purpose of a generic conversion: entries are of type T=text, P=properties (e.g. `bold`=`1`), C=commands (e.g. `block(open=1)`). Data from any format, e.g. DOCX or XLSX, is read in this way. Then, regions are detected as sequences of such entries.

In this case the `ExcelEntryReader` is used to read rows from an Excel file where each row represents a target _item_ in Cadmus. A Cadmus item is a record containing any number and type of _parts_, which are objects with their own self-contained model.

The import idea is that each row, read as a sequence of open command entry, text entry and close command entry, whence a row region, generates an item of a given type (=facet); then data from columns (read in the same way, whence a column region) get remodeled into parts and added to the item.

Each row and column read are treated as regions with a specific name. Entry region parsers in the import project handle each row and column accordingly. In XLSX we just deal with open/close commands for rows and columns and text entries for cell content.

As for naming, note that (this is relevant to validate JSON pipeline configurations in `tes-tool/Assets` and `Tag` attributes of code components):

- the region tag is always built with prefix `col-` + the column name exactly as it stands in README. For instance in Inscription column A has ID `ID`, so its region tag is `col-ID`.
- the component `Tag` attribute value is built from the region tag but can be shortened or renamed when the component is more generic. It uses only dash as separator. Each component must have its own distinct `Tag`.
- the corresponding thesauri IDs are suffixed with `_` + part role ID when present.

Finally, an exporter in the Proteus pipeline exports the item either in a Markdown file for diagnostic purposes, or into a MongoDB database. Here the import covers multiple items, so each item type has its own pipeline profile. Their pipeline profiles are in `tes-tool/Assets` (`Dump*` for those exporting Markdown, `Import*` for those exporting into a database).

Most columns are handled by generic configurable parsers (namespace `Cadmus.Tes.Import`), each configured with the region tag it handles (`tag`) and its target:

| parser ID (`entry-region-parser.tes.`) | purpose                                                                                                                          |
| -------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `row-item`                             | create an item with the specified facet (`facetId`) for each row.                                                                |
| `col-metadata`                         | add a metadatum (`name`) and/or set the item's title (`setTitle`); for `eid`, optionally reuse IDs (`reuseIdsFrom`).             |
| `col-categories`                       | map labels (single or multiple) via a thesaurus to `CategoriesPart` with a role.                                                 |
| `col-flags`                            | map boolean (`YES`) columns to thesaurus entry IDs, into `CategoriesPart` or resource features (`resourceRoleId`).               |
| `col-note`                             | append text, optionally under a heading, to `NotePart` with a role or to the resource note (`resourceRoleId`).                   |
| `col-coords`                           | `ycoord`, `xcoord`, `coord_certainty` into `AssertedLocationsPart` or the resource location (`resourceRoleId`).                  |
| `col-periods`                          | `period_1`-`period_22` into `AssertedHistoricalDatesPart`, resource dates (`resourceRoleId`), or `CommentPart` (`target`).       |
| `col-internal-link`                    | internal link to the item with the EID in the cell, looked up in an item IDs set (`itemIdsSet`).                                 |
| `col-link-attr`                        | set the rank, note or tag of the links with the specified scope added by a previous column.                                      |
| `col-taxo-link`                        | taxonomy links (`@TX:TREEID/NODEKEY`), resolving labels via `trees.csv` and `nodes.csv` in `taxonomyDir`.                        |
| `col-parent-item-id`                   | merge the row into the item imported in a previous pass, whose EID is in the cell; rows with an unknown EID are skipped.          |
| `row-resource`                         | (namespace `Resource`) create an item with a `SiteResourcesPart` with a role and a new resource for each row.                    |
| `col-res-property`                     | (namespace `Resource`) set the EID, type, tag, or features of the resource of the row.                                           |

Facet-specific parsers are in the namespaces `Inscription` and `Site`.

---

### 🟢 Inscription

(1) **A** (`ID`)\*: the ISicily inscription ID (e.g. `ISic000822`) ▶️ `MetadataPart.metadata`: add metadata and set as item's title.

(2) **B** (`Date notBefore`)\*: a numeric value representing a year for the from-date, negative if BC. This is imported together with C.

(3) **C** (`Date notAfter`)\*: a numeric value representing a year for the to-date, negative if BC. ▶️ `AssertedHistoricalDatesPart.dates` as a range B-C.

(4) **D** (`Site of origin (ancient name)`)\*: the ancient name of the site of origin (e.g. `Syracusae`). This is imported together with E and F.

(5) **E** (`Site of origin (modern name)`)\*: the modern name of the site of origin (e.g. `Siracusa`). This is imported together with D and F.

(6) **F** (`Pleiades ID`): the Pleiades ID of the site of origin (e.g. `places/579570`) ▶️ `PinLinksPart.links`: add an external metadata link for the ancient name and another one for the modern name, both referring to the same Pleiades ID. If there is no Pleiades ID, the names are preserved as links with scope `toponym-ancient` and `toponym-modern`, whose GID is the name itself, and a warning is logged.

(7) **G** (`Origin latitude`)\*: latitude. This is imported together with H.

(8) **H** (`Origin longitude`)\*: longitude ▶️ ``AssertedLocationsPart.locations` together with G. G-H are the first location which refers to origin.

(9) **I** (`Provenance latitude`): latitude. This is imported together with J.

(10) **J** (`Provenance longitude`): longitude ▶️ `AssertedLocationsPart.locations` together with I. I-J are the second location which refers to provenance.

(11) **K** (`Material`)\*: ▶️ `EpiSupportPart.material` via 📚 `epi-support-materials`.

(12) **L** (`Object type`): when not specified the value is `N/A`. ▶️ `EpiSupportPart.support.objectType` via 📚 `epi-support-object-types`.

(13) **M** (`Type`): when not specified the value is `N/A`. ▶️ `CategoriesPart:ins-fn.categories` via 📚 `categories_ins-fn`.

(14) **N** (`Execution type 1`): e.g. `chiselled` ▶️ `EpiTechniquePart.techniques` via 📚 `epi-technique-types`.

(15) **O** (`Execution type 2`): as for N.

(16) **P** (`Language`)\*: ▶️ `CategoriesPart:ins-lng.categories` via 📚 `categories_ins-lng`.

(17) **Q** (`Repository name`): repository name (e.g. `Antiquarium di Megara Hyblaia`) ▶️ `MetadataPart.metadata.preservation-place`.

(18) **R** (`Inventory number`): inventory number (e.g. `104387`) ▶️ `MetadataPart.metadata.inventory-nr`.

(19) **S** (`Edition (interpretive)`): text (Leiden) ▶️ multiline text.

---

### 🟢 Site

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ `MetadataPart.metadata`: `type="string"`, `name="eid"`, `value=site_id`.

(3) **C** (`site_name`)\*: ▶️ `Item.title` and `AssertedToponymsPart.toponyms[].name.pieces[]`: one piece with `type="name"`, `value=site_name`; no ancient name or language.

(4) **D** (`archaeological_site_type`): ▶️ `CategoriesPart:site-type.categories` via 📚 `categories_site-type@en`.

(5) **E** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories` via 📚 `categories_evidence@en`.

(6) **F** (`ycoord`): ▶️ Together with `xcoord` and `coord_certainty`: `AssertedLocationsPart.locations[].value.latitude/longitude` and `locations[].assertion.rank` (`high`=1, `medium`=2, `low`=3). Label the location with `site_name`.

(7) **G** (`xcoord`): ▶️ See the coordinate group at F.

(8) **H** (`coord_certainty`): ▶️ See the coordinate group at F.

(9) **I** (`pleiades_ref`): ▶️ `PinLinksPart.links[]`: when it contains digits only, it's a Pleiades ID, which goes into `PinLinksPart` as an external link with `Scope`=`pleiades`, `Target.Gid`=value; else preserve a generic label in a metadatum entry in `MetadataPart.metadata` (`name="pleiades_label"`) and log a warning ("missing Pleiades identifier").

(10) **J** (`geonames_ref`): ▶️ `PinLinksPart.links[]`: external link with `Scope`=`geonames`, `Target.Gid`=value.

Columns 11-17 are booleans targeting 📚 `categories_site-feats@en` for ▶️ part `CategoriesPart:site-feats`; when the cell contains `YES`, add the specified thesaurus entry ID to the categories part:

(11) **K** (`cults`): → `cults`.

(12) **L** (`resources`): → `resources`.

(13) **M** (`storage`): → `storage`.

(14) **N** (`production`): → `production`.

(15) **O** (`status_marker`): → `status-marker`.

(16) **P** (`figurative_representations`): → `representations-figurative`.

(17) **Q** (`inscriptions`): → `inscriptions`.

(18-39) **R-AM** (`period_1` … `period_22`): ▶️ `AssertedHistoricalDatesPart.dates[]`: one approximate date range per consecutive YES run, using the common period-boundary rule. These columns correspond each to a binned years range starting from -1000 with size=25. Use `DateBinsConverter` to convert the array of cells with YES into one or more ranges; then create a `HistoricalDate` for each range having its A and B points set to the range boundaries, both with approximate=true, and add it to `AssertedHistoricalDatesPart`.

(40) **AN** (`source_ids`): ❌

(41) **AO** (`source_locator`): ❌

(42) **AP** (`evidence_note`): ▶️ `NotePart:evidence`.

(43) **AQ** (`certainty_note`): ▶️ `NotePart:certainty`.

(44) **AR** (`review_status`): ❌ this contains `READY` when the item is to be imported. ⚠️ The importer does not care about that: users will just filter the Excel file accordingly before importing.

(45) **AS** (`cadmus_mapping`): ❌

> The `site` item has additional parts specified in separate Excel spreadsheets. Each of them is listed below. It is assumed that import happens sequentially: first sites, where also the item IDs are exported during import into a file (via `ItemIdEntrySetExporter`); then site's parts, leveraging the item IDs loaded from this file into an `ExportedItemIdSet` instance which is stored in the conversion pipeline context `Data` dictionary, so that it is available to all column parsers. Each row of these spreadsheets is a resource, which is added to the `SiteResourcesPart` (with the role of its spreadsheet) of the site whose EID is in `site_id`: the row's item gets the GUID of that site, so that when exported into the database it is merged into the site item. All the resources of the same site are accumulated into a single part. Important: ensure that for those files you configure the MongoDB exporter in pipeline to enable merging (`IsMergeEnabled`=`true`) so that the site item is not overwritten by the import of its parts. Note that merging replaces a part with the same type and role: so each of these spreadsheets must be imported in a single pass.

#### 🟩 Site Resources

All columns in this spreadsheet refer to site resources ▶️ `SiteResourcesPart:site-res`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID: the row is merged into the site item having this EID, which must be resolved from the loaded item IDs set (otherwise it's an error and the row is skipped).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-res.resources[].tag` via 📚 `site-resource-tags_site-res@en` (alias of `categories_evidence@en`).

(5) **E** (`resource_type`): ▶️ `SiteResourcesPart:site-res.resources[].type` via 📚 `site-resource-types_site-res@en` (single type, map label to value).

(6) **F** (`resourcetype_certainty`): ❌

(7) **G** (`resource_feature_ID`): ▶️ `SiteResourcesPart:site-res.resources[].eid`.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates, targeting `resources[].location`.

(11) **K** (`exploitation_evidence`): ▶️ `SiteResourcesPart:site-res.resources[].features[]` via 📚 `site-resource-features_site-res@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

(12) **L** (`exploitation_evidence_certainty`): ❌

(13) **M** (`connection_to_site`): ❌

(14) **N** (`production_scale`): ❌

(15) **O** (`productionscale_certainty`): ❌

(16-37) **P-AK** (`period_1` … `period_22`): ▶️ `SiteResourcesPart:site-res.resources[].dates[].a.value/b.value`: as above for site periods.

(38) **AL** (`source_ids`): ❌

(39) **AM** (`source_locator`): ❌

(40) **AN** (`evidence_note`): ▶️ append to `SiteResourcesPart:site-res.resources[].note` under heading "Evidence Note".

(41) **AO** (`certainty_note`): ▶️ append to `SiteResourcesPart:site-res.resources[].note` under heading "Certainty Note".

(42) **AP** (`review_status`): ❌

(43) **AQ** (`cadmus_mapping`): ❌

#### 🟩 Site Productions

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-prod`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-prod@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-prod@en` rather than `site-resource-features_site-res@en`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID: the row is merged into the site item having this EID, which must be resolved from the loaded item IDs set (otherwise it's an error and the row is skipped).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-prod.resources[].tag` via 📚 `site-resource-tags_site-prod@en` (alias of `categories_evidence@en`).

(5) **E** (`activity_type`): ▶️ `SiteResourcesPart:site-prod.resources[].type` via 📚 `site-resource-types_site-prod@en` (single type, map label to value).

(6) **F** (`activity_type_certainty`): ❌

(7) **G** (`production_feature_ID`): ▶️ `SiteResourcesPart:site-prod.resources[].eid`.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates, targeting `resources[].location`.

(11) **K** (`material_focus`): ▶️ `SiteResourcesPart:site-prod.resources[].features[]` via 📚 `site-resource-features_site-prod@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

(12) **L** (`evidence_type`): ▶️ `SiteResourcesPart:site-prod.resources[].features[]` via 📚 `site-resource-features_site-prod@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

(13) **M** (`production_scale`): ❌

(14) **N** (`productionscale_certainty`): ❌

(15-36) **O-AJ** (`period_1` … `period_22`): ▶️ `SiteResourcesPart:site-prod.resources[].dates[].a.value/b.value`: as above for site periods.

(37) **AK** (`source_ids`): ❌

(38) **AL** (`source_locator`): ❌

(39) **AM** (`evidence_note`): ▶️ append to `SiteResourcesPart:site-prod.resources[].note` under heading "Evidence Note".

(40) **AN** (`certainty_note`): ▶️ append to `SiteResourcesPart:site-prod.resources[].note` under heading "Certainty Note".

(41) **AO** (`review_status`): ❌

(42) **AP** (`cadmus_mapping`): ❌

#### 🟩 Site Markers

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-mark`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-mark@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-mark@en` rather than `site-resource-features_site-res@en`.

The type of this resource is always `funerary-evidence`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID: the row is merged into the site item having this EID, which must be resolved from the loaded item IDs set (otherwise it's an error and the row is skipped).

(3) **C** (`site_name`): ❌

(4) **D** (`area_name`): ❌

(5) **E** (`data_accuracy`): ▶️ `SiteResourcesPart:site-mark.resources[].tag` via 📚 `site-resource-tags_site-mark@en` (alias of `categories_evidence@en`).

(6-8) **F-H** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates, targeting `resources[].location`.

Columns 7-12 are booleans targeting 📚 `site-resource-features_site-mark@en` for ▶️ `SiteResourcesPart:site-mark.resources[].features[]`; when the cell contains `YES`, add the specified thesaurus entry ID to the categories part:

(7) **I** (`differentiated_tomb_types`)

(8) **J** (`distinctive_funerary_habit`)

(9) **K** (`distinctive_funerary_assemblage`)

(10) **L** (`exotic_good`)

(11) **M** (`jewelry`)

(12) **N** (`metal objects`)

(13-34) **O-AJ** (`period_1` … `period_22`): ▶️ `SiteResourcesPart:site-mark.resources[].dates[].a.value/b.value`: as above for site periods.

(35) **AK** (`source_ids`): ❌

(36) **AL** (`source_locator`): ❌

(37) **AM** (`evidence_note`): ▶️ append to `SiteResourcesPart:site-mark.resources[].note` under heading "Evidence Note".

(38) **AN** (`certainty_note`): ▶️ append to `SiteResourcesPart:site-mark.resources[].note` under heading "Certainty Note".

(39) **AO** (`cadmus_mapping`): ❌

(40) **AP** (`marker_feature_id`): ▶️ `SiteResourcesPart:site-mark.resources[].eid`.

#### 🟩 Site Storages

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-store`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-store@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-store@en` rather than `site-resource-features_site-res@en`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID: the row is merged into the site item having this EID, which must be resolved from the loaded item IDs set (otherwise it's an error and the row is skipped).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-store.resources[].tag` via 📚 `site-resource-tags_site-store@en` (alias of `categories_evidence@en`).

(5) **E** (`storage_type`): ▶️ `SiteResourcesPart:site-store.resources[].type` via 📚 `site-resource-types_site-store@en` (single type, map label to value).

(6) **F** (`storage_type_certainty`): ❌

(7) **G** (`storage_feature_ID`): ▶️ `SiteResourcesPart:site-store.resources[].eid`.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates, targeting `resources[].location`.

(11) **K** (`evidence_type`): ▶️ `SiteResourcesPart:site-store.resources[].features[]` via 📚 `site-resource-features_site-store@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

(12-33) **L-AG** (`period_1` … `period_22`): ▶️ `SiteResourcesPart:site-store.resources[].dates[].a.value/b.value`: as above for site periods.

(34) **AH** (`source_ids`): ❌

(35) **AI** (`source_locator`): ❌

(36) **AJ** (`evidence_note`): ▶️ append to `SiteResourcesPart:site-store.resources[].note` under heading "Evidence Note".

(37) **AK** (`certainty_note`): ▶️ append to `SiteResourcesPart:site-store.resources[].note` under heading "Certainty Note".

(38) **AL** (`cadmus_mapping`): ❌

---

### 🟢 Cult

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ `PinLinksPart.links[].target`: internal link to the site whose EID is `site_id`.

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories` via 📚 `categories_evidence@en`.

(5) **E** (`cult_element_type`): ▶️ `CategoriesPart.categories:cult-type` via 📚 `categories_cult-type@en`.

(6) **F** (`cult_element_type_certainty`): ❌

(7) **G** (`cult_element_id`): ▶️ `MetadataPart.metadata` (`type="string"`, `name="eid"`) and `Item.title`.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates.

(11) **K** (`related_cult_element_id`): ▶️ Together with `relation_type` and `relation_certainty`: `PinLinksPart.links[]`, resolving the target cult by EID plus site context. Set `target.itemId` and `target.label`; keep the relation code in `links[].note`, with its supplied certainty in `links[].assertion.rank` (`high`=1, `medium`=2, `low`=3).

(12) **L** (`relation_type`): ▶️ See K.

(13) **M** (`relation_certainty`): ▶️ See K.

(14) **N** (`divinity`): ▶️ Together with `divinity_certainty`: `PinLinksPart.links[]`, taxonomy link (tree=gods) whose GID=`@TX:TREEID/` + node key derived from the corresponding label found in `divinity` (the divinity ID in a taxonomy, e.g. `zeus`); label=value of `divinity`. Keep the certainty in `links[].assertion.rank` (`high`=1, `medium`=2, `low`=3).

(15) **O** (`divinity_certainty`): ▶️ See N.

Columns 16-23 are booleans targeting 📚 `categories_cult-feats@en` for ▶️ part `CategoriesPart:cult-feats`; when the cell contains `YES`, add the specified thesaurus entry ID to the categories part:

(16) **P** (`animal_sacrifices`): → `animal-sacrifices`.

(17) **Q** (`food_consumption`): → `food-consumption`.

(18) **R** (`beverage_consumption`): → `beverage-consumption`.

(19) **S** (`architectural_models`): → `architectural-models`.

(20) **T** (`statuettes`): → `statuettes`.

(21) **U** (`lamps`): → `lamps`.

(22) **V** (`weapons`): → `weapons`.

(23) **W** (`epigraphical_evidence`): → `epigraphical-evidence`.

(24-45) **X-AS** (`period_1` … `period_22`): ▶️ A labelled chronology block in `CommentPart.text`: list each computed approximate range separately.

(46) **AT** (`source_ids`): ❌

(47) **AU** (`source_locator`): ❌

(48) **AV** (`evidence_note`): ▶️ `NotePart:evidence`.

(49) **AW** (`certainty_note`): ▶️ `NotePart:certainty`.

(50) **AX** (`cadmus_mapping`): ❌

---

### 🟢 Artifact

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ `PinLinksPart.links[].target`: link to the site resolved by `site_id`; set `target.itemId` to the resolved item UUID and its display label to `site_name`.

(3) **C** (`site_name`): ❌

(4) **D** (`feature_ID`): ▶️ `MetadataPart.metadata` (`type="string"`, `name="eid"`, `value=feature_ID`).

(5) **E** (`designation`): ▶️ `Item.title`.

(6) **F** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories` via 📚 `categories_evidence@en`.

(7) **G** (`artefact_type`): ▶️ `CategoriesPart.categories:art-type` via 📚 `categories_art-type@en`.

(8) **H** (`iconography`): ❌

(9) **I** (`iconography_certainty`): ▶️ append to `NotePart:ico` text under heading `Certainty`.

(10) **J** (`iconography_representation`): ▶️ append to `NotePart:ico` text under heading `Representation`.

(11) **K** (`iconography_descriptors`): ▶️ `PinLinksPart.links[]`, taxonomy link (tree=descriptors) whose GID=`@TX:TREEID/` + node key derived from the corresponding label found in `iconography_descriptors` (the iconography ID in a taxonomy, e.g. `warrior`); label=value of `iconography_descriptors`. Multiple items are separated by `;` in the cell; create 1 link per item.

(12) **L** (`iconography_subject`): ▶️ `PinLinksPart.links[]`, taxonomy link (tree=subjects) whose GID=`@TX:TREEID/` + node key derived from the corresponding label found in `iconography_subject` (the iconography ID in a taxonomy, e.g. `warrior`); label=value of `iconography_subject`. Multiple items are separated by `;` in the cell; create 1 link per item.

(13) **M** (`iconography_subject_certainty`): ▶️ append to `NotePart:ico` text under heading `Subject`.

(14) **N** (`context_type`): ▶️ `CategoriesPart.categories:art-ctx` via 📚 `categories_art-ctx@en`.

(15) **O** (`contexttype_certainty`): ❌

(16) **P** (`material`): ▶️ `CategoriesPart.categories:art-mat` via 📚 `categories_art-mat@en`.

(17) **Q** (`production_origin`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="production_origin"`, `value` = the supplied value: `local` / `imported` / `uncertain`, as an origin statement, not a place identifier or a coordinate.

(18) **R** (`productionorigin_certainty`): ❌

(19) **S** (`evidence_type`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="evidence_type"`, `value` = the supplied value.

(20-41) **T-AO** (`period_1` … `period_22`): ▶️ `AssertedHistoricalDatesPart.dates[]`: as above for site periods.

(42) **AP** (`chronology_certainty`): ▶️ `AssertedHistoricalDatesPart.dates[].assertion.rank`, applied to each derived range.

(43) **AQ** (`source_ids`): ❌

(44) **AR** (`source_locator`): ❌

(45) **AS** (`evidence_note`): ▶️ `NotePart:evidence`.

(46) **AT** (`certainty_note`): ▶️ `NotePart:certainty`.

(47) **AU** (`cadmus_mapping`): ❌

---

### Code Template

Template for a column region parser. Prefer the generic configurable parsers listed above when possible; when a new parser is required, derive it from `ColEntryRegionParser`, which checks for the current item, gets the filtered cell value, and skips rows marked as skipped. Use `ImportHelper` for thesauri, ranks, coordinates, notes, and item IDs.

- `__TAG__`: the region tag.

```cs
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column __TAG__ entry region parser.
/// <para>Tag: <c>entry-region-parser.tes.col-__TAG__</c>.</para>
/// </summary>
/// <seealso cref="ColEntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.tes.col-__TAG__")]
public sealed class Col__TAG__EntryRegionParser : ColEntryRegionParser,
    IEntryRegionParser
{
    /// <summary>
    /// Gets the tags of the regions that this parser can handle.
    /// </summary>
    public string[] RegionTags => ["col-__TAG__"];

    /// <summary>
    /// Parses the specified column value.
    /// </summary>
    /// <param name="context">The context, having a current item.</param>
    /// <param name="region">The column region.</param>
    /// <param name="value">The filtered value, or null if empty.</param>
    protected override void ParseValue(CadmusEntrySetContext context,
        EntryRegion region, string? value)
    {
        // use CheckRequired(region, value) for required columns
        if (value == null) return;

        // TODO
    }
}
```
