namespace Payroll.Api.Models;

/// <summary>One employee's calculated row within a payroll run.</summary>
public class PayrollLine
{
    public int EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public int WorkingDays { get; set; }
    public int DaysPresent { get; set; }
    public decimal GrossPay { get; set; }
    public decimal PFDeduction { get; set; }
    public decimal ProfessionalTax { get; set; }
    public decimal NetPay { get; set; }
}
