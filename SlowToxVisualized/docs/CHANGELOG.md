# История версий SlowTox Visualized

**Текущая версия** задаётся в [`SlowToxVisualized/modinfo.json`](../modinfo.json) (поле `version`). Ниже — кратко что менялось по релизам.

**Player Status HUD** (`playerstatusstrip`): версия в [`PlayerStatusStrip/modinfo.json`](../../PlayerStatusStrip/modinfo.json); краткая история в конце файла.

## 1.1.9

- Публичный patch-релиз после ModDB baseline `1.1.8`.
- Включён фикс синхронизации анимации иконок статусов на первом кадре.
- Добавлена clean-code консолидация расчётов эффектов и тултипов:
  - вынесен общий реестр иконок (`SlowToxHudEffectIcons`),
  - унифицирована математика poison через `SlowToxHudDefaults` (`PoisonDamagePerTick` / `PoisonDamagePerSecond`),
  - убрано дублирование в `SlowToxStatusTooltipContent`.
- Повышена стабильность сборки в IDE: добавлен fallback-путь к DLL Vintage Story в `SlowToxVisualized.csproj`, если `VINTAGE_STORY` не подхвачен.
- Сохранены улучшенные диагностические предупреждения по `gear.svg` и консистентный нейминг `SlowTox`.
- i18n: добавлена `uk`-локализация tooltip-строк для эффектов SlowTox в status strip (`slowtoxvisualized/lang/uk.json`).

## 1.1.37

- Clean-code проход по `SlowToxVisualized/src` без изменения игровой логики: вынесен общий реестр иконок эффектов (`SlowToxHudEffectIcons`), убрано дублирование маппинга `effect kind -> icon` между HUD и provider.
- Формула poison и связанные константы собраны в `SlowToxHudDefaults` (`PoisonDamagePerTick` / `PoisonDamagePerSecond`), чтобы и probe, и tooltip использовали один источник правды.
- В `SlowToxIntoxicationHud` вынесены повторяющиеся константы для tooltip-компоновки (`TooltipComposeHeightPx`, `TooltipGapBelowPx`) и ранний `EnsureDefaults()` в `ComposeHud`.

## 1.1.36

- Исправлен спам предупреждений `Translation string format exception` в `client-main.log` при активных статусах интоксикации: тултипы теперь форматируются через `Lang.Get(key, args)` без промежуточного вызова `Lang.Get(key)` с пустыми аргументами.

## 1.1.35

- Обновлён compatibility target: рекомендуется **Player Status HUD 1.2.0+**.

## 1.1.34

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.33

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.32

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.31

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.30

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.29

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.28

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.27

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.26

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.25

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.24

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.23

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.22

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.21

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.20

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.19

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.18

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.17

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.16

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.15

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.14

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.13

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.12

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.11

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.10

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.9

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

## 1.1.8

- Compatibility sync с эволюцией `Player Status HUD` (без отдельных изменений логики `SlowToxVisualized`).

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
- Деплой: скрипт [`scripts/deploy-slowtoxvisualized.ps1`](../../scripts/deploy-slowtoxvisualized.ps1), описание в [`DEV_ENV.md`](DEV_ENV.md).

---

## Player Status HUD (`playerstatusstrip`)

Источник версии: [`PlayerStatusStrip/modinfo.json`](../../PlayerStatusStrip/modinfo.json).

### 1.2.0

- Пакетный релиз после последней опубликованной версии `0.1.33`: включает все изменения итераций `0.1.34 ... 0.1.63`.
- Layout engine: миграция на edge-based позиционирование (`StatusStripScreenPlacement`) и устранение size-dependent смещений/«скрытой базы» для `*Bottom` якорей.
- Wizard UX: пресеты inset/size/gap, корректное центрирование `CenterTop`, удалён `CenterBottom` из выбора (с fallback при чтении старых конфигов), лейбл `Inset`.
- Tooltip behavior: для нижних якорей приоритет рендера сверху с безопасным авто-flip.
- Runtime consistency: финальная схема хоткеев `F8` (reload layout) и `Ctrl+F8` (open/close wizard), плюс корректная регистрация модификаторов.
- Документация и релизные материалы обновлены: markdown changelog, HTML changelog, релизные архивы.

### 0.1.63

- Hotkey mapping tweak: reload layout возвращён на `F8`, wizard остаётся на `Ctrl+F8`.
- Wizard UX copy: `Inset from corner` -> `Inset`.

### 0.1.62

- Hotkey modifiers fix: исправлен порядок bool-параметров `RegisterHotKeyFirst` для wizard hotkey (`alt, ctrl, shift`), поэтому биндинг теперь реально `Ctrl+F8` (а не `Alt+F8`).

