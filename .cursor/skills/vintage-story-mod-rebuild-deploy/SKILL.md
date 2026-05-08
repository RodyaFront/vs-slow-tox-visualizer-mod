---
name: vintage-story-mod-rebuild-deploy
description: Rebuilds this repository’s Vintage Story C# mods with dotnet Release and copies the flattened mod folder into the client Mods directory (VINTAGE_STORY for compile, VS_MODS_TEST or default data Mods for deploy). Use when the user invokes this skill or asks to rebuild and copy the mod to Mods, redeploy, or sync the build to Vintage Story without doing it manually.
disable-model-invocation: true
---

# Vintage Story mod: rebuild and copy to Mods

## When this runs

The user turned this skill on so the agent **finishes the loop**: after edits, **build Release** and **deploy flat** into Vintage Story `Mods/<modid>/` without them having to ask again for that step.

## Single source of truth

1. Read **[`SlowToxVisualized/docs/DEV_ENV.md`](../../../SlowToxVisualized/docs/DEV_ENV.md)** for this machine: `VINTAGE_STORY`, `VS_MODS_TEST`, log paths.
2. If `VINTAGE_STORY` is missing in the shell, set it for the session to the path from that doc (or from `.vscode/settings.json` terminal env).

## Rules (do not skip)

- **Flat copy:** copy **`bin/Release/Mods/<modid>/*`** into **`.../Mods/<modid>/`**, never a nested `Mods/<modid>/<modid>/`.
- **Game closed:** if `Copy-Item` fails because `*.dll` is locked, tell the user to close the Vintage Story client and retry the copy (or full deploy).
- **Which mod:** If the last changes clearly touch only one project, deploy **that** mod. If both changed or unclear, **build and copy both** mods listed below.

## Mods in this repo

| Mod id | Project path | Build output folder |
|--------|----------------|---------------------|
| `slowtoxvisualized` | `SlowToxVisualized/SlowToxVisualized.csproj` | `SlowToxVisualized/bin/Release/Mods/slowtoxvisualized/` |
| `playerstatusstrip` | `PlayerStatusStrip/PlayerStatusStrip.csproj` | `PlayerStatusStrip/bin/Release/Mods/playerstatusstrip/` |

## Deploy destination

- If **`VS_MODS_TEST`** is set and non-empty: `Join-Path $env:VS_MODS_TEST.TrimEnd('\','/') '<modid>'`.
- Else (default): `%AppData%\VintagestoryData\Mods\<modid>` (see DEV_ENV for the user’s resolved path).

Deploy scripts apply the same flat-copy pattern as below:

```powershell
Remove-Item -Path "$dst\*" -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item -Path "$src\*" -Destination $dst -Recurse -Force
```

## Commands to execute

From repository root, with `VINTAGE_STORY` set:

**SlowTox Visualized:**

```powershell
.\scripts\deploy-slowtoxvisualized.ps1
```

**Player Status Strip:**

```powershell
.\PlayerStatusStrip\scripts\deploy-playerstatusstrip.ps1
```

Each script prints **`Done. modinfo version: …`** from the copied `modinfo.json`. When deploying **both** mods in one go, run both scripts and **repeat both version lines** in the user-facing summary.

If the user’s shell is not PowerShell, translate the same steps (flat copy, same paths as in [`deploy-slowtoxvisualized.ps1`](../../../scripts/deploy-slowtoxvisualized.ps1) / [`deploy-playerstatusstrip.ps1`](../../../PlayerStatusStrip/scripts/deploy-playerstatusstrip.ps1)).

## After deploy

Remind briefly: confirm **`version`** in the in-game mod list matches the script output; full client restart if assets/lang changed; logs under data `Logs/` per DEV_ENV.
