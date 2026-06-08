# Employee Payroll Run Module

## Overview

The Employee Payroll Run Module is a payroll processing application developed using **ASP.NET Core 8 Web API**, **Dapper**, and **SQL Server**. The system allows HR users to generate monthly payroll, calculate employee salaries based on attendance, view payroll summaries, and generate printable payslips.

The application follows a layered architecture consisting of:

* Presentation Layer (HTML, CSS, JavaScript)
* API Controllers
* Service Layer
* Repository Layer
* SQL Server Database with Stored Procedures

---

## Features

### Employee Management

* View employee information
* Department-wise employee records
* Attendance-based payroll processing

### Payroll Processing

* Run payroll for a selected month and year
* Calculate Gross Salary
* Calculate PF Deduction (12% of Basic Salary)
* Apply Professional Tax (₹200)
* Generate Net Salary
* Store payroll run history

### Payslip Generation

* View employee payslip
* Print payroll details
* Monthly payroll summaries

### Validation & Error Handling

* Input validation
* Business rule validation
* Global exception handling
* Proper HTTP status codes


## Technology Stack

### Backend

* ASP.NET Core 8 Web API
* C#
* Dapper ORM
* SQL Server

### Frontend

* HTML5
* CSS3
* JavaScript (Vanilla JS)

### Testing

* xUnit

### Documentation

* Swagger/OpenAPI



## Project Structure

```text
PayrollRunModule
│
├── database
│   ├── 01_schema.sql
│   ├── 02_seed.sql
│   └── 03_usp_RunPayroll.sql
│
├── src
│   └── Payroll.Api
│       ├── Controllers
│       ├── Services
│       ├── Repositories
│       ├── Models
│       ├── Data
│       ├── Common
│       └── wwwroot
│
├── tests
│   └── Payroll.Tests
│
└── PayrollRunModule.sln
```



## Payroll Calculation Logic

### Gross Salary

```text
Gross Salary =
(Basic Salary / Total Working Days) × Days Present
```

### PF Deduction

```text
PF = 12% of Basic Salary
```

### Professional Tax

```text
₹200 per month
```

### Net Salary

```text
Net Salary = Gross Salary - PF - Professional Tax
```



## Database Setup

### Step 1: Create Database

Execute the SQL scripts in the following order:

```sql
database/01_schema.sql
database/02_seed.sql
database/03_usp_RunPayroll.sql
```

These scripts will:

* Create PayrollDb database
* Create required tables
* Insert sample data
* Create payroll stored procedures



## Configure Connection String

Update the connection string in:

```text
src/Payroll.Api/appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "PayrollDb": "Server=(localdb)\\MSSQLLocalDB;Database=PayrollDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```



## Running the Application

### Restore Packages

```bash
dotnet restore
```

### Build Project

```bash
dotnet build
```

### Run Application

```bash
cd src/Payroll.Api
dotnet run
```

Application URL:

```text
http://localhost:5080
```

Swagger URL:

```text
http://localhost:5080/swagger
```



## Running Unit Tests

Navigate to:

```bash
cd tests/Payroll.Tests
```

Run:

```bash
dotnet test
```



## API Endpoints

### Employees

| Method | Endpoint       |
| ------ | -------------- |
| GET    | /api/employees |

### Payroll

| Method | Endpoint                                         |
| ------ | ------------------------------------------------ |
| POST   | /api/payroll/run                                 |
| GET    | /api/payroll/{month}/{year}                      |
| GET    | /api/payroll/payslip/{employeeId}/{month}/{year} |



## Sample Seed Data

The project includes:

* 2 Departments
* 5 Employees
* Monthly Attendance Records
* Sample Payroll Data

This allows the application to run immediately after database setup without manual data entry.



## Assumptions

* PF deduction is fixed at 12% of Basic Salary.
* Professional Tax is fixed at ₹200 per month.
* Payroll records are immutable after finalization.
* Attendance data is available before payroll processing.



## Future Enhancements

* Employee CRUD operations
* Authentication & Authorization
* Export payroll to Excel/PDF
* Email payslip delivery
* Advanced payroll reports
* Role-based access control



## Author

**Mayur Kature**

ASP.NET Full Stack Developer

Technologies: ASP.NET Core, C#, SQL Server, JavaScript, Angular, Dapper, REST APIs