### 0.1.61

- Hotkey fix: `playerstatusstrip_reloadlayout` переведён на отдельный key id и `F9`, чтобы не перехватывать `Ctrl+F8` у layout wizard.

### 0.1.60

- Center alignment fix: для center-якорей `StripLayoutWizardStripSide` теперь использует runtime side `Center`, а `StatusStripLayoutMath` центрирует всю полосу статусов по ширине контейнера.

### 0.1.59

- Wizard layout corners: `CenterBottom` удалён из списка `Screen corner`; добавлен совместимый fallback `CenterBottom -> CenterTop` при чтении текущего `DialogArea` в мастере.

### 0.1.58

- Bottom-anchor baseline fix: в `StatusStripLayoutMath` добавлен явный runtime-режим вертикального выравнивания (`top/center/bottom`) на основе `DialogArea`; для `*Bottom` ряд иконок прижат к нижней линии контейнера (без size-dependent slack).

### 0.1.57

- Tooltip placement: для нижних якорей (`*Bottom`) добавлен приоритет рендера тултипа сверху иконки + fallback-flip по свободному месту во вьюпорте.

### 0.1.56

- `StatusStripHudElement.ResolveDialogBounds()` теперь якорит `hud-container` по effective container size (минимум = `StatusIconSize`), поэтому размер иконки влияет только на геометрию ряда, а не на edge-позицию контейнера.

### 0.1.55

- Iteration 1: `StatusStripLayoutMath` и `StatusStripHudElement.BuildStripLayout()` переведены на inward-flow модель от краёв (`Right*` → влево, `Left*` → вправо) с runtime-канонизацией `StatusStripSide` по `DialogArea`; базовые `StatusStripOffsetX/Y` и vertical-align больше не добавляют «скрытую базу» для ряда иконок.

### 0.1.54

- Добавлен `StatusStripScreenPlacement`: `ComposeHud()` теперь ставит HUD в абсолютные экранные координаты (GUI-space) по `DialogArea + DialogOffsetX/Y`, без опоры на внутренние margin-ы `ElementBounds.Fixed(EnumDialogArea, ...)`.

### 0.1.53

- Left* inset fix: компенсация `LeftDialogMargin` перенесена из пресетов в `StatusStripLayoutConfig.DialogBounds()`, чтобы `DialogOffsetX` в конфиге оставался «чистым» шагом inset, а итоговая позиция в игре учитывала margin ровно один раз.

### 0.1.52

- `StripLayoutInsetPresets`: для **Left\*** якорей `DialogOffsetX` = **k − LeftDialogMargin** (с зажимом), чтобы inset не суммировался с базовым левым margin диалогов движка.

### 0.1.51

- Мастер: `StripLayoutWizardStripSide.ForDialogArea` — для якорей Left/Center задаётся `StatusStripSide: Right`, для Right — `Left`; см. `StripLayoutWizardDialog.TryBuildConfig`.

### 0.1.50

- `en.json`: пресеты размера иконок в мастере — Small / Medium / Large / Huge для 32 / 42 / 64 / 78 px.

### 0.1.49

- Мастер: пресеты inset 4/8/16/32, иконки 32/42/64/78; `StripLayoutInsetPresets` — смещения по коду угла; подправка Y для верхних якорей; верстка intro / отступ перед Layout.

### 0.1.48

- Мастер раскладки: пресеты вместо `NumberInput` для размера иконок, зазора и смещения от угла; новые ключи `en.json` (`wizard-preset-*`, `wizard-label-inset` и т.д.).

### 0.1.47

- `en.json`: `wizard-footer-hint` — вместо Unicode **★** используется **\*** (совместимость с шрифтом диалогов).

### 0.1.46

- Мастер раскладки: ширина **390** px (было 350); зарезервированы высоты под intro/footer и двухстрочную подпись к полю зазора; окно по высоте под сумму блоков + нижний отступ.

### 0.1.45

- Мастер раскладки: размер окна ~**350×470** GUI px (раньше ~700×648); подписи, поля и выпадающий список подогнаны под узкий диалог.

### 0.1.44

- Мастер раскладки: кнопки — шрифт кнопки с `WithOrientation(EnumTextOrientation.Center)`; нижняя подсказка (`wizard-footer-hint`) — `FontWeight.Bold` + тёплый цвет через `CairoFont.WithColor`; текст в `en.json` с префиксом «★ Tip — …».

### 0.1.43

- Мастер раскладки: заголовки секций **Layout** / **Appearance** — **жирный** шрифт (`FontWeight.Bold`), чтобы визуально отделять их от подписей полей.

