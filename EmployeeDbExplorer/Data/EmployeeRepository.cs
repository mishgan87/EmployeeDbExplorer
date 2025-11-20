using EmployeeDbExplorer.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EmployeeDbExplorer.Data
{
    /// <summary>
    /// Реализация репозитория для MSSQL
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        private SqlConnection? _connection;

        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<SqlConnection> GetOpenConnectionAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            return _connection;
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = @"
                INSERT INTO Employees (FirstName, LastName, Email, DateOfBirth, Salary)
                VALUES (@FirstName, @LastName, @Email, @DateOfBirth, @Salary)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
            command.Parameters.AddWithValue("@Salary", employee.Salary);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = "SELECT * FROM Employees ORDER BY EmployeeID";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var employees = new List<Employee>();
            while (await reader.ReadAsync())
            {
                employees.Add(new Employee
                {
                    EmployeeID = reader.GetInt32("EmployeeID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Salary = reader.GetDecimal("Salary")
                });
            }
            return employees;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = "SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeID", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee
                {
                    EmployeeID = reader.GetInt32("EmployeeID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    Email = reader.GetString("Email"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Salary = reader.GetDecimal("Salary")
                };
            }
            return null;
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = @"
                UPDATE Employees 
                SET FirstName = @FirstName, LastName = @LastName, 
                    Email = @Email, DateOfBirth = @DateOfBirth, Salary = @Salary
                WHERE EmployeeID = @EmployeeID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
            command.Parameters.AddWithValue("@FirstName", employee.FirstName);
            command.Parameters.AddWithValue("@LastName", employee.LastName);
            command.Parameters.AddWithValue("@Email", employee.Email);
            command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);
            command.Parameters.AddWithValue("@Salary", employee.Salary);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeID", id);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<int> GetEmployeesCountWithAboveAverageSalaryAsync()
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = @"
                SELECT COUNT(*) 
                FROM Employees 
                WHERE Salary > (SELECT AVG(Salary) FROM Employees)";

            using var command = new SqlCommand(query, connection);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> EmployeeExistsAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            const string query = "SELECT COUNT(1) FROM Employees WHERE EmployeeID = @EmployeeID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EmployeeID", id);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null)
        {
            using var connection = await GetOpenConnectionAsync();
            string query = "SELECT COUNT(1) FROM Employees WHERE Email = @Email";

            if (excludeEmployeeId.HasValue)
            {
                query += " AND EmployeeID != @EmployeeID";
            }

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            if (excludeEmployeeId.HasValue)
            {
                command.Parameters.AddWithValue("@EmployeeID", excludeEmployeeId.Value);
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}