# История версий Slow Tox Visualized

**Текущая версия** задаётся в [`SlowToxVisualized/modinfo.json`](../SlowToxVisualized/modinfo.json) (поле `version`). Ниже — кратко что менялось по релизам.

## 0.4.3 (текущий срез)

- Тултипы статусов: числовые строки из той же математики, что и иконки; VTML в `lang/en.json` и `lang/ru.json` без лишнего `\n` между заголовком и текстом (единый поток, как у регенерации).
- Выравнивание текста и фона: `ElementBounds` для richtext в **GUI-единицах** (`/ RuntimeEnv.GUIScale`), чтобы текст не «уплывал» относительно подложки.
- Панель тултипа + `GuiElementRichtext`, `StatusTooltipMaxWidth` / `StatusTooltipZ` в конфиге HUD.
- Деплой: скрипт [`scripts/deploy-slowtoxvisualized.ps1`](../scripts/deploy-slowtoxvisualized.ps1), описание в [`DEV_ENV.md`](DEV_ENV.md).
