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

| part         | inscription    | lit. source | artifact                          | site                          | cult                                    | iconography |
| ------------ | -------------- | ----------- | --------------------------------- | ----------------------------- | --------------------------------------- | ----------- |
| categories   | ins-fn ins-lng |             | art-type art-mat art-ctx evidence | site-type site-feats evidence | cult-type cult-gods cult-feats evidence | ico-sub     |
| comment      | X              |             | X                                 | X                             | X                                       | X           |
| dates        | X              |             | X                                 | X                             |                                         |             |
| fragments    | X              |             |                                   |                               |                                         |             |
| links        | X              | X           | X                                 | X                             | X                                       | X           |
| locations    | X              |             | X                                 | X                             |                                         |             |
| metadata     | X              | X           | X                                 | X                             | X                                       | X           |
| support      | X              |             |                                   |                               |                                         |             |
| techniques   | X              |             |                                   |                               |                                         |             |
| toponyms     |                |             |                                   | X                             |                                         |             |
| note         | X transl       | X transl    | X                                 | X                             | X                                       | X           |
| references   | X              | X           | X                                 | X                             | X                                       | X           |
| resources    |                |             |                                   | site-res site-prod            |                                         |             |
| states       | X              |             |                                   |                               |                                         |             |
| scripts      | X              |             |                                   |                               |                                         |             |
| signs        | X              |             |                                   |                               |                                         |             |
| text         | X              | X           |                                   |                               |                                         |             |
| apparatus=   | X              | X           |                                   |                               |                                         |             |
| comment=     | X              | X           |                                   |                               |                                         |             |
| chronology=  | X              | X           |                                   |                               |                                         |             |
| orthography= | X              |             |                                   |                               |                                         |             |
| ligatures=   | X              |             |                                   |                               |                                         |             |
| links=       | X              | X           |                                   |                               |                                         |             |

## TES Import

The CLI app `tes-tool` in this solution is used to import entities from an Excel file via the Proteus framework. Source Excel files have the following columns (▶️ is the mapping into the above model for inscription items; \* marks a column which is always filled with a value) for each entity type.

To import:

1. fire the API without seeding any items (set item seed count=0 in `appsettings.json`) to create an empty TES MongoDB database.
2. run the import commands, once per item type (change the path to your import file), e.g. for inscriptions:

```sh
./tes-tool import-i c:/users/dfusi/desktop/tes/import-i.json
```

Note that dump importers will save an `item-ids.json` file with the item GUIDs and their EIDs, which is used to resolve internal links in the next import passes. So be sure to copy it in the pipeline folder before running the next import command.

3. finally, index the imported database:

```sh
./tes-tool index cadmus-tes D:/Projects/Cadmus/Tes/CadmusTes/CadmusTesApi/wwwroot/seed-profile.json
```

In what follows:

- ❌ = ignored.
- ▶️ = target part type ID or target item property.
- 📚 = thesaurus ID.
- **asterisk** \* marks a required column. If not specified, the cell can be null/empty and is just skipped. If a cell belonging to a required column has no value, an error is logged and no data stored for that cell.
- **colon** after a part name specifies the part's role ID (e.g. `CategoriesPart:ins-fn.categories`).
- **internal link** means an internal link in a `PinLinksPart` part, where the target has these properties:
  - gid=`P` + GUID of the MetadataPart of the target item.
  - label=metadata `eid` value + ` | ` + target item GUID + ` (metadata)`
  - itemId=target item GUID
  - partId=target item's MetadataPart GUID
  - partTypeId: `it.vedph.metadata`
  - name: `eid`
  - value: `alpha`

This implies that you need a mapping between items EIDs, which are the value of the (first) metadatum `eid` in the `MetadataPart` of each item, and the GUIDs of the item and of its metadata part.

---

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

(11) **K** (`Material`)\*: ▶️ `EpiSupportPart.material` mapped to 📚 `epi-support-materials`.

(12) **L** (`Object type`): when not specified the value is `N/A`. ▶️ `EpiSupportPart.support.objectType` mapped to 📚 `epi-support-object-types`.

(13) **M** (`Type`): when not specified the value is `N/A`. ▶️ `CategoriesPart:ins-fn.categories` mapped to 📚 `categories_ins-fn`.

(14) **N** (`Execution type 1`): e.g. `chiselled` ▶️ `EpiTechniquePart.techniques` mapped to 📚 `epi-technique-types`.

