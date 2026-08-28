## Стиль кода C#

Базовый источник правил — `src/.editorconfig` (действует на весь solution). Ниже — то, что
важно держать в голове при правках именно этого пакета.

### Синтаксис

- File-scoped namespaces.
- `using`-директивы — вне `namespace`; System-директивы отдельно не поднимаются
  (`dotnet_sort_system_directives_first = false`).
- `ImplicitUsings=enable` — не добавляй `using` для того, что уже входит в неявный набор.
  Отдельного `GlobalUsings.cs` в проекте нет; специфичные `using` пиши в самом файле.
- `var`, если тип очевиден из правой части.
- Pattern matching и `switch`-выражения предпочтительнее приведений и цепочек `if/else`
  (см. `Predicate` в `AppDefinitionExtensions.cs`).
- `nameof(...)` вместо строковых литералов для имён членов.
- Expression-bodied члены допустимы (в коде уже используются).

### Nullable

- `<Nullable>enable</Nullable>`. Не глушить предупреждения через `!` без комментария с причиной.

### Модификаторы и структура класса

- Явно указывай модификатор доступа у всех членов.
- `sealed` по умолчанию для классов, не рассчитанных на наследование
  (`AppDefinitionCollection`, `AppDefinitionItem` — `sealed`).
  Исключения намеренно открыты: `AppDefinition` — база для пользовательских определений,
  `AppDefinitionsNotFoundException` — база для более конкретных исключений.
- Приватные поля — `_camelCase` (`dotnet_naming` в `.editorconfig`).
- `readonly` для полей и авто-свойства — уровень `error`, не понижать.
- Порядок членов: приватные `readonly` поля → конструкторы → публичные члены →
  приватные вспомогательные методы → `override`. Для статических классов-расширений
  (`AppDefinitionExtensions`) — публичные методы, приватные хелперы рядом с местом вызова.

### Неизменяемость

- `record` / `readonly record struct` для DTO-подобных носителей данных
  (`AppDefinitionItem` — `sealed record`).

### Обработка ошибок

- Fail fast. На этапе конфигурации приложения допустимо и ожидаемо бросать исключения:
  `AppDefinitionsNotFoundException`, `DirectoryNotFoundException`,
  `ArgumentException.ThrowIfNullOrEmpty(...)`.
- Пакета `Calabonga.Results` здесь нет и добавлять его в этот лёгкий пакет
  (2 зависимости: `Microsoft.Extensions.DependencyInjection`,
  `Microsoft.Extensions.Logging.Abstractions`) не нужно.
- `try-catch` — только для реально исключительных ситуаций, не для управления потоком.
  Существующий паттерн: обернуть операцию, залогировать через `ILogger` и пробросить (`throw;`).

### Логирование

- `ILogger<IAppDefinition>` из `Microsoft.Extensions.Logging.Abstractions`.
- Structured logging с именованными плейсхолдерами (`{ModuleName}`, `{@items}`).
- Перед дорогими сообщениями проверяй уровень: `logger.IsEnabled(LogLevel.Debug)`.

### Чего в этом пакете нет (не тащить в правки без явной задачи)

- Нет `async`/`await` — публичное API синхронное; `CancellationToken` в сигнатурах отсутствует.
- Нет EF Core / БД / `DbContext`.
- Нет WPF-типов, XAML, Blazor, `TimeProvider`/работы с датами.
- Нет тестового проекта.

### API как контракт

Пакет потребляется как опубликованный NuGet (в т.ч. `Calabonga.Commandex.Engine`).
Изменение членов `IAppDefinition` или публичных сигнатур `AppDefinitionExtensions` —
breaking change: сопровождай сменой версии и записью в `README.md`.
