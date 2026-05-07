# Варианты визуализации в моддинге Vintage Story

Документ можно **дополнять**: новые разделы, подразделы или строки в таблицах — сохраняйте единый стиль и при необходимости обновляйте блок «Ссылки» внизу.

## Назначение

Сводка способов показать игроку состояние (опьянение, бафы, очереди SlowTox и т.д.): экранный HUD, окна, частицы в мире, интеграция с другими модами.

---

## 1. Экранный интерфейс (2D)

Клиентский код: `ModSystem` с `ShouldLoad` / `StartClientSide`, доступ к **`ICoreClientAPI`** (`capi`).

| Вариант | Класс / API | Когда использовать |
|--------|----------------|-------------------|
| Оверлей без захвата мыши | **`HudElement`** (наследование; база как у HUD игры) | Постоянные или условные полоски, иконки, текст рядом с хотбаром |
| Полноценное окно с кнопками | **`GuiDialog`** | Настройки, справка, отладочная панель по хоткею |
| Сборка из виджетов | **`capi.Gui.CreateCompo`**, **`GuiComposer`** | Стандартный способ собрать layout (цепочка `.Add…().Compose()`) |
| Полоска числового значения | **`GuiElementStatbar`** (`GuiComposer.AddStatbar` / `AddInvStatbar`) | Шкала 0…N (интоксикация, подочередь и т.д.) |
| Текст | **`GuiElementStaticText`** и варианты авторазмера | Подписи, проценты, подсказки |
| Произвольная отрисовка | **`GuiElementCustom`**, переопределение **`OnRenderGUI`** | Нестандартная графика |
| Управление показом окон | **`IGuiAPI`**: `RegisterDialog`, фокус, границы | Открытие/закрытие, перекрытие с другим UI |

**Хоткеи:** `capi.Input.RegisterHotKey`, `SetHotKeyHandler` — открыть/закрыть свой диалог или переключить HUD.

**Рисование:** элементы опираются на **Cairo** (`ComposeElements`, текстуры из поверхностей); паттерны и картинки из ассетов через API элементов (`GuiElement.*`, загрузка текстур).

---

## 2. ImGui через мод VSImGui (maltiez2)