(15) **O** (`Execution type 2`): as for N.

(16) **P** (`Language`)\*: ▶️ `CategoriesPart:ins-lng.categories` mapped to 📚 `categories_ins-lng`.

(17) **Q** (`Repository name`): repository name (e.g. `Antiquarium di Megara Hyblaia`) ▶️ `MetadataPart.metadata.preservation-place`.

(18) **R** (`Inventory number`): inventory number (e.g. `104387`) ▶️ `MetadataPart.metadata.inventory-nr`.

(19) **S** (`Edition (interpretive)`): text (Leiden) ▶️ multiline text.

---

### Site

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ `MetadataPart.metadata`: `type="string"`, `name="eid"`, `value=site_id`.

(3) **C** (`site_name`)\*: ▶️ `Item.title` and `AssertedToponymsPart.toponyms[].name.pieces[]`: one piece with `type="name"`, `value=site_name`; no ancient name or language.

(4) **D** (`archaeological_site_type`): ▶️ `CategoriesPart:site-type.categories`; 📚 `categories_site-type@en`. Match the supplied label to its exact ID, e.g. multiple functions → `multiple`, sacred area → `sacred-area`, necropolis → `necropolis`.

(5) **E** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories`; 📚 `categories_evidence@en`.

(6) **F** (`ycoord`): ▶️ Together with `xcoord` and `coord_certainty`: `AssertedLocationsPart.locations[].value.latitude/longitude` and `locations[].assertion.rank`. Label the location with `site_name`.

(7) **G** (`xcoord`): ▶️ See the coordinate group at F.

(8) **H** (`coord_certainty`): ▶️ See the coordinate group at F.

(9) **I** (`pleiades_ref`): ▶️ `PinLinksPart.links[]`: when it contains digits only, it's a Pleiades ID, which goes into `PinLinksPart` as an external link with `Scope`=`pleiades`, `Target.Gid`=value; else preserve a generic label in a metadatum entry in `MetadataPart.metadata` (`name="pleiades_label"`) and log a warning (missing Pleiades identifier).

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

(42) **AP** (`evidence_note`): ❌

(43) **AQ** (`certainty_note`): ❌

(44) **AR** (`review_status`): ❌ this contains `READY` when the item is to be imported. ⚠️ The importer does not care about that: users will just filter the Excel file accordingly before importing.

(45) **AS** (`cadmus_mapping`): ❌

> The `site` item has additional parts specified in separate Excel spreadsheets. Each of them is listed below. It is assumed that import happens sequentially: first sites, where also the item IDs are exported during import into a file (via `ItemIdEntrySetExporter`); then site's parts, leveraging the item IDs loaded from this file into an `ExportedItemIdSet` instance which is stored in the conversion pipeline context `Data` dictionary, so that it is available to all column parsers.

#### Site Resources

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-res`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`)\*: ▶️ the target site EID. This must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ ??.

(5) **E** (`resource_type`): ▶️ `SiteResourcesPart:site-res.resources[].type`; 📚 `site-resource-types_site-res@en`: map strings to thesauri ID: "clay" → `clay-source`; "quarry" → `stone-quarry`; "mine" → `metal-mine`; "salt source" → `salt-source`; "water source" → `water-spring`. The cell contains a single entry.

(6) **F** (`resourcetype_certainty`): ▶️ `resources[].note`??.

(7) **G** (`resource_feature_ID`): ▶️ `resources[].eid`: retain the supplied subsidiary identifier.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates.

(11) **K** (`exploitation_evidence`): ▶️ `resources[].features[]`; 📚 `site-resource-features_site-res@en`: map these strings to thesauri ID: "archaeological assemblage" → `archaeological-asm`; for all other values, match labels from thesauri to their IDs as usual??.

(12) **L** (`exploitation_evidence_certainty`): ▶️ `resources[].note`??.

(13) **M** (`connection_to_site`): ▶️ `resources[].note`??.

(14) **N** (`production_scale`): ▶️ `resources[].note`??.

(15) **O** (`productionscale_certainty`): ▶️ `resources[].note`??.

(16-37) **P-AK** (`period_1` … `period_22`): ▶️ `resources[].date.a.value/b.value`: as above for site periods.

(38) **AL** (`source_ids`): ❌

(39) **AM** (`source_locator`): ❌

(40) **AN** (`evidence_note`): ❌

(41) **AO** (`certainty_note`): ❌

(42) **AP** (`review_status`): ❌

(43) **AQ** (`cadmus_mapping`): ❌

#### Site Production

All columns in this spreadsheet refer to site resources, which target ▶️ `SiteResourcesPart:site-prod`.

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ the target site EID. This must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ??

(5) **E** (`activity_type`): ▶️ `SiteResourcesPart:site-prod.resources[].type`; 📚 `site-resource-types_site-prod@en`: ceramic production → `ceramic-production`; match the other activities to existing entries.

(6) **F** (`activitytype_certainty`): ▶️ `resources[].note`, labelled activitytype_certainty.

(7) **G** (`production_feature_ID`): ▶️ `resources[].eid`: retain the supplied subsidiary identifier.

(8) **H** (`ycoord`): ▶️ With `xcoord` and `coord_certainty`, `resources[].location.value.latitude/longitude` and `resources[].location.assertion.rank`; `value.label=production_feature_ID`. No location is created for the sample with missing coordinates.

(9) **I** (`xcoord`): ▶️ See H.

(10) **J** (`coord_certainty`): ▶️ See H; if coordinates are absent retain this label in `resources[].note` only.

(11) **K** (`material_focus`): ▶️ `resources[].note`, labelled material_focus; preserve the stated material without assigning an artifact-material category to the site.

(12) **L** (`evidence_type`): ▶️ `resources[].features[]`; 📚 `site-resource-features_site-prod@en`. Split on ;: kilns → `kilns`, wasters → `wasters`; unknown values stay in `resources[].note` and are reported.

(13) **M** (`production_scale`): ▶️ `resources[].features[]`, same production features thesaurus: household scale → `household-scale`, workshop scale → `workshop-scale`, uncertain → `uncertain`. Do not infer a scale from kiln presence alone.

(14) **N** (`productionscale_certainty`): ▶️ `resources[].note`, labelled productionscale_certainty.

(15-36) **O-AJ** (`period_1` … `period_22`): ▶️ `resources[].date.a.value/b.value`: an approximate range for the documented activity represented by this row, not a precise date for every installation. Apply the singular-date exception for disjoint runs.

(37) **AK** (`source_ids`): ❌

(38) **AL** (`source_locator`): ❌

(39) **AM** (`evidence_note`): ❌

(40) **AN** (`certainty_note`): ❌

(41) **AO** (`review_status`): ❌

(42) **AP** (`cadmus_mapping`): ❌

#### Site Markers

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ the target site EID. This must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`area_name`): ▶️ Heading of this `CommentPart.text` record block, preserving the supplied area name.

(5) **E** (`data_accuracy`): ??

(6) **F** (`ycoord`): ▶️ Together with xcoord and coord_certainty, a labelled location in this record block. Do not overwrite the parent Site location.

(7) **G** (`xcoord`): ▶️ See F.

(8) **H** (`coord_certainty`): ▶️ See F.

(9) **I** (`differentiated_tomb_types`): ▶️ Labelled `differentiated_tomb_types` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(10) **J** (`distinctive_funerary_habit`): ▶️ Labelled `distinctive_funerary_habit` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(11) **K** (`distinctive_funerary_assemblage`): ▶️ Labelled `distinctive_funerary_assemblage` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(12) **L** (`exotic_good`): ▶️ Labelled `exotic_good` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(13) **M** (`jewelry`): ▶️ Labelled `jewelry` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(14) **N** (`metal objects`): ▶️ Labelled `metal objects` in this `CommentPart.text` block. Preserve YES / NO / blank distinctly; no social rank, ethnicity, or group membership is inferred.

(15-36) **O-AJ** (`period_1` … `period_22`): ▶️ Computed approximate ranges in this record’s `CommentPart.text` chronology block; do not extend the main Site dates from these bins.

(37) **AK** (`source_ids`): ❌

(38) **AL** (`source_locator`): ❌

(39) **AM** (`evidence_note`): ❌

(40) **AN** (`certainty_note`): ❌

(41) **AO** (`review_status`): ❌

(42) **AP** (`cadmus_mapping`): ❌

#### Site Storage

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ the target site EID. This must be resolved from the loaded item IDs set (otherwise it's an error).

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ??

(5) **E** (`storage_type`): ▶️ `CommentPart.text`, labelled storage_type in the subsidiary record block. Preserve silo/pit; no categories_storage-type thesaurus is assumed in the current profile.

(6) **F** (`storagetype_certainty`): ▶️ `CommentPart.text`, labelled storagetype_certainty in the same block.

(7) **G** (`storage_feature_id`): ▶️ Heading / subsidiary EID in this `CommentPart.text` block. It is not a new item EID.

(8) **H** (`ycoord`): ▶️ Together with xcoord and coord_certainty, a labelled location in this record block. Do not overwrite the parent Site location.

(9) **I** (`xcoord`): ▶️ See H.

(10) **J** (`coord_certainty`): ▶️ See H.

(11) **K** (`evidence_type`): ▶️ `CommentPart.text`, labelled evidence_type, preserving architectural or other supplied values without an unsupported new category role.

(12-33) **L-AG** (`period_1` … `period_22`): ▶️ Computed approximate ranges in the subsidiary `CommentPart.text` record block; do not add them as a new continuous period of the whole site.

(34) **AH** (`source_ids`): ❌

(35) **AI** (`source_locator`): ❌

(36) **AJ** (`evidence_note`): ❌

(37) **AK** (`certainty_note`): ❌

(38) **AL** (`review_status`): ❌

(39) **AM** (`cadmus_mapping`): ❌

---

### Cult

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ `PinLinksPart.links[].target`: internal link to the site whose EID is `site_id`.

(3) **C** (`site_name`): ❌

(4) **D** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories`; 📚 `categories_evidence@en`.

