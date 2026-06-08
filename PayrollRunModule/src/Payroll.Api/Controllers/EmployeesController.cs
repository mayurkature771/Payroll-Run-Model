using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Models;
using Payroll.Api.Services;

namespace Payroll.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeesController(IEmployeeService service) => _service = service;

    /// <summary>GET /api/employees -> list of all employees.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
    {
        var employees = await _service.GetAllAsync();
        return Ok(employees);
    }
}
