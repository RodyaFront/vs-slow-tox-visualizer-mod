# Player Status HUD changelog

Source of truth version: [`PlayerStatusStrip/modinfo.json`](../modinfo.json).

Public baseline on ModDB before this release: `1.0.0`.

## 1.0.1

- Public patch release after `1.0.0`.
- Fix: stabilized edge-based HUD positioning across anchors.
- Fix: corrected center-anchor behavior for scaled status icons.
- i18n: added `ru` and `uk` translations for the layout wizard.
- UX: retitled wizard window to "Status strip settings" with synchronized `en`/`ru`/`uk` translations.
- UI: refactored wizard dialog into explicit `Header`, `Form`, `Tip`, and `Actions` layout sections.
- UI: migrated wizard positioning to flow-based bounds calculation; sections now adapt to localized text height without overlap.
- UX: switched negative status update pulse to horizontal shake.
- UX: wizard preview now uses mock-only icons (no production icon leakage in preview).
- Keeps provider API and integration model stable for dependent mods.

Detailed release notes are in `docs/changelog-*.html`.
