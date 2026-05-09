# История версий SlowTox Visualized

**Текущая версия** задаётся в [`modinfo.json`](../modinfo.json) (поле `version`). Ниже — кратко что менялось по релизам.

Публичный baseline на ModDB перед текущим freeze: `1.1.9` (`https://mods.vintagestory.at/slowtoxvisualizer`, релиз "1 day ago" на момент проверки).

## Unreleased

- Refactor: удалён legacy standalone HUD-рендерер (`SlowToxIntoxicationHud`, `IntoxicationPalette`, `StatusTooltipPanelBackend`, `UxColorMath`) после перехода на provider-only модель через `Player Status HUD`.
- Cleanup: из `HudLayoutConfig` удалены поля/методы, использовавшиеся только старым рендерером; в `SlowToxStatusStripProvider` удалено копирование неактуальных legacy-полей.
- Поведение в игре не меняется: мод по-прежнему публикует статусы в `Player Status HUD`, без возврата к отдельному HUD-слою.

## 1.1.35

- Документационный cleanup после разделения репозиториев: удалены устаревшие дублирующиеся compatibility-only записи из changelog.
- Актуальная история оставлена только по релизам с отдельными изменениями `SlowToxVisualized`.

## 1.1.9

- Fix: устранён `Translation string format exception` в тултипах статусов (`Lang.Get(..., value)` вместо форматирования уже локализованной строки).
- Build: стабилизированы ссылки на `VintagestoryAPI.dll` через fallback `VintageStoryDir` в `.csproj`.
- Refactor: серия clean-code правок (чистка дублирования, упрощение расчётов и структурирование UI/tooltip логики).
- i18n: добавлены украинские переводы тултипов (`assets/slowtoxvisualized/lang/uk.json`).

## 1.1.8

- Последний публичный baseline перед патч-релизом `1.1.9`.

## 1.1.7

- Дефолты [`HudLayoutConfig`](../src/HudLayoutConfig.cs) для новых JSON приведены к текущей раскладке: `DialogOffsetY` **8**, `MockIntoxicationRaw` **0.1** (для мока). **Player Status HUD 0.1.33+** (тот же `DialogOffsetY` у полоски).

## 1.1.6

- Дефолт `DialogArea`: **RightTop** (правый верх) для новых `slowtoxvisualized-hudlayout.json`. В [`HudLayoutConfig.ParseDialogArea`](../src/HudLayoutConfig.cs) добавлен разбор `RightTop` (раньше неизвестное значение уезжало в `CenterBottom`). **Player Status HUD 0.1.32+**.

## 1.1.5

- Дефолт `StatusIconSize` в [`HudLayoutConfig`](../src/HudLayoutConfig.cs): **46** px (новые `slowtoxvisualized-hudlayout.json`; уже созданный JSON не перезаписывается). Для согласованной полоски статусов — **Player Status HUD 0.1.31+**.

## 1.1.4

- Патч-релиз для публикации: повторно проверено STR (вход в мир → главное меню → снова в мир), статусы и хоткей F9 работают. Рекомендуется **Player Status HUD 0.1.30+** (F8, жизненный цикл HUD, `.striplayout list` без поломки чата); минимум для фикса повторного входа — **0.1.28**.

## 1.1.3

- Исправлен провал `StartClientSide` при повторном входе в мир после выхода в меню: хоткей F9 регистрируется только если его ещё нет в клиентском `HotKeys` (повторный `RegisterHotKeyFirst` давал исключение и ломал весь мод до перезапуска клиента). Нужна сборка **Player Status HUD** с тем же исправлением для F8.

## 1.1.2

- Исправлено исчезновение полоски статусов после выхода в главное меню и повторного входа в мир/на сервер: сброс HUD и провайдера при `LeftWorld`, повторное открытие HUD и регистрация провайдера при `LevelFinalize` (нужна актуальная сборка **Player Status HUD**).

## 1.1.1

- Fixed icon sourcing after migration: SlowToxVisualized statuses now use production icon paths from `slowtoxvisualized` assets, not `Player Status HUD` mock assets.
- Intoxication neutral status is now shown only when intoxication level is greater than `0`.
- Documentation clarified that `playerstatusstrip` `mock_*` icons are strictly dev/mock-only and must not be used for production status visuals.

## 1.1.0

- **Migration:** SlowToxVisualized now integrates with **Player Status HUD** (`playerstatusstrip`) as a status provider instead of rendering its own standalone HUD layer.
- Intoxication is now a regular neutral status in the shared strip; its tooltip shows the current intoxication level.
- Added hard dependency on `playerstatusstrip` in `SlowToxVisualized/modinfo.json`.

## 1.0.2

- Единое написание **SlowTox** (зависимость) в `modinfo.json`, лендинге и префиксах логов **SlowTox Visualized**.
- Предупреждение про `gear.svg`: отдельные сообщения, если ассет не найден в пакете мода (часто из‑за копирования только DLL) и если файл есть, но растеризация SVG не удалась.

## 1.0.1

- **Продакшн:** в коде по умолчанию `UseMockIntoxicationOverride` = **false** — HUD берёт `intoxication` с сущности игрока; мок по-прежнему включается в `slowtoxvisualized-hudlayout.json` для скринов и отладки.

## 1.0.0

- Публичная базовая версия клиентского HUD (интоксикация, палитра, полоска статусов SlowTox, конфиг, F9).

## 0.4.3

- Тултипы статусов: числовые строки из той же математики, что и иконки; VTML в `lang/en.json` и `lang/ru.json` без лишнего `\n` между заголовком и текстом (единый поток, как у регенерации).
- Выравнивание текста и фона: `ElementBounds` для richtext в **GUI-единицах** (`/ RuntimeEnv.GUIScale`), чтобы текст не «уплывал» относительно подложки.
- Панель тултипа + `GuiElementRichtext`, `StatusTooltipMaxWidth` / `StatusTooltipZ` в конфиге HUD.
- Деплой: запуск `dotnet build -c Release` из корня репозитория (single-mod структура), далее плоское копирование в `Mods\slowtoxvisualized` по `DEV_ENV.md`.
