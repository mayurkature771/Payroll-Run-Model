using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Common;
using Payroll.Api.Models;
using Payroll.Api.Services;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/payroll")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _service;

    public PayrollController(IPayrollService service) => _service = service;

    /// <summary>POST /api/payroll/run -> trigger a run. 201 Created, or 409 if it already exists.</summary>
    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] PayrollRunRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var summary = await _service.RunAsync(request.Month, request.Year);
            return CreatedAtAction(
                nameof(GetByPeriod),
                new { month = request.Month, year = request.Year },
                summary);
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>GET /api/payroll/{month}/{year} -> saved run (paginated). 200 or 404.</summary>
    [HttpGet("{month:int}/{year:int}")]
    public async Task<IActionResult> GetByPeriod(
        int month, int year,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _service.GetByPeriodAsync(month, year, page, pageSize);
        if (result is null)
            return NotFound(new { message = $"No payroll run found for {month:00}/{year}." });

        return Ok(result);
    }

    /// <summary>GET /api/payroll/{runId}/slip/{employeeId} -> one payslip. 200 or 404.</summary>
    [HttpGet("{runId:int}/slip/{employeeId:int}")]
    public async Task<IActionResult> GetSlip(int runId, int employeeId)
    {
        var slip = await _service.GetPayslipAsync(runId, employeeId);
        if (slip is null)
            return NotFound(new { message = "Payslip not found for the given run and employee." });

        return Ok(slip);
    }
}
