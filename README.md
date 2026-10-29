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
| note         | X transl       | X transl    | X evidence certainty              | X evidence certainty                    | X evidence certainty          |
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

▶️ (2) run the import commands, once per item type (change the path to your import file), e.g. for inscriptions:

```sh
./tes-tool import-i c:/users/dfusi/desktop/tes/import-i.json
```

> ⚠️ Note that dump importers will save an `item-ids.json` file with the item GUIDs and their EIDs, which is used to resolve internal links in the next import passes. Be sure to copy it in the pipeline folder before running the next import command.

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

---

### 🟢 Inscription

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

> The `site` item has additional parts specified in separate Excel spreadsheets. Each of them is listed below. It is assumed that import happens sequentially: first sites, where also the item IDs are exported during import into a file (via `ItemIdEntrySetExporter`); then site's parts, leveraging the item IDs loaded from this file into an `ExportedItemIdSet` instance which is stored in the conversion pipeline context `Data` dictionary, so that it is available to all column parsers. Each of these additional parts will link to the parent site via an internal link in a `PinLinksPart` part, as illustrated above. All data required to build this link are provided in `ExportedItemIdSet`.

#### 🟢 Site Resources

All columns in this spreadsheet refer to site resources ▶️ `SiteResourcesPart:site-res`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID, stored in `item.groupId`. The EID must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-res.resources[].tag` via 📚 `site-resource-tags@en`.

(5) **E** (`resource_type`): ▶️ `SiteResourcesPart:site-res.resources[].type` via 📚 `site-resource-types_site-res@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

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

#### 🟢 Site Productions

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-prod`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-prod@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-prod@en` rather than `site-resource-features_site-res@en`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID, stored in `item.groupId`. The EID must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-prod.resources[].tag` via 📚 `site-resource-tags@en`.

(5) **E** (`activity_type`): ▶️ `SiteResourcesPart:site-prod.resources[].type` via 📚 `site-resource-types_site-prod@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

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

#### 🟢 Site Markers

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-mark`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-mark@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-mark@en` rather than `site-resource-features_site-res@en`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID, stored in `item.groupId`. The EID must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`area_name`): ❌

(5) **E** (`data_accuracy`): ▶️ `SiteResourcesPart:site-mark.resources[].tag` via 📚 `site-resource-tags@en`.

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

#### 🟢 Site Storages

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-store`. These thesauri change their name because of the different part's role:

- 📚 `site-resource-types_site-store@en` rather than `site-resource-types_site-res@en`.
- 📚 `site-resource-features_site-store@en` rather than `site-resource-features_site-res@en`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID, stored in `item.groupId`. The EID must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `SiteResourcesPart:site-store.resources[].tag` via 📚 `site-resource-tags@en`.

(5) **E** (`storage_type`): ▶️ `SiteResourcesPart:site-store.resources[].type` via 📚 `site-resource-types_site-store@en`: split on `;` and map thesauri labels (from cells) to the corresponding IDs.

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

(14) **N** (`divinity`): ▶️ Together with `divinity_certainty`: `PinLinksPart.links[]`, taxonomy link (tree=gods) whose GID=`@TX` + value derived from the corresponding label found in `divinity` (the divinity ID in a taxonomy, e.g. `zeus`); label=value of `divinity`. Keep the certainty in `links[].assertion.rank` (`high`=1, `medium`=2, `low`=3).

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

(24-45) **X-AS** (`period_1` … `period_22`): ▶️ A labelled chronology block in `CommentPart.text`: list each computed approximate range separately. The current cult facet has no dates part; do not add one silently.

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

(8) **H** (`iconography`): ??

(9) **I** (`iconography_certainty`): see K??.

(10) **J** (`iconography_representation`): ▶️ `NotePart:ico`: free text description of iconography representation.

(11) **K** (`iconography_descriptors`): Together with `iconography_certainty`: ▶️ `PinLinksPart.links[]`, taxonomy link (tree=descriptors) whose GID=`@TX` + value derived from the corresponding label found in `iconography` (the iconography ID in a taxonomy, e.g. `warrior`); label=value of `iconography`. Keep the certainty in `links[].assertion.rank` (`high`=1, `medium`=2, `low`=3). Multiple items are separated by `;` in the cell; create 1 link per item. All items share the same certainty, which is the value of `iconography_certainty`.

(12) **L** (`iconography_subject`): Together with `iconography_subject_certainty`: ▶️ `PinLinksPart.links[]`, taxonomy link (tree=subjects) whose GID=`@TX` + value derived from the corresponding label found in `iconography_subject` (the iconography ID in a taxonomy, e.g. `warrior`); label=value of `iconography_subject`. Keep the certainty in `links[].assertion.rank` (`high`=1, `medium`=2, `low`=3). Multiple items are separated by `;` in the cell; create 1 link per item. All items share the same certainty, which is the value of `iconography_subject_certainty`.

(13) **M** (`iconography_subject_certainty`): See L.

(14) **N** (`context_type`): ▶️ `CategoriesPart.categories:art-ctx` via 📚 `categories_art-ctx@en`.

(15) **O** (`contexttype_certainty`): ❌

(16) **P** (`material`): ▶️ `CategoriesPart.categories:art-mat` via 📚 `categories_art-mat@en`.

(17) **Q** (`production_origin`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="production_origin"`, `value` = the supplied value: `local` / `imported` / `uncertain`, as an origin statement, not a place identifier or a coordinate.

(18) **R** (`productionorigin_certainty`): ❌

(19) **S** (`evidence_type`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="evidence_type"`, `value` = the supplied value.

(20-41) **T-AO** (`period_1` … `period_22`): ▶️ `resources[].date.a.value/b.value`: as above for site periods.

(42) **AP** (`chronology_certainty`): ▶️ `AssertedHistoricalDatesPart.dates[].assertion.rank`, applied to each derived range.

(43) **AQ** (`source_ids`): ❌

(44) **AR** (`source_locator`): ❌

(45) **AS** (`evidence_note`): ▶️ `NotePart:evidence`.

(46) **AT** (`certainty_note`): ▶️ `NotePart:certainty`.

(47) **AU** (`cadmus_mapping`): ❌

---

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
using System.Threading.Tasks;

namespace Cadmus.Tes.Import;

/// <summary>
/// TES column categories entry region parser.
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
            Logger?.LogError(
                "{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = entrySet.GetEntryAt<DecodedTextEntry>(
            entryIndex + 1)!;
        string? value = ImportHelper.FilterValue(txt.Value, false);

        // TODO

        return Task.FromResult(entryIndex + 3);
    }
}
```
