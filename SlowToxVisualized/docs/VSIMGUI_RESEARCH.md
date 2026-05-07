# VintageStory_ImGui (maltiez2) — заметки по интеграции

Источник: репозиторий [maltiez2/VintageStory_ImGui](https://github.com/maltiez2/VintageStory_ImGui). Локальная копия для разбора: `VintageStory_ImGui-upstream/` (при необходимости обновить `git pull --recurse-submodules`).

Публичная страница мода на сайте модов: [mods.vintagestory.at/imgui](https://mods.vintagestory.at/imgui).

## Что это

Отдельный **клиентский мод** (`modid`: **`vsimgui`**, в проекте имя «Dear ImGui»), который встраивает **Dear ImGui** в Vintage Story через **ImGui.NET** (форк/биндинги) и контроллер на базе **OpenTK** (подмодуль `ImGui.NET_OpenTK_Sample_multi-viewports`). Поддерживаются **мульти-viewport** (окна ImGui могут выноситься в отдельные нативные окна), на macOS по умолчанию в конфиге отключается multi-viewport.

Второй артефакт — NuGet-пакет **`VSImGui_DebugTools`**: статические **`DebugWidgets`** для быстрых отладочных окон без ручной разметки каждого виджета.

## Состав репозитория

| Часть | Назначение |
|-------|------------|
| `VSImGui/` | Сам мод: `ImGuiModSystem`, интеграция с окном игры, диалог VS, рендер |
| `VSImGui_DebugTools/` | Библиотека виджетов для отладки (`DebugWidgets`, `DebugWindowsManager`) |
| `ImGui.NET_OpenTK_Sample_multi-viewports/` | Подмодуль: `ImGuiController`, рендерер, нативные библиотеки |

Пакеты на NuGet: [VSImGui](https://www.nuget.org/packages/VSImGui), [VSImGui_DebugTools](https://www.nuget.org/packages/VSImGui_DebugTools) (версии см. на сайте; в README указаны примеры `dotnet add package`).

## Точка входа для других модов: `IImGuiRenderer`

`ImGuiModSystem` реализует **`IImGuiRenderer`** (`VSImGui.API`). Главное для подписчиков:

- Событие **`Draw`** типа **`DrawCallbackDelegate(float deltaSeconds)`** → возвращает **`CallbackGUIStatus`**:
  - **`Closed`** — в этом колбэке нет открытых окон ImGui;
  - **`GrabMouse`** — нужен курсор и захват мыши (диалоговый режим);
  - **`DontGrabMouse`** — окна есть, но без «полного» захвата (в т.ч. immersive mouse).

Правила из XML-документации: внутри колбэка каждый `ImGui.Begin` закрывать `End`, `Push`/`Pop` и стили — симметрично.

- **`Show()`** — форсированно открыть слой VS, чтобы новые окна получили ввод.
- **`Closed`** — когда игрок закрыл все ImGui-окна (Escape / хоткей).
- **`DefaultStyle`** — стиль по умолчанию из ассета `vsimgui:config/defaultstyle.json`.

Получение API в клиентском коде:

```csharp
var imgui = api.ModLoader.GetModSystem<ImGuiModSystem>(); // IImGuiRenderer
imgui.Draw += MyDrawCallback;
```

Удобные обёртки: **`ImGuiDialogBase`** / **`ImGuiDialogWindow`** — сами подписываются на `Draw`/`Closed` и управляют `Open`/`Close`.

## Связка с GUI Vintage Story

Рендер и ввод проходят через **`GuiDialog`** (`VSImGuiDialog`): хоткей **`imguitoggle`** (по коду: **`GlKeys.P`**, модификаторы как в регистрации), тип диалога переключается между HUD и полноценным диалогом в зависимости от **`GrabMouse`**. Ввод с клавиатуры/мыши помечается как обработанный, если ImGui запросил захват (`Controller.KeyboardCaptured`, `MouseCaptured`, и т.д.).

Дополнительно зарегистрирован рендер **`OffWindowRenderer`** на стадии **`EnumRenderStage.Ortho`** для окон вне главного viewport.

Шрифты: **`FontManager`** подгружает TTF из каталога игры (`GamePaths.AssetsPath` → `game/fonts/...`), глифы зависят от **`Lang.CurrentLocale`**. Есть событие **`FontManager.BeforeFontsLoaded`** для добавления своих путей и размеров (атлас ограничен).

Конфиг ImGui на диске: **`GamePaths.ModConfig/imgui.ini`** (расположение окон).

Мод-конфиг: **`imgui.json`** — класс **`ImGuiConfig`**, поле **`MultiViewportSupport`** (для OSX в `StartPre` принудительно `false`, если конфиг создаётся впервые).

## Отладочные виджеты (`VSImGui_DebugTools`)

Класс **`DebugWidgets`** (частичный `partial`): методы вроде **`Text`**, **`Draw`**, **`IntSlider`**, кнопки, чекбоксы и др. Параметры **`domain`** (заголовок/ID окна) и **`category`** (вкладка). Регистрация идёт в статические структуры **`DebugWindowsManager`**, отрисовка вызывается из **`ImGuiModSystem`** через **`DebugWindowsManager.Draw()`**.

По README: чтобы пользоваться пакетом, нужны и **установленный мод ImGui**, и ссылка на пакет; для релиза вызывать только под **`#if DEBUG`** или убирать вызовы, чтобы не тянуть зависимость у конечных пользователей.

## Зависимости сборки из исходников

В **`VSImGui.csproj`** зашиты пути автора (например **`GameDirectory`**, **`ImGuiController_OpenTK`** под `net7.0` в HintPath, при этом таргет фреймворка мода **`net10.0`**). Для своей машины пути и версии нужно **переопределить** под установку Vintage Story и собранный контроллер. Для потребителя проще ставить готовый мод с [каталога модов](https://mods.vintagestory.at/imgui) или подключать **NuGet** там, где это поддерживается вашим пайплайном.

## Плюсы и ограничения для SlowToxVisualized

**Плюсы:** быстрые итерации UI (immediate mode), графики через **ImPlot** / узлы через **ImNodes** (DLL присутствуют в структуре пакета), отладочные панели без Cairo/`GuiComposer`.

**Ограничения:** игрок должен иметь мод **`vsimgui`**; визуальный стиль ImGui отличается от нативного VS GUI; нужно корректно возвращать **`CallbackGUIStatus`**, чтобы не ломать управление камерой; производительность и порядок отрисовки — как у любого оверлея на каждый кадр.

## Связанные документы

- [VISUALIZATION_OPTIONS.md](./VISUALIZATION_OPTIONS.md) — общий обзор путей визуализации (ванильный HUD + этот раздел).
- [DEV_ENV.md](./DEV_ENV.md) — путь к игре и переменная `VINTAGE_STORY`.

---

*При обновлении upstream проверяйте версию в `VSImGui.csproj` (`ModVersion` / `Version`) и актуальность README на GitHub.*
