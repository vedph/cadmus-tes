# History

## 1.0.0

- 2026-11-14:
  - ⚠️ upgraded to .NET 11 and Cadmus 18.
  - ⚠️ moved the generic parsers (`row-item`, `col-metadata`, `col-categories`, `col-flags`, `col-note`, `col-periods`, `col-internal-link`, `col-link-attr`, `col-taxo-link`, `col-parent-item-id`) and `ImportHelper` to the shared library `Cadmus.Import.Excel`: their IDs are now prefixed with `it.vedph.entry-region-parser.xlsx.` rather than `entry-region-parser.tes.`. Pipelines in `tes-tool/Assets` were updated accordingly.
  - ⚠️ resource targets of flags, note, periods and parent item ID moved into TES-specific parsers `col-res-flags`, `col-res-note`, `col-res-periods`, `col-res-parent-item-id`.
  - renamed TES `ImportHelper` to `TesImportHelper` (site resources only).
  - added unit tests for all the TES parsers.

## 0.0.6

- 2026-11-10: updated packages.
- 2026-11-08: updated packages.
- 2026-11-01: adjusted pipeline paths.
- 2026-10-30:
  - added base `ColEntryRegionParser` and generic configurable parsers (`row-item`, `col-metadata`, `col-flags`, `col-note`, `col-periods`, `col-link-attr`, `col-taxo-link`, `col-res-property`), replacing facet-specific duplicates.
  - fixed site import: thesauri IDs (`@en`), periods region tags, site features, evidence/certainty notes overwriting each other.
  - fixed resources import: missing resource for each row, part item IDs, accumulation of multiple resources of the same site, rows with unknown site skipped.
  - fixed inscription import: duplicate `col-coords` tag (now `col-ins-coords`), culture-dependent coordinates parsing, Pleiades links as documented, text lines splitting.
  - added pipelines for site productions, markers, storages, cults, and artifacts.
  - CLI: load all the `item-ids*.json` files in the pipeline folder; items reuse IDs from their own item IDs file.
  - fixed trailing comma in `Thesauri.json`.
- 2026-10-27:
  -	removed iconography item from profile.
  - removed gods categories from cult item (will be replaced by taxonomies via links).
  - added support for taxonomies.
- 2026-10-26:
  - updated packages.
  - added `NotePart:evidence` and `NotePart:certainty` to site profile.
  - completed site import code.
  - changed `SiteResourcesPart`.`date` to `dates` and added `references`.
- 2026-10-15:
  - refactor import to handle multiple items.
  - added categories:evidence to artifact, cult, site with its thesaurus.
- 2026-10-10:
  - added `DateBinsConverter`.
  - migrated tests to MTP platform.
- 2026-10-09: updated packages.
- 2026-10-03: updated packages.
- 2026-09-29:
  - updated packages.
  - added `Cadmus.Tes.Services` and index command to CLI tool.
- 2026-09-22:
  - updated packages.
  - migrated to MTP test platform (<https://learn.microsoft.com/en-us/dotnet/core/testing/migrating-vstest-microsoft-testing-platform>).
- 2026-08-30: updated packages.
- 2026-08-07: fixes to import.
- 2026-08-03: fixes to thesauri import (missing `@en`).
- 2026-07-30: updated thesauri.
- 2026-07-27: updated packages.

## 0.0.5

- 2026-07-27: unified versions.
- 2026-07-25:
  - updated packages.
  - added parsers.
- 2026-07-21: updated packages.
- 2026-06-10: updated packages, with adjustments for Proteus-related components.
- 2026-03-23: added literary sources.
- 2026-03-22:
  - updated packages.
  - added `settings` to profile for facet editing.
- 2026-03-19: updated packages.
- 2026-03-12: updated packages.
- 2026-03-10: updated packages.

## 0.0.3

- 2026-03-03: updated packages.