Отдельный клиентский мод встраивает **Dear ImGui** (**ImGui.NET** + OpenTK, multi-viewport). Репозиторий: [github.com/maltiez2/VintageStory_ImGui](https://github.com/maltiez2/VintageStory_ImGui); страница мода: [mods.vintagestory.at/imgui](https://mods.vintagestory.at/imgui). NuGet: пакеты **`VSImGui`** и **`VSImGui_DebugTools`**.

| Что использовать | Назначение |
|------------------|------------|
| **`ImGuiModSystem`** / интерфейс **`IImGuiRenderer`** | Подписка на **`Draw`** (`DrawCallbackDelegate`) → рисование окон виджетами **`ImGui.*`**; возврат **`CallbackGUIStatus`** (`Closed` / `GrabMouse` / `DontGrabMouse`) для связки с камерой и диалогом VS |
| **`ImGuiDialogBase`**, **`ImGuiDialogWindow`** | Готовые обёртки с подпиской на события и `Open`/`Close` |
| **`VSImGui_DebugTools`**, класс **`DebugWidgets`** | Однострочные отладочные окна (domain/category/id); по замыслу вызывать в **DEBUG** или убирать из релиза |
| Пакеты **ImPlot.NET**, **ImNodes.NET**, **ImGuizmo.NET** (в составе проекта) | Графики, ноды, гизмы — если нужен «продвинутый» UI поверх того же рендера |

Игрок должен **иметь установленный мод `vsimgui`**. Внешний вид не совпадает с нативным HUD Vintage Story; нужно аккуратно возвращать статус колбэка, чтобы не ломать управление.

Подробный разбор кода и ограничений сборки: **[VSIMGUI_RESEARCH.md](./VSIMGUI_RESEARCH.md)**.

---

## 3. Визуализация в мире (3D)

| Вариант | API / материалы | Заметки |
|--------|------------------|---------|
| Частицы | `World.SpawnParticles`, **`SimpleParticleProperties`**, **`AdvancedParticleProperties`**, провайдеры частиц | Эффекты вокруг сущности (например лёгкое «головокружение»); следить за частотой спавна и нагрузкой |
| Модели блоков/предметов/сущностей | JSON-ассеты, шейпы, при необходимости код | Редко нужно только для «индикатора статуса»; чаще HUD |

Для SlowTox-подобной логики **основной надёжный канал** — HUD; частицы — дополнительный колорит.

---

## 4. Зависимость от других модов

| Подход | Описание |
|--------|----------|
| Опциональная DLL / рефлексия | Загрузка сборки при наличии (как черновик `StatusHudLoader` в SlowTox): регистрация виджета только если мод установлен |
| Объявление зависимости в `modinfo.json` | Жёсткая связь: ваш мод не загрузится без указанного мода |

Моды вроде **StatusHud** могут предлагать **слоты под элементы статуса** — это контракт между модами, не встроенный API движка.

---

## 5. Практика для проекта SlowToxVisualized

- **Минимальный полезный набор:** `HudElement` + композер + одна или несколько **`GuiElementStatbar`** для `intoxication` и при желании для внутренних полей (`slowtox:newToxins`, `digestingToxins`, `detoxicants`), если они доступны на клиенте.
- **Синхронизация данных:** убедиться, что нужные **`WatchedAttributes`** реплицируются на клиент; иначе — отдельные пакеты или зеркалирование только для отображения.
- **Частицы** — опционально, не заменяют читаемую шкалу.

---

## 6. Связанные документы в репозитории

- [SLOWTOX_RESEARCH.md](./SLOWTOX_RESEARCH.md) — разбор upstream-мода SlowTox и почему визуализации там почти нет.
- [VSIMGUI_RESEARCH.md](./VSIMGUI_RESEARCH.md) — интеграция ImGui (VSImGui), API и отладочные виджеты.

---

## 7. Официальные и сторонние ссылки (обновлять при смене версии игры)

| Ресурс | URL |
|--------|-----|
| Wiki: GUIs | https://wiki.vintagestory.at/Modding:GUIs |
| Wiki: Simple Particles | https://wiki.vintagestory.at/Modding:Simple_Particles |
| API: `GuiElement` | https://apidocs.vintagestory.at/api/Vintagestory.API.Client.GuiElement.html |
| API: `HudElement` | https://apidocs.vintagestory.at/api/Vintagestory.API.Client.HudElement.html |
| API: `GuiDialog` | https://apidocs.vintagestory.at/api/Vintagestory.API.Client.GuiDialog.html |
| API: `IGuiAPI` | https://apidocs.vintagestory.at/api/Vintagestory.API.Client.IGuiAPI.html |
| API: `GuiElementStatbar` | https://apidocs.vintagestory.at/api/Vintagestory.API.Client.GuiElementStatbar.html |
| API: `ParticleBase` | https://apidocs.vintagestory.at/api/Vintagestory.API.Common.ParticleBase.html |
| VSImGui (GitHub) | https://github.com/maltiez2/VintageStory_ImGui |
| Мод ImGui на сайте модов | https://mods.vintagestory.at/imgui |
| NuGet: VSImGui_DebugTools | https://www.nuget.org/packages/VSImGui_DebugTools |

---

## 8. Журнал дополнений

Используйте для кратких записей о новых находках (дата — по ситуации).

| Дата | Что добавили |
|------|----------------|
| 2026-05-03 | Первый набросок: HUD/диалоги/частицы/интеграции, ссылки на wiki и apidocs. |
| 2026-05-03 | Раздел про VSImGui / Dear ImGui; ссылки на GitHub, мод и NuGet; документ VSIMGUI_RESEARCH.md. |
