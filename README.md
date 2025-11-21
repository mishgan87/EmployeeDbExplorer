Консольное приложение для управления информацией о сотрудниках.
Разработано на C# с использованием MS SQL Server.
Приложение предоставляет полный набор CRUD операций для работы с данными сотрудников.

Технологический стек:
  - Язык: C# (.NET 8.0)
  - База данных: MS SQL Server (LocalDB/Express)
  - Архитектура: Repository Pattern
  - Безопасность: Parameterized Queries (защита от SQL-инъекций)

Файлы приложения
  - EmployeeDbExplorer.exe - исполняемый файл
  - settings.xml - файл конфигурации с строкой подключения
  - publish.7z - пример файла настроек и приложение

Команда для сборки
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=false -o ./publish

В будущем дополнится тестам xUnit + Moq
