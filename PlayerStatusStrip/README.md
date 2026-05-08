# Player Status HUD

Developer-focused status HUD library for Vintage Story.

`modid`: `playerstatusstrip`  
Version/source-of-truth: `modinfo.json`

## What this mod provides

- Horizontal player status icon strip with VTML tooltips.
- Semantic animation profiles (`Neutral`, `Positive`, `Negative`).
- Provider API for other mods (`IStatusStripHudApi`, `IStatusStripProvider`).
- Dev mock tooling for scenario-based UI testing.
- Production-safe defaults (mock placeholders are disabled unless explicitly enabled).

## Documentation

- [API reference and integration guide](docs/PLAYER_STATUS_STRIP_API.md)
- [Dev/testing guide and release checklist](docs/PLAYER_STATUS_STRIP_DEV_GUIDE.md)
- [ModDB release draft template (RU/EN)](docs/MODDB_RELEASE.md)
- [Landing page HTML (ModDB style)](docs/mod-landing.html)

## Key config files

- `ModConfig/playerstatusstrip-hudlayout.json` — HUD layout and animation profile settings.
- `ModConfig/playerstatusstrip-dev.json` — dev mode and mock behavior.

## Dev commands

With `DevMode=true`:

- `/stripmock list`
- `/stripmock run [id]`
- `/stripmock stop`

Layout config chat commands (client-side):

- `.striplayout help`
- `.striplayout list` (all keys supported by `get`/`set`, with short descriptions)
- `.striplayout show`
- `.striplayout get [key]`
- `.striplayout set [key] [value]`
- `.striplayout reload`

## Build, test, deploy

From repository root:

```powershell
dotnet build -c Release PlayerStatusStrip/PlayerStatusStrip.csproj
dotnet test -c Release -p:UseSharedCompilation=false PlayerStatusStrip/PlayerStatusStrip.Tests/PlayerStatusStrip.Tests.csproj
./PlayerStatusStrip/scripts/deploy-playerstatusstrip.ps1
```
