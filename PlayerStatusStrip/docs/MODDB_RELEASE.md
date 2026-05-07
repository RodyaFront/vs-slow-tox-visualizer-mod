# Player Status HUD — ModDB Release Draft

Use this file as a reusable template. Fill values from `modinfo.json` and the current release artifact, do not hardcode them in source docs.

## Release metadata (fill per release)

- Version: `<from modinfo.json>`
- Mod id: `<from modinfo.json>`
- Package file: `<from release/*.zip>`
- SHA256: `<from Get-FileHash>`

---

## RU: Краткое описание

Player Status HUD — библиотека/HUD-мод для Vintage Story, которая рисует горизонтальную полосу статус-иконок игрока.
Мод предоставляет API для других модов: они могут регистрировать свои статусы (иконка, tooltip, приоритет, метрика пульса, семантика эффекта).

### Основные возможности

- Общая полоса статусов с tooltip (VTML).
- Семантические профили анимации: `Neutral`, `Positive`, `Negative`.
- Конфигурируемый layout (`ModConfig/playerstatusstrip-hudlayout.json`).
- Dev-инструменты mock-сценариев для тестирования:
  - `/stripmock list`
  - `/stripmock run <id>`
  - `/stripmock stop`
- По умолчанию моки выключены для production:
  - `UseMockStatuses = false`

### Установка

1. Распакуйте релизный архив.
2. Скопируйте папку `playerstatusstrip` в `VintagestoryData/Mods/`.
3. Проверьте версию мода в списке модов игры.

### Для мододелов

- API-документация: `PlayerStatusStrip/docs/PLAYER_STATUS_STRIP_API.md`
- Dev-guide: `PlayerStatusStrip/docs/PLAYER_STATUS_STRIP_DEV_GUIDE.md`

### Что нового (вставьте пункты релиза)

- <bullet 1>
- <bullet 2>
- <bullet 3>

---

## EN: Short description

Player Status HUD is a base HUD/library mod for Vintage Story that renders a horizontal row of player status icons.
It exposes an API so other mods can register statuses (icon, tooltip, sort order, pulse metric, effect kind).

### Key features

- Shared status row with VTML tooltips.
- Semantic animation profiles: `Neutral`, `Positive`, `Negative`.
- Fully configurable layout (`ModConfig/playerstatusstrip-hudlayout.json`).
- Mock scenario tooling for development:
  - `/stripmock list`
  - `/stripmock run <id>`
  - `/stripmock stop`
- Production-safe defaults:
  - `UseMockStatuses = false` by default.

### Installation

1. Extract the release archive.
2. Copy the `playerstatusstrip` folder into `VintagestoryData/Mods/`.
3. Verify mod version in the in-game mod list.

### For mod developers

- API documentation: `PlayerStatusStrip/docs/PLAYER_STATUS_STRIP_API.md`
- Dev guide: `PlayerStatusStrip/docs/PLAYER_STATUS_STRIP_DEV_GUIDE.md`

### Changelog highlights (fill per release)

- <bullet 1>
- <bullet 2>
- <bullet 3>
