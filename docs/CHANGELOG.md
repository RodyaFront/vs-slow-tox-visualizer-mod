# История версий SlowTox Visualized

**Текущая версия** задаётся в [`SlowToxVisualized/modinfo.json`](../SlowToxVisualized/modinfo.json) (поле `version`). Ниже — кратко что менялось по релизам.

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
- Деплой: скрипт [`scripts/deploy-slowtoxvisualized.ps1`](../scripts/deploy-slowtoxvisualized.ps1), описание в [`DEV_ENV.md`](DEV_ENV.md).
