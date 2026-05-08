# Player Status HUD — Dev Guide

## Goal

Keep production behavior clean (no placeholder statuses by default) while preserving mock tooling for fast HUD iteration.

## Config checklist

`ModConfig/playerstatusstrip-dev.json`:

```json
{
  "DevMode": true,
  "UseMockStatuses": false
}
```

- `DevMode=true` enables mock commands.
- `UseMockStatuses=false` avoids always-on placeholder statuses.

Production values:

```json
{
  "DevMode": false,
  "UseMockStatuses": false
}
```

## Mock workflow

1. List scenarios: `/stripmock list`
2. Start scenario: `/stripmock run recovery` (or other id)
3. Stop scenario: `/stripmock stop`
4. Reload layout after JSON edits: `F8`

Scenario ids:
- `meal`
- `mining`
- `combat`
- `weather`
- `recovery`
- `buzz`

## Pre-release checklist

1. Verify `DevMode=false` and `UseMockStatuses=false` in your production config baseline.
2. Build release:
   - `dotnet build -c Release PlayerStatusStrip/PlayerStatusStrip.csproj`
3. Run tests:
   - `dotnet test -c Release -p:UseSharedCompilation=false PlayerStatusStrip/PlayerStatusStrip.Tests/PlayerStatusStrip.Tests.csproj`
4. Bump `PlayerStatusStrip/modinfo.json` version.
5. Deploy:
   - `PlayerStatusStrip/scripts/deploy-playerstatusstrip.ps1`
6. In-game smoke:
   - check mod version in mod list;
   - `/stripmock list`, `/stripmock run recovery`, `/stripmock stop`;
   - confirm no statuses are shown when no scenario is running.
