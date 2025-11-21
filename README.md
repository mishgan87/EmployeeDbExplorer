Консольное приложение для управления информацией о сотрудниках.
Разработано на C# с использованием MS SQL Server.
Приложение предоставляет полный набор CRUD операций для работы с данными сотрудников.

Файлы приложения
  - EmployeeDbExplorer.exe - исполняемый файл
  - settings.xml - файл конфигурации с строкой подключения
  - EmployeeDB.mdf - файл базы данных (создается автоматически)

Команда для сборки
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=false -o ./publish
