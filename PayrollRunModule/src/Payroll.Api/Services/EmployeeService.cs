using Payroll.Api.Models;
using Payroll.Api.Repositories;

namespace Payroll.Api.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllAsync();
}

public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeService(IEmployeeRepository repository) => _repository = repository;

    public Task<IEnumerable<Employee>> GetAllAsync() => _repository.GetAllAsync();
}
