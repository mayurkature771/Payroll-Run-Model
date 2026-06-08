namespace Payroll.Api.Models;

public class Employee
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public bool IsActive { get; set; }
}
