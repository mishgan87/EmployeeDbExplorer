using System.Xml;

namespace EmployeeDbExplorer.Data
{
    public class SettingsService
    {
        private const string SettingsFile = "settings.xml";
        private readonly DatabaseSettings _settings;

        public SettingsService()
        {
            _settings = new DatabaseSettings();
            LoadSettings();
        }

        public DatabaseSettings GetSettings()
        {
            return _settings;
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFile))
                {
                    CreateDefaultSettings();
                    return;
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(SettingsFile);

                var connectionStringNode = xmlDoc.SelectSingleNode("//Database/ConnectionString");
                if (connectionStringNode != null)
                {
                    _settings.ConnectionString = connectionStringNode.InnerText;
                }
                else
                {
                    throw new InvalidOperationException("Строка подключения отсутствует в файле настроек.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения файла настроек: {ex.Message}");
                CreateDefaultSettings();
            }
        }

        private void CreateDefaultSettings()
        {
            try
            {
                var xmlDoc = new XmlDocument();

                // Создаем декларацию XML
                var declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(declaration);

                // Создаем корневой элемент
                var rootElement = xmlDoc.CreateElement("Settings");
                xmlDoc.AppendChild(rootElement);

                // Создаем элемент Database
                var databaseElement = xmlDoc.CreateElement("Database");
                rootElement.AppendChild(databaseElement);

                // Создаем элемент ConnectionString
                var connectionStringElement = xmlDoc.CreateElement("ConnectionString");
                connectionStringElement.InnerText = "Server=(localdb)\\mssqllocaldb;Database=EmployeeDB;Trusted_Connection=true;TrustServerCertificate=true;";
                databaseElement.AppendChild(connectionStringElement);

                // Сохраняем файл
                xmlDoc.Save(SettingsFile);
                Console.WriteLine("Настройки подключения заданы по умолчанию.");
                Console.WriteLine($"Проверьте строку подключения к БД в {SettingsFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания файла настроек: {ex.Message}");
                throw;
            }
        }

        public void UpdateConnectionString(string connectionString)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(SettingsFile);

                var connectionStringNode = xmlDoc.SelectSingleNode("//Database/ConnectionString");
                if (connectionStringNode != null)
                {
                    connectionStringNode.InnerText = connectionString;
                    xmlDoc.Save(SettingsFile);
                    _settings.ConnectionString = connectionString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания файла настроек: {ex.Message}");
                throw;
            }
        }

        public bool SettingsFileExists()
        {
            return File.Exists(SettingsFile);
        }

        public string GetSettingsFilePath()
        {
            return Path.GetFullPath(SettingsFile);
        }
    }
}