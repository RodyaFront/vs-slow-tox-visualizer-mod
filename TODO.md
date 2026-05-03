# TODO — Slow Tox Visualized

Мод (`modid`: `slowtoxvisualized`). Дополняйте по мере работы.

## План (порядок шагов)

1. **Конвейер сборки** — минимальный загружаемый мод: `modinfo.json` (`modid`: `slowtoxvisualized`), пустой `ModSystem`, `dotnet build`, копия артефакта в `VS_MODS_TEST` (или вывод сразу туда); проверка, что игра грузит мод без ошибок.
2. **Мок полоски** — на клиенте `HudElement` (или HUD-режим диалога): полоска с **захардкоженными** значениями, без привязки к SlowTox и сети.
3. **Живые данные** — подставить `intoxication` / при необходимости `slowtox:*` из `WatchedAttributes` локального игрока; если чего-то нет на клиенте — отдельно: синхронизация / клиентский behavior.
4. **Зависимость** — в `modinfo.json` зафиксировать `slowtox`, когда визуализация от него зависит.

---

## В работе / очередь

- [ ] П.3: подключить реальные атрибуты игрока.
- [ ] П.4: зависимость от `slowtox` в `modinfo.json`.

## Готово

- [x] П.1: конвейер сборки — проект [`SlowToxVisualized/SlowToxVisualized.csproj`](SlowToxVisualized/SlowToxVisualized.csproj), выход `bin/.../Mods/slowtoxvisualized/`, копия в `%AppData%\VintagestoryData\Mods` (2026-05-03). Проверка в клиенте: вручную убедиться, что мод в списке и в логе есть `[Slow Tox Visualized] Mod loaded.`
- [x] П.2: мок HUD — [`IntoxicationMockHud`](SlowToxVisualized/src/IntoxicationMockHud.cs): иконка PNG (`assets/.../intox_icon.png`), полоска **3.5 / 10**, позиция низ по центру (`EnumDialogArea.CenterBottom`). Проверка в игре вручную; при занятом файле DLL закройте клиент перед копией в `Mods`.
