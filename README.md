# Employee Payroll Run Module

A small but complete payroll feature that replaces an HR team's monthly Excel process.
HR picks a month/year, triggers a payroll run, and views per-employee results (with a
printable payslip). Built with the preferred stack: **ASP.NET Core 8 Web API + Dapper +
SQL Server**, with a single-page HTML/JS frontend.

## Architecture

```
HTML / JS frontend  (wwwroot/index.html)
        │  fetch JSON
        ▼
API Controllers     (EmployeesController, PayrollController)   -> HTTP status codes
        ▼
Service Layer       (EmployeeService, PayrollService)          -> business logic, pagination
        ▼
Repository Layer    (EmployeeRepository, PayrollRepository)    -> Dapper / ADO.NET
        ▼
Stored Procedure    (usp_RunPayroll)                           -> calculation + save
        ▼
SQL Server          (Departments, Employees, Attendance, PayrollRuns, PayrollDetails)
```

## Business rules

| Component        | Formula                                        |
| ---------------- | ---------------------------------------------- |
| Gross Pay        | `ROUND((Basic ÷ TotalWorkingDays) × DaysPresent, 0)` |
| PF Deduction     | 12% of Basic Salary                            |
| Professional Tax | Flat ₹200 / month                              |
| Net Pay          | `Gross − PF − Professional Tax` (never below 0)|
| Immutability     | A finalised run can't be edited or deleted     |

Brief example (Ravi, Basic 30000, 26 working / 24 present): Gross **27,692**, PF **3,600**,
PT **200**, Net **23,892** — verified by a unit test and reproduced by the seed data.

---

## Prerequisites

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- SQL Server — **LocalDB** (ships with Visual Studio / "SQL Server Express LocalDB") or any
  full SQL Server instance
- A query tool to run the SQL scripts: **SSMS**, **Azure Data Studio**, or `sqlcmd`

---

## 1. Database setup

Run the three scripts **in order** against your SQL Server instance:

```
database/01_schema.sql        -- creates PayrollDb, tables, FKs, constraints, indexes, immutability triggers
database/02_seed.sql          -- 2 departments, 5 employees, attendance for the CURRENT month
database/03_usp_RunPayroll.sql-- the usp_RunPayroll stored procedure
```

Using `sqlcmd` with LocalDB (from the `database` folder):

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 01_schema.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 02_seed.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -i 03_usp_RunPayroll.sql
```

> The seed deliberately uses `MONTH(GETDATE())` / `YEAR(GETDATE())`, so attendance is always
> for the month you run it in — the "current month" the brief asks for.

### Connection string

Configured in `src/Payroll.Api/appsettings.json` under `ConnectionStrings:PayrollDb`.
Default (LocalDB):

```
Server=(localdb)\MSSQLLocalDB;Database=PayrollDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Format for a full instance / SQL auth:

```
Server=YOUR_SERVER;Database=PayrollDb;User Id=USER;Password=PASS;TrustServerCertificate=True;
```

No secrets are committed; change the connection string in `appsettings.json` (or override via
`appsettings.local.json`, which is git-ignored).

---

## 2. Run the API + frontend

```powershell
cd src/Payroll.Api
dotnet run
```

