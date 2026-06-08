using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Payroll.Api.Common;
using Payroll.Api.Data;
using Payroll.Api.Models;

namespace Payroll.Api.Repositories;

public interface IPayrollRepository
{
    Task<PayrollRunSummary> RunAsync(int month, int year);
    Task<(PayrollRunSummary? Run, IReadOnlyList<PayrollLine> Lines)> GetByPeriodAsync(int month, int year);
    Task<Payslip?> GetPayslipAsync(int runId, int employeeId);
}

public sealed class PayrollRepository : IPayrollRepository
{
    // Custom error numbers raised by usp_RunPayroll
    private const int ErrConflict = 50409;
    private const int ErrNotFound = 50404;
    private const int ErrBadInput = 50400;

    private readonly IDbConnectionFactory _factory;

    public PayrollRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<PayrollRunSummary> RunAsync(int month, int year)
    {
        using var connection = _factory.Create();
        try
        {
            return await connection.QuerySingleAsync<PayrollRunSummary>(
                "dbo.usp_RunPayroll",
                new { Month = month, Year = year },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60);
        }
        catch (SqlException ex) when (ex.Number == ErrConflict)
        {
            throw new ConflictException(ex.Message);
        }
        catch (SqlException ex) when (ex.Number == ErrNotFound || ex.Number == ErrBadInput)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    public async Task<(PayrollRunSummary?, IReadOnlyList<PayrollLine>)> GetByPeriodAsync(int month, int year)
    {
        const string sql = @"
SELECT  r.RunId, r.[Month], r.[Year], r.RunDateUtc,
        r.EmployeeCount, r.TotalNetPay, r.IsFinalised
FROM    dbo.PayrollRuns r
WHERE   r.[Month] = @month AND r.[Year] = @year;

SELECT  d.EmployeeId,
        e.FullName AS Name,
        d.BasicSalary,
        d.WorkingDays,
        d.DaysPresent,
        d.GrossPay,
        d.PFDeduction,
        d.ProfessionalTax,
        d.NetPay
FROM    dbo.PayrollDetails d
JOIN    dbo.PayrollRuns    r ON r.RunId      = d.RunId
JOIN    dbo.Employees      e ON e.EmployeeId = d.EmployeeId
WHERE   r.[Month] = @month AND r.[Year] = @year
ORDER BY e.FullName;";

        using var connection = _factory.Create();
        using var multi = await connection.QueryMultipleAsync(sql, new { month, year });

        var run = await multi.ReadSingleOrDefaultAsync<PayrollRunSummary>();
        var lines = (await multi.ReadAsync<PayrollLine>()).AsList();
        return (run, lines);
    }

    public async Task<Payslip?> GetPayslipAsync(int runId, int employeeId)
    {
        const string sql = @"
SELECT  r.RunId, r.[Month], r.[Year],
        d.EmployeeId,
        e.FullName AS Name,
        dep.Name   AS DepartmentName,
        d.BasicSalary, d.WorkingDays, d.DaysPresent,
        d.GrossPay, d.PFDeduction, d.ProfessionalTax, d.NetPay
FROM    dbo.PayrollDetails d
JOIN    dbo.PayrollRuns    r   ON r.RunId        = d.RunId
JOIN    dbo.Employees      e   ON e.EmployeeId   = d.EmployeeId
JOIN    dbo.Departments    dep ON dep.DepartmentId = e.DepartmentId
WHERE   d.RunId = @runId AND d.EmployeeId = @employeeId;";

        using var connection = _factory.Create();
        return await connection.QuerySingleOrDefaultAsync<Payslip>(sql, new { runId, employeeId });
    }
}
