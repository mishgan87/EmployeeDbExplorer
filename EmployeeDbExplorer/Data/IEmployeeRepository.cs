using EmployeeDbExplorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDbExplorer.Data
{
    /// <summary>
    /// Интерфейс репозитория
    /// </summary>
    public interface IEmployeeRepository : IDisposable
    {
        Task AddEmployeeAsync(Employee employee);
        Task<List<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);
        Task<int> GetEmployeesCountWithAboveAverageSalaryAsync();
        Task<bool> EmployeeExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email, int? excludeEmployeeId = null);
    }
}
