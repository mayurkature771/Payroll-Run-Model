using Payroll.Api.Models;
using Payroll.Api.Repositories;

namespace Payroll.Api.Services;

public interface IPayrollService
{
    Task<PayrollRunSummary> RunAsync(int month, int year);
    Task<PayrollRunResult?> GetByPeriodAsync(int month, int year, int page, int pageSize);
    Task<Payslip?> GetPayslipAsync(int runId, int employeeId);
}

public sealed class PayrollService : IPayrollService
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    private readonly IPayrollRepository _repository;

    public PayrollService(IPayrollRepository repository) => _repository = repository;

    public Task<PayrollRunSummary> RunAsync(int month, int year)
        => _repository.RunAsync(month, year);

    public async Task<PayrollRunResult?> GetByPeriodAsync(int month, int year, int page, int pageSize)
    {
        var (run, lines) = await _repository.GetByPeriodAsync(month, year);
        if (run is null) return null;

        // Normalise pagination inputs (bonus feature)
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = DefaultPageSize;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var pagedItems = lines
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PayrollRunResult
        {
            RunId = run.RunId,
            Month = run.Month,
            Year = run.Year,
            RunDateUtc = run.RunDateUtc,
            EmployeeCount = run.EmployeeCount,
            TotalNetPay = run.TotalNetPay,
            Page = page,
            PageSize = pageSize,
            TotalItems = lines.Count,
            Items = pagedItems
        };
    }

    public Task<Payslip?> GetPayslipAsync(int runId, int employeeId)
        => _repository.GetPayslipAsync(runId, employeeId);
}
