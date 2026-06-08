using Dapper;
using Payroll.Api.Data;
using Payroll.Api.Models;

namespace Payroll.Api.Repositories;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
}

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly IDbConnectionFactory _factory;

    public EmployeeRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        const string sql = @"
SELECT  e.EmployeeId,
        e.FullName,
        e.Email,
        e.DepartmentId,
        d.Name AS DepartmentName,
        e.BasicSalary,
        e.DateOfJoining,
        e.IsActive
FROM    dbo.Employees   e
JOIN    dbo.Departments d ON d.DepartmentId = e.DepartmentId
ORDER BY e.EmployeeId;";

        using var connection = _factory.Create();
        return await connection.QueryAsync<Employee>(sql);
    }
}
