using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDbExplorer.Model
{
    /// <summary>
    /// Модель данных
    /// </summary>
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public decimal Salary { get; set; }

        public override string ToString()
        {
            return $"ID: {EmployeeID}, Name: {FirstName} {LastName}, Email: {Email}, " +
                   $"Date of Birth: {DateOfBirth:yyyy-MM-dd}, Salary: {Salary:C}";
        }
    }
}
