## Правила рабочего процесса

### Git

- Перед изменениями создавай отдельную ветку. Префиксы: `feature/`, `bugfix/`, `hotfix/`,
  `chore/`, `docs/`.
- Коммиты — Conventional Commits: `type: description`
  (`feat, fix, refactor, test, docs, style, perf, build, chore, revert`).
  Для веток `bugfix/` и `hotfix/` тип коммита — `fix:`.
- Атомарные коммиты: одно логическое изменение на коммит.
- `main` защищён правилом workflow: любой push в `main` публикует пакет на nuget.org
  (`.github/workflows/main.yml`). Не пушить в `main` напрямую — только через PR.

### Перед коммитом

- `dotnet build src/Calabonga.Wpf.AppDefinitions.sln -c Release` должен проходить без ошибок
  и новых предупреждений.
- Тестового проекта в репозитории нет — `dotnet test` не запускается. Если добавляешь тесты,
  заводи отдельный проект в `src/` и подключай его к solution.
- При создании нового класса проверь, нет ли уже файла/типа с таким именем в решении
  (дедупликация определений идёт по простому имени типа — коллизии имён вредны).

### Изменения публичного API

- `IAppDefinition`, `AppDefinition`, `AppDefinitionExtensions`, `AppDefinitionItem`,
  `AppDefinitionsNotFoundException` — публичный контракт пакета. Менять их сигнатуры только
  по явной задаче.
- Любое breaking-изменение сопровождай:
  1. повышением `<Version>` в `src/Calabonga.Wpf.AppDefinitions/Calabonga.Wpf.AppDefinitions.csproj`
     по SemVer;
  2. записью в разделе «Что нового» в `README.md`;
  3. при необходимости — обновлением `PackageReleaseNotes` в `.csproj`.

### Релиз

- Публикация автоматическая: merge в `main` → workflow собирает `dotnet pack` и делает
  `dotnet nuget push --skip-duplicate`.
- Значит версию в `.csproj` нужно поднять **в той же ветке/PR**, что несёт изменения, иначе
  push с той же версией будет пропущен (`--skip-duplicate`).

### Локальная проверка потребителями

- Downstream-код (WPF-приложения, `Calabonga.Commandex.Engine`) видит только опубликованный
  NuGet. Для локальной проверки — `dotnet pack` + локальный feed или bump версии с
  pre-release-суффиксом.