Then open the URL printed in the console (default **http://localhost:5080**):

- **`/`** — the HR frontend (select month → Run Payroll → results table → Print payslip)
- **`/swagger`** — interactive API docs

The frontend is served from `wwwroot`, so it calls the API on the same origin (no CORS setup
needed for normal use; CORS is still enabled for convenience).

## 3. Run the unit tests

```powershell
cd tests/Payroll.Tests
dotnet test
```

---

## API endpoints

| Method | Endpoint                                    | Result                          |
| ------ | ------------------------------------------- | ------------------------------- |
| GET    | `/api/employees`                            | 200 — all employees             |
| POST   | `/api/payroll/run`  body `{ "month", "year" }` | 201 — run summary, **409** if already exists |
| GET    | `/api/payroll/{month}/{year}`               | 200 (paginated) or 404          |
| GET    | `/api/payroll/{runId}/slip/{employeeId}`    | 200 or 404                      |

Each payroll line includes: `EmployeeId, Name, BasicSalary, WorkingDays, DaysPresent,
GrossPay, PFDeduction, ProfessionalTax, NetPay`.

Pagination (bonus): `GET /api/payroll/{month}/{year}?page=1&pageSize=50`.

### Quick test (PowerShell)

```powershell
# Run payroll for the current month
Invoke-RestMethod -Uri http://localhost:5080/api/payroll/run -Method Post `
  -ContentType 'application/json' -Body '{ "month": 6, "year": 2026 }'

# Fetch it back
Invoke-RestMethod -Uri http://localhost:5080/api/payroll/6/2026
```

---

## Bonus features included

- ✅ **HTTP 409 Conflict** when a run already exists for the month/year
  (enforced by a UNIQUE constraint + `THROW 50409`, mapped to 409 in the repository).
- ✅ **Unit tests** for the net-pay logic (xUnit, `PayrollCalculatorTests`).
- ✅ **Pagination** on `GET /api/payroll/{month}/{year}`.
- ✅ **Printable payslip** per employee (frontend "Print" button → clean print window).

---

## Design decisions & assumptions

- **Attendance is a monthly summary** (`TotalWorkingDays`, `DaysPresent`) per employee, not
  one row per day. This matches the formula in the brief and keeps the model simple; a daily
  table could be added later and aggregated.
- **Gross is rounded to the nearest rupee** (`ROUND(..., 0)`) so the output matches the
  brief's example exactly (₹27,692 / ₹23,892). PF is kept to 2 decimals.
- **Immutability** is enforced two ways: a UNIQUE `(Month, Year)` constraint stops a second
  run, and `INSTEAD OF UPDATE, DELETE` triggers block edits/deletes on the payroll tables.
  The stored procedure inserts the run header with totals already aggregated, so it never
  needs to UPDATE.
- **Snapshotting**: `PayrollDetails` stores the computed values (basic, gross, net, …) at run
  time. A later salary change never alters a finalised payslip.
- **Edge cases**:
  - *0 days present* → Gross 0, Net clamped to 0 (not negative). Seeded for "Karan Mehta".
  - *Missing attendance* → the SP `LEFT JOIN`s, so an active employee with no attendance row
    is still included with 0 working/0 present (rather than being silently dropped).
  - *Divide-by-zero* (0 working days) → guarded in both the SP and `PayrollCalculator`.
- **Calculation lives in the stored procedure** (the brief requires it). `PayrollCalculator`
  mirrors the same formula in C# purely so the logic is unit-testable in isolation.
- **Only active employees** (`IsActive = 1`) are included in a run.

## What I'd add with more time

- Persist a `PayrollRunStatus` (Draft → Finalised) so HR can preview before locking.
- Authentication/authorization (HR role) — currently the API is open for the demo.
- Server-side, SQL-level pagination (`OFFSET/FETCH`) instead of paging in the service layer;
  fine at this scale, but cleaner for thousands of rows.
- Integration tests against a real/LocalDB database for the stored procedure itself.
- Configurable PF rate / Professional Tax (slabs by salary/state) instead of constants.
- Export payslip to PDF and email it to employees.

## Project structure

```
PayrollRunModule/
├─ database/                  01_schema.sql, 02_seed.sql, 03_usp_RunPayroll.sql
├─ src/Payroll.Api/
│  ├─ Controllers/            EmployeesController, PayrollController
│  ├─ Services/               business logic + pagination
│  ├─ Repositories/           Dapper data access
│  ├─ Models/                 DTOs
│  ├─ Common/                 PayrollCalculator, exceptions
│  ├─ Data/                   DbConnectionFactory
│  └─ wwwroot/index.html      the HR frontend
├─ tests/Payroll.Tests/       xUnit tests
└─ PayrollRunModule.sln
```