### 0.1.42

- Мастер раскладки: секции **Layout** / **Appearance**; порядок полей угол экрана → позиция → размер иконок → зазор; подписи полей мелким шрифтом; кнопки **Save & apply** (`EnumButtonStyle.Normal`) и **Not now** (`Small`); переписаны `wizard-intro`, `wizard-footer-hint`, ключ `wizard-not-now`.

### 0.1.41

- Мастер раскладки: из подсказки внизу окна убрано упоминание стрелок/Shift для числовых полей.

### 0.1.40

- Мастер раскладки: убраны поля **Strip offset**; при **Apply** значения `StatusStripOffsetX` / `StatusStripOffsetY` берутся из уже загруженного конфига (не сбрасываются).

### 0.1.39

- Мастер раскладки: увеличены размер окна и области для `AddStaticTextAutoBoxSize` (ширина = ширина диалога минус отступы); в `en.json` добавлены явные переводы строк для вводного и нижнего текста.

### 0.1.38

- Мастер раскладки: в сфокусированном числовом поле **Up/Down** меняют значение на ±1, **Shift+Up/Down** на ±10; превью обновляется как при вводе.

### 0.1.37

- Dev: в [`StatusStripDevConfig`](../../PlayerStatusStrip/src/StatusStripDevConfig.cs) поле **`AlwaysAutoLayoutWizard`** (учитывается только вместе с **`DevMode`**) — авто-открытие мастера при каждом `LevelFinalize`, без учёта `SuppressAutoLayoutWizard`.

### 0.1.36

- Онбординг мастера: при смене **версии мода** (из списка модов игры) сбрасывается `SuppressAutoLayoutWizard`, если ранее игрок уже закрывал мастер для **старой** версии — авто-показ снова возможен после каждой обновы. В `playerstatusstrip-onboarding.json` хранится `WizardDismissedForModVersion`.

### 0.1.35

- Мастер раскладки: при открытом окне на полоску добавляются **те же четыре мок-иконки**, что в dev-режиме; раскладка **обновляется при изменении** якоря, размера, смещений и зазора (`ApplyLayoutPreview`). После **Skip** / закрытия без **Apply** — тихий откат к конфигу с диска; после **Apply** — без дублирующего длинного чата F8 (остаётся краткое уведомление мастера).

### 0.1.34

- Мастер раскладки (GUI): пресет якоря (`DialogArea`), размер иконок, смещения HUD/полоски, зазор; **Apply** пишет `playerstatusstrip-hudlayout.json` и перезагружает HUD; **Skip** / закрытие без принудительного сохранения раскладки, но выставляет `SuppressAutoLayoutWizard` в `playerstatusstrip-onboarding.json`, чтобы авто-показ больше не мешал.
- Авто-показ мастера один раз (если флаг не подавлен), ~650 ms после успешного открытия HUD.
- Хоткей **Ctrl+F8** (регистрация только если слот свободен) и команда `.striplayout wizard` / `setup` для повторного открытия; при выходе из мира мастер закрывается без подавления онбординга (если игрок не завершил шаг — авто снова возможен в следующей сессии).

### 0.1.33

- Дефолт `DialogOffsetY`: **8** для новых `playerstatusstrip-hudlayout.json` (согласовано с правым верхним якорем).

### 0.1.32

- Дефолт `DialogArea`: **RightTop** (правый верх) для новых `playerstatusstrip-hudlayout.json`.

### 0.1.31

- Дефолт `StatusIconSize` в [`StatusStripLayoutConfig`](../../PlayerStatusStrip/src/StatusStripLayoutConfig.cs): **46** px для новых `playerstatusstrip-hudlayout.json` (`0` по-прежнему означает авто из раскладки, если явно задать в файле).

### 0.1.30

- Тексты для `ShowChatNotification` (`.striplayout` / `.stripmock`, список ключей): без сырых `<`/`>` в разметке чата (Vtml), чтобы не ломать UI чата и не сыпать ошибками парсера в лог.

### 0.1.29

- Команда `.striplayout list` (и `/striplayout list`): вывод в чат всех полей раскладки, которые можно менять через `get`/`set`, с кратким описанием; блоки `NeutralAnim` / `PositiveAnim` / `NegativeAnim` по-прежнему только через `playerstatusstrip-hudlayout.json`.

### 0.1.28

- Исправление повторной регистрации хоткея F8 при повторном входе в мир; жизненный цикл HUD (`LeftWorld` / `LevelFinalize`) в связке со SlowTox Visualized 1.1.2+.
