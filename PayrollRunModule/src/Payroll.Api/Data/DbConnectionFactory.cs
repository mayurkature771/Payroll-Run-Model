using System.Data;
using Microsoft.Data.SqlClient;

namespace Payroll.Api.Data;

/// <summary>Creates a fresh ADO.NET connection per call (Dapper opens/closes it).</summary>
public interface IDbConnectionFactory
{
    IDbConnection Create();
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PayrollDb")
            ?? throw new InvalidOperationException(
                "Connection string 'PayrollDb' is not configured in appsettings.json.");
    }

    public IDbConnection Create() => new SqlConnection(_connectionString);
}