(5) **E** (`cult_element_type`): ▶️ `CategoriesPart.categories:cult-type`; 📚 `categories_cult-type@en`. religious building (uncertain type) → `building-religious`; altar → `altar-300052391`; votive deposit → `deposit-votive`. Match other inputs to existing entries, never to IDs copied from `cadmus_mapping`.

(6) **F** (`cult_element_type_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="cult_element_type_certainty"`, `value` = the supplied value. The certainty refers to this classification, not to the named divinity.

(7) **G** (`cult_element_id`): ▶️ `MetadataPart.metadata` (`type="string"`, `name="eid"`) and `Item.title`. Preserve the full supplied identifier, including any descriptive suffix, for matching. Resolve related cults by the same exact value; disambiguate with `site_id` when necessary.

(8-10) **H-J** (`ycoord`, `xcoord`, `coord_certainty`): as above for site coordinates.

(11) **K** (`related_cult_element_id`): ▶️ Together with `relation_type` and `relation_certainty`: `PinLinksPart.links[]`, resolving the target cult by EID plus site context. Set `target.itemId` and `target.label`; keep the relation code in `links[].note`, with its supplied certainty in `links[].assertion.rank`. Preserve unresolved references in `CommentPart.text` and report them, never fabricate a target.

(12) **L** (`relation_type`): ▶️ See K. Preserve `within_same_cult_area` literally in the link note. This is a spatial association; it is not evidence of the same god, function, or phase. No `categories_cult-rel` thesaurus is assumed.

(13) **M** (`relation_certainty`): ▶️ See K. If the target ID is blank, do not create a relation from this cell alone.

(14) **N** (`divinity`): ▶️ `CategoriesPart.categories:cult-gods`; 📚 `categories_cult-gods@en`. Match explicit supplied divinity labels to exact entries; leave blank if no divinity is identified.

(15) **O** (`divinity_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="divinity_certainty"`, `value` = the supplied value. It qualifies the supplied identification; no identification is inferred from a certainty value alone.

(16) **P** (`animal_sacrifices`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `animal-sacrifices`; 📚 `categories_cult-feats@en`.

(17) **Q** (`food_consumption`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `food-consumption`; 📚 `categories_cult-feats@en`.

(18) **R** (`beverage_consumption`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `beverage-consumption`; 📚 `categories_cult-feats@en`.

(19) **S** (`architectural_models`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `architectural-models`; 📚 `categories_cult-feats@en`.

(20) **T** (`statuettes`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `statuettes`; 📚 `categories_cult-feats@en`.

(21) **U** (`lamps`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `lamps`; 📚 `categories_cult-feats@en`.

(22) **V** (`weapons`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `weapons`; 📚 `categories_cult-feats@en`.

(23) **W** (`epigraphical_evidence`): ▶️ `CategoriesPart.categories:cult-feats`: explicit YES → `epigraphical-evidence`; 📚 `categories_cult-feats@en`.

(24-45) **X-AS** (`period_1` … `period_22`): ▶️ A labelled chronology block in `CommentPart.text`: list each computed approximate range separately. The current cult facet has no dates part; do not add one silently.

(46) **AT** (`source_ids`): ❌

(47) **AU** (`source_locator`): ❌

(48) **AV** (`evidence_note`): ❌

(49) **AW** (`certainty_note`): ❌

(50) **AX** (`review_status`): ❌

(51) **AY** (`cadmus_mapping`): ❌

---

### Artifact

(1) **A** (`id`): ❌

(2) **B** (`site_id`): ▶️ `PinLinksPart.links[].target`: link to the site resolved by `site_id`; set `target.itemId` to the resolved item UUID and its display label to `site_name`. This is not the artifact EID.

(3) **C** (`site_name`): ❌ Display label for the site link; the artifact title comes from designation.

(4) **D** (`feature_ID`): ▶️ `MetadataPart.metadata` (`type="string"`, `name="eid"`, `value=feature_ID`). Use the original identifier; qualify with `site_id` if IDs repeat between sites. Maintain an explicit `(site_id, feature_ID)` lookup for links; do not silently merge distinct objects.

(5) **E** (`designation`): ▶️ `Item.title`.

(6) **F** (`data_accuracy`): ▶️ `CategoriesPart:evidence.categories`; 📚 `categories_evidence@en`.

(7) **G** (`artefact_type`): ▶️ `CategoriesPart.categories:art-type`; 📚 `categories_art-type@en`. architectural elements → `architectural-300003531`; reliefs (sculpture) → `reliefs-300047218`; other inputs must resolve to an existing ID.

(8) **H** (`iconography`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_legacy"`, `value` = the supplied value. Original unsplit label; does not create an iconography item or select a taxonomy term.

(9) **I** (`iconography_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_legacy_certainty"`, `value` = the supplied value. Do not copy automatically to subject certainty: protome, for example, may be a form rather than an identified subject.

(10) **J** (`iconography_representation`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_representation"`, `value` = the supplied value. Local label identifying the depicted figure/scene described by the next fields.

(11) **K** (`iconography_descriptors`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_descriptors"`, `value` = the supplied value. Preserve the proposed labels and their ; separator; taxonomy resolution is postponed until the descriptor tree exists.

(12) **L** (`iconography_subject`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_subject"`, `value` = the supplied value. Proposed interpretation; may remain empty when only a form is documented.

(13) **M** (`iconography_subject_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="iconography_subject_certainty"`, `value` = the supplied value. Applies only to the supplied subject; no value is inferred from legacy iconography certainty.

(14) **N** (`context_type`): ▶️ `CategoriesPart.categories:art-ctx`; 📚 `categories_art-ctx@en`: isolated find → `isolated-find`, funerary context → `funerary`; match other explicit values to the existing thesaurus.

(15) **O** (`contexttype_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="contexttype_certainty"`, `value` = the supplied value.

(16) **P** (`material`): ▶️ `CategoriesPart.categories:art-mat`; 📚 `categories_art-mat@en`: limestone → `limestone-300011447`, terracotta → `terracotta-300010669`; match other materials to existing entries.

(17) **Q** (`production_origin`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="production_origin"`, `value` = the supplied value. local / imported / uncertain is an origin statement, not a place identifier or a coordinate.

(18) **R** (`productionorigin_certainty`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="productionorigin_certainty"`, `value` = the supplied value.

(19) **S** (`evidence_type`): ▶️ `MetadataPart.metadata`: `type="string"`, `name="evidence_type"`, `value` = the supplied value. Preserve e.g. reused or documented only (published analysis); it is distinct from documentary-context data_accuracy.

(20-41) **T-AO** (`period_1` … `period_22`): ▶️ `AssertedHistoricalDatesPart.dates[]`: one approximate range per consecutive YES run; apply `chronology_certainty` as specified at AP.

(42) **AP** (`chronology_certainty`): ▶️ `AssertedHistoricalDatesPart.dates[].assertion.rank`, applied to each derived range; also preserve the supplied label as `MetadataPart.metadata` (`name="chronology_certainty"`). If no range is supplied, the label alone does not generate a date.

(43) **AQ** (`source_ids`): ❌

(44) **AR** (`source_locator`): ❌

(45) **AS** (`evidence_note`): ❌

(46) **AT** (`certainty_note`): ❌

(47) **AU** (`review_status`): ❌

(48) **AV** (`cadmus_mapping`): ❌

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
