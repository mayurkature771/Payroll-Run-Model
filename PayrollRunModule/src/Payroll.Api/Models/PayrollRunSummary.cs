namespace Payroll.Api.Models;

/// <summary>Header row returned by usp_RunPayroll and the GET endpoints.</summary>
public class PayrollRunSummary
{
    public int RunId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime RunDateUtc { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetPay { get; set; }
    public bool IsFinalised { get; set; }
}
