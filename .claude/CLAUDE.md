# CLAUDE.md

Указания для Claude Code при работе с репозиторием **Calabonga.Wpf.AppDefinitions**.

## Обзор

NuGet-пакет **Calabonga.Wpf.AppDefinitions** — набор базовых классов для наведения порядка
в WPF-приложении и построения системы модулей/плагинов. Идея та же, что у AppDefinitions для
ASP.NET Core, но без HTTP-конвейера: определение умеет только регистрировать сервисы в
`IServiceCollection` (`ConfigureServices`), стадии `ConfigureApplication`/middleware нет.

Пакет ничего не знает про WPF-типы напрямую (нет `UseWPF`, нет XAML) — это чистая
инфраструктура поверх `Microsoft.Extensions.DependencyInjection`. TFM `net10.0-windows8.0`
задан из-за целевой платформы потребителей, а не из-за зависимостей.

Потребители (WPF-приложения и вышестоящие пакеты, например `Calabonga.Commandex.Engine`)
подключают его **как опубликованный NuGet**. Поэтому публичный API — это контракт: любое
изменение членов `IAppDefinition` ломает всех потребителей (см. удаление `ApplicationOrderIndex`
в v1.0.0-beta.10, README).

## Команды сборки

```bash
# Сборка solution
dotnet build src/Calabonga.Wpf.AppDefinitions.sln -c Release

# Упаковка (GeneratePackageOnBuild=true — .nupkg создаётся и при обычном build)
dotnet pack src/Calabonga.Wpf.AppDefinitions.sln -c Release -o ./nupkg

# Публикация (требуется NUGET_API_KEY)
dotnet nuget push ./nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate
```

Тестового проекта в репозитории нет.

## Структура

Один проект, файлы лежат плоско в `src/Calabonga.Wpf.AppDefinitions/`:

```
src/
├── .editorconfig                       # общий стиль для всех проектов решения
└── Calabonga.Wpf.AppDefinitions/
    ├── IAppDefinition.cs               # контракт определения
    ├── AppDefinition.cs                # abstract-база с реализацией по умолчанию
    ├── AppDefinitionExtensions.cs      # точки входа: AddDefinitions, AddDefinitionsWithModules
    ├── AppDefinitionCollection.cs      # internal sealed — реестр найденных определений
    ├── AppDefinitionItem.cs            # public sealed record — запись реестра
    ├── AppDefinitionsNotFoundException.cs
    └── logo.png                        # иконка пакета (Pack=True)
```

## Ключевые типы

### `IAppDefinition` / `AppDefinition`

| Член | В `AppDefinition` по умолчанию | Назначение |
|---|---|---|
| `void ConfigureServices(IServiceCollection services)` | `abstract` | регистрация сервисов модуля |
| `int ServiceOrderIndex` | `0` | порядок вызова `ConfigureServices` (по возрастанию) |
| `bool Enabled` | `true` | участвует ли определение в конвейере |
| `bool Exported` | `false` | доступно ли определение для загрузки как внешний модуль |

Свои определения наследуют **`AppDefinition`** (не реализуют `IAppDefinition` напрямую — см. фильтр ниже).
`Enabled` / `Exported` имеют `protected set`, то есть переопределяются в наследнике.

### `AppDefinitionExtensions` (публичное API)

- **`AddDefinitions(this IServiceCollection services, params Type[] entryPointsTypes)`**
  Для каждого entry-point-типа берёт его сборку, сканирует `Assembly.ExportedTypes`, создаёт
  экземпляры через `Activator.CreateInstance`, оставляет `Enabled`, сортирует по
  `ServiceOrderIndex`, для каждого вызывает `ConfigureServices(services)`. В конце регистрирует
  `AppDefinitionCollection` как singleton.

- **`AddDefinitionsWithModules(this IServiceCollection services, string modulesFolderPath, params Type[] entryPointsAssembly)`**
  Дополнительно грузит `*.dll` из папки `modulesFolderPath` через `Assembly.LoadFile`, оставляет
  только типы с `Enabled && Exported`, затем делегирует в `AddDefinitions`.
  Кидает `DirectoryNotFoundException`, если папки нет; при пустой папке — просто выходит.

### Фильтр обнаружения (`Predicate`)

```csharp
type is { IsAbstract: false, IsInterface: false } && typeof(AppDefinition).IsAssignableFrom(type)
```

Только неабстрактные классы, наследующие `AppDefinition`. Классы, реализующие голый
`IAppDefinition` без наследования от `AppDefinition`, **не подхватываются**.

### `AppDefinitionCollection` (internal sealed)

Реестр найденных определений и имён entry-point'ов. Дедупликация — `DistinctBy` по
`Definition.GetType().Name` (простое имя типа): два определения с одинаковым именем классов в
разных namespace/сборках схлопнутся в одно.

### `AppDefinitionItem`

`public sealed record AppDefinitionItem(IAppDefinition Definition, string AssemblyName, bool Enabled, bool Exported)`

## Gotchas

- `AddDefinitions` / `AddDefinitionsWithModules` несколько раз вызывают
  `services.BuildServiceProvider()` (для получения `ILogger<IAppDefinition>` и промежуточной
  `AppDefinitionCollection`) — это создаёт временные провайдеры (аналог предупреждения ASP0000).
  Существующее поведение; не тиражировать в новом коде без необходимости.
- Оба метода синхронные, делают синхронный I/O и рефлексию; `async`/`CancellationToken` в API нет.
- В `.csproj` `<Title>` = `Calabonga.AspWpf.AppDefinitions` (лишнее «Asp») — расходится с
  PackageId `Calabonga.Wpf.AppDefinitions`; на упаковку не влияет, но при правке метаданных учитывать.

## Стиль кода

Задаётся `src/.editorconfig` (действует на весь solution):

- `end_of_line = crlf`, `charset = utf-8-bom`, отступ 4 пробела, `insert_final_newline = false`
- file-scoped namespaces; `using` — вне namespace, System-директивы не поднимаются наверх
- `var` предпочтителен; pattern matching и `switch`-выражения предпочтительны
- приватные поля — `_camelCase`; `dotnet_style_readonly_field` и авто-свойства — уровень `error`
- макс. длина строки 200
- в `.csproj`: `ImplicitUsings=enable`, `Nullable=enable`, `Deterministic=true`,
  `IncludeSymbols`/`IncludeSource` + `snupkg`

## Версионирование

- SemVer; текущая версия — **3.0.0** (`<Version>` в `.csproj`), лицензия MIT.
- Changelog ведётся в `README.md` (раздел «Что нового»).
- Изменение сигнатуры `IAppDefinition` — всегда breaking change для потребителей.

## CI/CD

`.github/workflows/main.yml`:

- Триггеры: `push` в `main`, `workflow_dispatch`
- `windows-latest`, `actions/setup-dotnet` с .NET 10 SDK
- шаги: `dotnet restore` → `dotnet pack` (в `$DOTNET_ROOT\Package`) → `dotnet nuget push` на
  nuget.org с `--skip-duplicate` (secret `NUGET_API_KEY`)

## Рабочий процесс

См. `.claude/rules/workflow.md` и `.claude/rules/code-styles.md`.
