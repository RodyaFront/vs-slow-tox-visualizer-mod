# Player Status HUD — API for modders

**Mod id:** `playerstatusstrip`  
**Current `IStatusStripHudApi.ApiVersion`:** `1` (bump when breaking the contract).

## Quick start

1. Add a reference to `PlayerStatusStrip.dll` in your mod project.
2. In client `ModSystem`, get `StatusApi` and register your provider.
3. In provider `Collect()`, append `StatusDescriptor` entries every frame.
4. On dispose/unload, unregister provider to avoid stale references.

```csharp
var baseSys = api.ModLoader.GetModSystem<PlayerStatusStrip.PlayerStatusStripModSystem>();
IStatusStripHudApi? stripApi = baseSys?.StatusApi;
if (stripApi == null) return;
stripApi.RegisterProvider(myProvider);
```

## What the base mod does

- Renders a horizontal strip of status icons with enter/update/exit animation.
- Merges entries from all providers each frame.
- Sorts by `SortOrder`, then `StableId`.
- Resolves duplicate `StableId` by last registered provider wins.
- Uses VTML for tooltip rendering.

## Provider contract

`IStatusStripProvider.Collect(ICoreClientAPI capi, float deltaTime, List<StatusDescriptor> dest)`

- Append to `dest`; do not clear it.
- Keep `StableId` stable across frames for smooth animation/hit-test continuity.
- Return no entry when effect is absent.

### StatusDescriptor fields

| Field | Role |
|---|---|
| `StableId` | Stable namespaced id (`mymod:bleed`). |
| `Icon` | Texture `AssetLocation` (your mod or shared `playerstatusstrip`). |
| `SortOrder` | Lower goes further left in row. |
| `TooltipVtml` | Tooltip body as VTML. |
| `PulseMetric` | Optional value to trigger update pulse on significant change. |
| `AffectKind` | `Neutral` (default), `Positive`, `Negative` animation semantics. |

## Typical usage scenarios

### 1) Minimal binary status
Use one icon while condition is true, remove it otherwise.

### 2) Progress-like status with pulse
Expose normalized metric (for example `0..1`) via `PulseMetric` so updates are visible without changing icon id.

### 3) Semantic feedback by effect polarity
Set `AffectKind.Positive/Negative` to reuse configured profile behavior (shake/slide/scales) without custom renderer code.

### 4) Full gameplay mod integration (reference: SlowToxVisualized)
Treat your mod as a pure data provider:
- compute your gameplay effects in your own code;
- map each active effect to a stable namespaced `StableId`;
- publish one neutral status for your primary state (for example intoxication);
- keep all UX rendering in Player Status HUD, including tooltip and animation execution.

## Config files

Paths are under Vintage Story data path `ModConfig/`.

### Layout config
`playerstatusstrip-hudlayout.json`

- HUD anchor and placement: `DialogArea`, `DialogOffsetX/Y`, `DialogWidth/Height`.
- Strip geometry: `StatusStripOffsetX/Y`, `StatusStripSide`, `StatusStripVerticalAlign`, `StatusIconSize`, `StatusIconGapPx`.
- Animation/layout extras: `StatusStripAnchorMode`, `StatusStripUseLegacyLeadingEdgeRow`, `StatusStripLockRowBaseline`, `NeutralAnim/PositiveAnim/NegativeAnim`.
- Reload with `F8`.

`StatusStripLockRowBaseline` behavior:
- omitted or `true`: shared Y baseline for all icons (no vertical slide/wave offset);
- `false`: allows vertical motion from profile and wave settings.

### Dev config
`playerstatusstrip-dev.json`

- `DevMode`: enables `.stripmock` and `/stripmock` tooling.
- `UseMockStatuses`: defaults to `false`; when `true`, always shows four static mock statuses while no scenario is running.

Production recommendation:
- keep `DevMode=false`;
- keep `UseMockStatuses=false`.

## Mock scenarios and commands

Available scenario ids:
- `meal` — satiety/regen flow
- `mining` — haste to fatigue
- `combat` — damage and recovery
- `weather` — wet/cold pressure
- `recovery` — fatigue to comfort
- `buzz` — rise/peak/fade chain

Commands (with `DevMode=true`):
- `/stripmock list`
- `/stripmock run <id>`
- `/stripmock stop`

Equivalent dot prefix is supported (`.stripmock ...`).

## Dedicated server note

Dedicated server must also have this mod installed so command endpoints exist.  
Client-side provider activation still depends on client config (`DevMode`).

## Versioning and compatibility

- Prefer depending on `game` unless you require specific strip API behavior.
- Check `api.ApiVersion` before relying on newly added features.

## Build reference

Reference `PlayerStatusStrip.dll` from `Mods/playerstatusstrip/` (or your build output).  
Do not reference server-only assemblies unless your mod needs them.
