using EmployeeDbExplorer.Data;
using EmployeeDbExplorer.Model;

namespace EmployeeDbExplorer
{
    class Program
    {
        private static IEmployeeRepository? _repository;
        private static SettingsService? _settingsService;

        static async Task Main(string[] args)
        {
            try
            {
                _settingsService = new SettingsService();

                if (!_settingsService.SettingsFileExists())
                {
                    Console.WriteLine("Настройки подключения заданы по умолчанию.");
                    Console.WriteLine("Проверьте строку подключения.");
                }

                await InitializeDatabaseAsync();
                await RunApplicationAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                Console.Write("Нажмите любую клавишу...");
                Console.ReadKey();
            }
        }
        private static async Task InitializeDatabaseAsync()
        {
            try
            {
                var settings = _settingsService!.GetSettings();
                _repository = new EmployeeRepository(settings.ConnectionString);

                // Проверка соединения путём получения количества сотрудников
                var employees = await _repository.GetAllEmployeesAsync();
                Console.WriteLine("Подключение к базе данных успешно установлено.");
                Console.WriteLine($"Найдено сотрудников: {employees.Count}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                Console.WriteLine($"{Environment.NewLine}Проверьте:");
                Console.WriteLine("1. Запущен ли SQL Server");
                Console.WriteLine("2. Есть ли в базе таблица 'EmployeeDB'");
                Console.WriteLine("3. Валидность строки подключения в файле settings.xml");

                await OfferConnectionStringUpdate();
                throw;
            }
        }

        private static async Task OfferConnectionStringUpdate()
        {
            Console.Write("\nОбновить строку подключения? (y/n): ");
            var response = Console.ReadLine()?.ToLower();

            if (response == "y" || response == "yes")
            {
                await UpdateConnectionStringInteractive();
            }
        }

        private static async Task UpdateConnectionStringInteractive()
        {
            try
            {
                Console.WriteLine($"{Environment.NewLine}Примеры строки подключения:");
                Console.WriteLine("1. Для LocalDB: Server=(localdb)\\\\mssqllocaldb;Database=EmployeeDB;Trusted_Connection=true;TrustServerCertificate=true;");
                Console.WriteLine("2. Для SQL Express: Server=.\\\\SQLEXPRESS;Database=EmployeeDB;Trusted_Connection=true;TrustServerCertificate=true;");
                Console.WriteLine("3. Для SQL Server with credentials: Server=localhost;Database=EmployeeDB;User Id=username;Password=password;TrustServerCertificate=true;");

                Console.Write($"{Environment.NewLine}Введите строку подключения: ");
                var newConnectionString = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(newConnectionString))
                {
                    _settingsService!.UpdateConnectionString(newConnectionString);
                    Console.WriteLine("Строка подключения обновлена!");
                    Console.WriteLine("Повторно запустите приложение для её использования");
                }
                else
                {
                    Console.WriteLine("Строка подключения не может быть пустой.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления строки подключения: {ex.Message}");
            }
        }

        private static async Task RunApplicationAsync()
        {
            while (true)
            {
                Console.Clear();
                DisplayMenu();

                var choice = GetUserChoice();
                await ProcessUserChoice(choice);

                if (choice == 7) break;

                Console.Write($"{Environment.NewLine}Нажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        private static void DisplayMenu()
        {
            Console.WriteLine($"Управление информацией о сотрудниках{Environment.NewLine}");
            Console.WriteLine("1. Добавить нового сотрудника");
            Console.WriteLine("2. Посмотреть всех сотрудников");
            Console.WriteLine("3. Обновить информацию о сотруднике");
            Console.WriteLine("4. Удалить сотрудника");
            Console.WriteLine("5. Сотрудники с зарплатой выше средней по всем сотрудникам");
            Console.WriteLine("6. Задать строку подключения");
            Console.WriteLine("7. Выйти из приложения");
            Console.Write($"{Environment.NewLine}Выберите опцию (1-7): ");
        }

        private static int GetUserChoice()
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 7)
                {
                    return choice;
                }
                Console.Write("Номер опции должен быть в интервале от 1 до 7: ");
            }
        }

        private static async Task ProcessUserChoice(int choice)
        {
            try
            {
                switch (choice)
                {
                    case 1:
                        await AddNewEmployeeAsync();
                        break;
                    case 2:
                        await ViewAllEmployeesAsync();
                        break;
                    case 3:
                        await UpdateEmployeeAsync();
                        break;
                    case 4:
                        await DeleteEmployeeAsync();
                        break;
                    case 5:
                        await ShowEmployeesWithAboveAverageSalaryAsync();
                        break;
                    case 6:
                        await UpdateConnectionStringInteractive();
                        break;
                    case 7:
                        _repository?.Dispose();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static async Task AddNewEmployeeAsync()
        {
            Console.WriteLine($"{Environment.NewLine}Новый сотрудник{Environment.NewLine}");

            var employee = new Employee();

            employee.FirstName = GetValidatedInput("First Name: ", false);
            employee.LastName = GetValidatedInput("Last Name: ", false);
            employee.Email = await GetValidatedEmailAsync();
            employee.DateOfBirth = GetValidatedDateOfBirth();
            employee.Salary = GetValidatedSalary();

            await _repository!.AddEmployeeAsync(employee);
            Console.WriteLine($"{Environment.NewLine}Добавлено!");
        }

        private static async Task ViewAllEmployeesAsync()
        {
            var employees = await _repository!.GetAllEmployeesAsync();

            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.WriteLine($"{Environment.NewLine}Сотрудники{Environment.NewLine}");

            foreach (var employee in employees)
            {
                Console.WriteLine(employee);
            }
            Console.WriteLine($"{Environment.NewLine}Всего сотрудников: {employees.Count}");
        }

        private static async Task UpdateEmployeeAsync()
        {
            Console.WriteLine($"{Environment.NewLine}Обновить данные{Environment.NewLine}");

            var employees = await _repository!.GetAllEmployeesAsync();
            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            await ViewAllEmployeesAsync();

            Console.Write("\nВведите ID сотрудника: ");
            if (!int.TryParse(Console.ReadLine(), out int employeeId))
            {
                Console.WriteLine("Employee ID не существует.");
                return;
            }

            var employee = await _repository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.WriteLine($"{Environment.NewLine}Текущие данные:{Environment.NewLine}{employee}");
            Console.WriteLine($"{Environment.NewLine}Редактируемые поля:");
            Console.WriteLine("1. First Name");
            Console.WriteLine("2. Last Name");
            Console.WriteLine("3. Email");
            Console.WriteLine("4. Date of Birth");
            Console.WriteLine("5. Salary");
            Console.WriteLine("6. Обновить все");
            Console.Write("Выберите поле (1-6): ");

            if (!int.TryParse(Console.ReadLine(), out int fieldChoice) || fieldChoice < 1 || fieldChoice > 6)
            {
                Console.WriteLine("Неверный индекс.");
                return;
            }

            switch (fieldChoice)
            {
                case 1:
                    employee.FirstName = GetValidatedInput("First Name: ", false);
                    break;
                case 2:
                    employee.LastName = GetValidatedInput("Last Name: ", false);
                    break;
                case 3:
                    employee.Email = await GetValidatedEmailAsync(employee.EmployeeID);
                    break;
                case 4:
                    employee.DateOfBirth = GetValidatedDateOfBirth();
                    break;
                case 5:
                    employee.Salary = GetValidatedSalary();
                    break;
                case 6:
                    employee.FirstName = GetValidatedInput("First Name: ", false);
                    employee.LastName = GetValidatedInput("Last Name: ", false);
                    employee.Email = await GetValidatedEmailAsync(employee.EmployeeID);
                    employee.DateOfBirth = GetValidatedDateOfBirth();
                    employee.Salary = GetValidatedSalary();
                    break;
            }

            await _repository.UpdateEmployeeAsync(employee);
            Console.WriteLine("Данные обновлены!");
        }

        private static async Task DeleteEmployeeAsync()
        {
            Console.WriteLine($"{Environment.NewLine}Удалить сотрудника");

            var employees = await _repository!.GetAllEmployeesAsync();
            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            await ViewAllEmployeesAsync();

            Console.Write("Введите ID сотрудника: ");
            if (!int.TryParse(Console.ReadLine(), out int employeeId))
            {
                Console.WriteLine("ID не существует");
                return;
            }

            var employee = await _repository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            Console.WriteLine($"{Environment.NewLine}Подтвердите удаление сотрудника:{Environment.NewLine}{employee}");
            Console.Write($"{Environment.NewLine}Вы уверены? (y/n): ");
            var confirmation = Console.ReadLine()?.ToLower();

            if (confirmation == "y" || confirmation == "yes")
            {
                await _repository.DeleteEmployeeAsync(employeeId);
                Console.WriteLine("Данные удалены!");
            }
            else
            {
                Console.WriteLine("Отмена удаления.");
            }
        }

        private static async Task ShowEmployeesWithAboveAverageSalaryAsync()
        {
            var count = await _repository!.GetEmployeesCountWithAboveAverageSalaryAsync();
            var employees = await _repository.GetAllEmployeesAsync();

            if (employees.Count == 0)
            {
                Console.WriteLine("Сотрудники не найдены.");
                return;
            }

            var averageSalary = employees.Average(e => e.Salary);

            Console.WriteLine($"{Environment.NewLine}Средняя зарплата: {averageSalary}");
            Console.WriteLine($"Сотрудники с зарплатой выше средней ({count}):{Environment.NewLine}");

            employees.Where(e => e.Salary > averageSalary)
                .ToList()
                .ForEach(emp => Console.WriteLine($"- {emp.FirstName} {emp.LastName}: {emp.Salary}"));
        }

        private static string GetValidatedInput(string prompt, bool allowEmpty)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim() ?? string.Empty;

                if (allowEmpty || !string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                Console.WriteLine("Это поле обязательно для заполнения.");
            }
        }

        private static async Task<string> GetValidatedEmailAsync(int? excludeEmployeeId = null)
        {
            while (true)
            {
                Console.Write("Email: ");
                var email = Console.ReadLine()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("Email не задан.");
                    continue;
                }

                if (!IsValidEmail(email))
                {
                    Console.WriteLine("Введите валидный email.");
                    continue;
                }

                if (await _repository!.EmailExistsAsync(email, excludeEmployeeId))
                {
                    Console.WriteLine("Email уже существует.");
                    continue;
                }

                return email;
            }
        }

        private static DateTime GetValidatedDateOfBirth()
        {
            while (true)
            {
                Console.Write("Date of Birth (yyyy-mm-dd): ");
                if (DateTime.TryParse(Console.ReadLine(), out DateTime dateOfBirth))
                {
                    if (dateOfBirth <= DateTime.Now && dateOfBirth > DateTime.Now.AddYears(-100))
                    {
                        return dateOfBirth;
                    }
                    Console.WriteLine("Дата рождения должна быть реалистичной: не в будущем и не старше 100 лет.");
                }
                else
                {
                    Console.WriteLine("Введите дату в формате yyyy-mm-dd.");
                }
            }
        }

        private static decimal GetValidatedSalary()
        {
            while (true)
            {
                Console.Write("Salary: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal salary) && salary >= 0)
                {
                    return salary;
                }
                Console.WriteLine("Зарплата не может быть отрицательной.");
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}