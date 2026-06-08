namespace Payroll.Api.Models;

/// <summary>
/// Full result for GET /api/payroll/{month}/{year}: the run header plus a
/// (paginated) list of employee lines.
/// </summary>
public class PayrollRunResult
{
    public int RunId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime RunDateUtc { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalNetPay { get; set; }

    // Pagination metadata (bonus)
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

    public IReadOnlyList<PayrollLine> Items { get; set; } = Array.Empty<PayrollLine>();
}
