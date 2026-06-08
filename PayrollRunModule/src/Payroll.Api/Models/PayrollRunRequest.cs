using System.ComponentModel.DataAnnotations;

namespace Payroll.Api.Models;

/// <summary>Body for POST /api/payroll/run.</summary>
public class PayrollRunRequest
{
    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    [Range(2000, 2100, ErrorMessage = "Year must be between 2000 and 2100.")]
    public int Year { get; set; }
}
