# SmartMealService

Тестовое задание на позицию Middle Fullstack C# Developer.

## Описание структуры Solution

Проект разделен на слои для возможности изолированного тестирования:

1. **SmartMealService.Domain** — библиотека с доменными моделями.
2. **SmartMealService.Services** — библиотека, содержащая логику интеграции с backend-сервером. Реализует два подхода: работу по протоколу **HTTP/JSON** (с Basic-аутентификацией) и по протоколу **gRPC** (без аутентификации).
3. **SmartMealService.Infrastructure** — слой работы с базами данных. Содержит контексты данных для PostgreSQL.
4. **SmartMealService.CliClient** — консольное приложение для получения блюд с сервера, автоматического сохранения/обновления данных в БД и отправки заказов.
5. **SmartMealService.GuiClient** — графическое приложение для чтения, изменения и логирования переменных среды ОС Windows.
6. **SmartMealService.Tests** — модульные и интеграционные тесты, покрывающие репозитории, HTTP и gRPC сервисы.

## Используемые технологии и библиотеки

* **Платформа:** .NET 8.0 SDK (для WPF-клиента требуется Windows)
* **СУБД:** PostgreSQL (основная база меню), SQLite (локальная база GuiClient)
* **ORM:** Entity Framework Core 8.0
* **Протоколы:** gRPC (с автогенерацией клиента через `Grpc.Tools`), HTTP/JSON
* **Логирование:** Serilog (с выводом в консоль и записью в файл)
* **UI-пакеты:** CommunityToolkit.Mvvm (Source Generators)
* **Тестирование:** xUnit, FluentAssertions, NSubstitute, EF Core InMemory 

---

## Сборка

Перед началом убедитесь, что у вас установлен **.NET 8 SDK**.

### 1. Клонирование репозитория

```bash
git clone [https://github.com/Matricarin/smart-meal-service.git](https://github.com/Matricarin/smart-meal-service.git)
cd smart-meal-service
```

### 2. Восстановление и сборка

```
dotnet restore
dotnet build --configuration Release
```

### 3. Настройка приложения

Перед запуском приложений указывайте корректные настройки подключения к базе данных и данные аутентификации в appsettings.json
Для графического клиента необходимо указать список переменных среды, которые будут созданы при инициализации приложения.

### 4. Запуск тестов

```
dotnet test --configuration Release
```

### 5. Запуск приложений

```
dotnet run --project SmartMealService.CliClient
dotnet run --project SmartMealService.GuiClient
```
