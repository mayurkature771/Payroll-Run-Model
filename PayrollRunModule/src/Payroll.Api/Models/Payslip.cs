namespace Payroll.Api.Models;

/// <summary>Single-employee payslip for GET /api/payroll/{runId}/slip/{employeeId}.</summary>
public class Payslip
{
    public int RunId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public int WorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public decimal GrossPay { get; set; }
    public decimal PFDeduction { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal NetPay { get; set; }
}
