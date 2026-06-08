using Payroll.Api.Data;
using Payroll.Api.Repositories;
using Payroll.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection: factory -> repositories -> services (layered architecture)
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// Allow the static frontend (and any local tool) to call the API
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve the frontend from wwwroot (index.html) at the site root
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
