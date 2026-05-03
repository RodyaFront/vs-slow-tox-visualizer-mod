# Окружение разработки (локально)

Машинозависимые пути. При переносе на другой ПК обновите значения.

**Версия мода:** номер в [`SlowToxVisualized/modinfo.json`](../SlowToxVisualized/modinfo.json); список изменений по релизам — [`CHANGELOG.md`](CHANGELOG.md).

## Установка Vintage Story

| Параметр | Значение |
|----------|----------|
| Каталог игры | `D:\Games\Vintagestory` |
| Папка модов для теста в игре | `C:\Users\rusya\AppData\Roaming\VintagestoryData\Mods` |
| Логи клиента (по умолчанию) | `%AppData%\VintagestoryData\Logs` → у этого пользователя путь проверен: `C:\Users\rusya\AppData\Roaming\VintagestoryData\Logs` (папка есть; типичные файлы: `client-main.log`, `client-debug.log`, `client-crash.log`) |

По умолчанию Vintage Story пишет логи в **`(data path)/Logs/`**, не в каталог установки игры. Если клиент запускается с **`--dataPath`** или **`--logPath`**, смотрите [параметры клиента](https://wiki.vintagestory.at/index.php/Client_startup_parameters) — логи будут относительно указанного пути.

Сюда копируйте результат сборки нашего мода (каталог мода с `modinfo.json`, DLL и ассеты). Если клиент запускается с другой data-папкой (`VintagestoryDataDev` и т.п.), используйте её подпапку `Mods`, чтобы тест совпадал с профилем запуска.

## Пересборка и копирование в Mods (канонический флоу)

**Проблема:** `dotnet build` кладёт артефакт в **`SlowToxVisualized/bin/Release/Mods/slowtoxvisualized/`**, а клиент читает моды из **`(data path)/Mods/`** — это **разные каталогы**. Пока результат сборки не скопирован в игровые `Mods`, в мире останется **старая** DLL и **старые** `assets` (в том числе `lang/*.json`).

**Ошибка копирования (уже бывала):** в PowerShell команда вида «скопировать папку `slowtoxvisualized` в уже существующую `...\Mods\slowtoxvisualized`» без `\*` создаёт **вложенную** папку `Mods\slowtoxvisualized\slowtoxvisualized\` с новой сборкой, а игра продолжает брать **`modinfo.json` и `assets` из корня** старой установки — кажется, что «ничего не обновилось».

**Правильно:** копировать **содержимое** выходной папки в **целевой корень** мода (поверх старых файлов):

```powershell
$src = "…\SlowToxVisualized\bin\Release\Mods\slowtoxvisualized\*"
$dst = "…\VintagestoryData\Mods\slowtoxvisualized"
Remove-Item -Path "$dst\*" -Recurse -Force -ErrorAction SilentlyContinue   # опционально: полная замена
Copy-Item -Path $src -Destination $dst -Recurse -Force
```

Либо один раз из корня репозитория запускать **[`scripts/deploy-slowtoxvisualized.ps1`](../scripts/deploy-slowtoxvisualized.ps1)** — он делает `dotnet build -c Release` и то же **плоское** копирование. Цель: если задана **`VS_MODS_TEST`**, используется `$env:VS_MODS_TEST\slowtoxvisualized`, иначе `%AppData%\Roaming\VintagestoryData\Mods\slowtoxvisualized`.

После копирования имеет смысл проверить в списке модов клиента поле **версии** из `modinfo.json` и при смене языковых файлов — **полный перезапуск** клиента.

Сборки модов в этом репозитории обычно ссылаются на DLL через переменную **`VINTAGE_STORY`** (см. `SlowTox-upstream/SlowTox/SlowTox.csproj`: `$(VINTAGE_STORY)/VintagestoryAPI.dll` и т.д.). Профили запуска в `Properties/launchSettings.json` ожидают тот же каталог как рабочую директорию и путь к `Vintagestory` / `VintagestoryServer`.

### Настройка `VINTAGE_STORY` в Windows

**Временно (сессия PowerShell):**

```powershell
$env:VINTAGE_STORY = "D:\Games\Vintagestory"
```

**Постоянно для пользователя:** «Параметры системы» → «Переменные среды» → пользовательская переменная `VINTAGE_STORY` = `D:\Games\Vintagestory`, либо:

```powershell
[System.Environment]::SetEnvironmentVariable("VINTAGE_STORY", "D:\Games\Vintagestory", "User")
```

После изменения пользовательских переменных перезапустите терминал и IDE.

### Cursor / VS Code

В `.vscode/settings.json` для интегрированного терминала заданы **`VINTAGE_STORY`** (сборка против DLL игры) и **`VS_MODS_TEST`** — тот же путь, что и «Папка модов для теста» в таблице выше; удобно для скриптов копирования после сборки, например `Copy-Item -Recurse … $env:VS_MODS_TEST`.

## Проверка в игре после изменений в моде

ИИ не запускает клиент за вас: после изменений в коде мода, `modinfo.json`, ассетах или в способе сборки/копирования имеет смысл **вручную** пройти короткий цикл:

1. `dotnet build` проекта в [`SlowToxVisualized/SlowToxVisualized.csproj`](../SlowToxVisualized/SlowToxVisualized.csproj) с заданным `VINTAGE_STORY`, **или** запуск [`scripts/deploy-slowtoxvisualized.ps1`](../scripts/deploy-slowtoxvisualized.ps1) из корня репозитория.
2. Если сборка вручную: скопировать **содержимое** `bin/Release/Mods/slowtoxvisualized/*` в каталог модов из таблицы выше (см. раздел «Пересборка и копирование» — обязательно **плоское** копирование, не вложенная папка).
3. Запустить клиент Vintage Story с той же data-папкой, куда копировали моды.
4. Убедиться, что мод в списке активен, при необходимости проверить клиентский лог на ошибки загрузки и ожидаемые сообщения.

### Версия мода в игре

Номер версии берётся из поля **`version`** в `modinfo.json` мода (например `0.1.0`). После пересборки и копирования в `Mods` в списке модов клиента отображается актуальный номер — так можно убедиться, что подхвачена новая сборка. При изменениях в коде версию в `modinfo.json` имеет смысл увеличивать (см. правило в `.cursor/rules/project-context.mdc`).

### Где смотреть логи

| Файл | Когда открывать |
|------|-----------------|
| `client-main.log` | Основной клиентский лог сессии |
| `client-debug.log` | Подробности, если включён подробный/отладочный вывод |
| `client-crash.log` | Падения и критические ошибки |

Каталог — см. строку «Логи клиента» в таблице выше. Агент Cursor при проверке логов должен читать файлы **там**, а не искать логи внутри `VINTAGE_STORY` (каталог игры), если не оговорено иное.

Для агента Cursor это же ожидание зафиксировано в **`.cursor/rules/project-context.mdc`** и **`vintage-story-csharp.mdc`**.

---

*Обновляйте таблицу при смене диска или версии установки.*
