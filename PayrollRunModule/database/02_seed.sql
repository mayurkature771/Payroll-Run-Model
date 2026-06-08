/* =====================================================================
   Seed data: 5 employees across 2 departments + attendance for the
   CURRENT month (derived from GETDATE(), so it is always "this month").

   NOTE: Run this on a fresh schema (right after 01_schema.sql).
   To fully reset, re-run 01_schema.sql then this file. Payroll tables
   are immutable, so this script only seeds master data; it does not
   try to delete finalised runs.

   IDs are looked up by natural keys (Name / Email) instead of being
   hardcoded, so the script is robust regardless of IDENTITY values.
   ===================================================================== */

USE PayrollDb;
GO

/* Clear master data so the seed is re-runnable on a clean schema */
DELETE FROM dbo.Attendance;
DELETE FROM dbo.Employees;
DELETE FROM dbo.Departments;
GO

/* ---- Departments (2) ---- */
INSERT INTO dbo.Departments (Name) VALUES
('Engineering'),
('Human Resources');
GO

/* ---- Employees (5) - DepartmentId resolved by name ---- */
INSERT INTO dbo.Employees (FullName, Email, DepartmentId, BasicSalary, DateOfJoining, IsActive)
SELECT v.FullName, v.Email, d.DepartmentId, v.BasicSalary, v.DateOfJoining, 1
FROM (VALUES
    ('Ravi Sharma', 'ravi.sharma@example.com', 'Engineering',     30000.00, '2022-04-01'),
    ('Priya Patel',  'priya.patel@example.com', 'Engineering',     45000.00, '2021-09-15'),
    ('Amit Verma',   'amit.verma@example.com',  'Engineering',     38000.00, '2023-01-10'),
    ('Sneha Iyer',   'sneha.iyer@example.com',  'Human Resources', 52000.00, '2020-07-20'),
    ('Karan Mehta',  'karan.mehta@example.com', 'Human Resources', 28000.00, '2023-06-05')
) AS v(FullName, Email, DeptName, BasicSalary, DateOfJoining)
JOIN dbo.Departments d ON d.Name = v.DeptName;
GO

/* ---- Attendance for the CURRENT month/year - EmployeeId resolved by email ---- */
DECLARE @M TINYINT  = MONTH(GETDATE());
DECLARE @Y SMALLINT = YEAR(GETDATE());

INSERT INTO dbo.Attendance (EmployeeId, [Month], [Year], TotalWorkingDays, DaysPresent)
SELECT e.EmployeeId, @M, @Y, v.WorkingDays, v.DaysPresent
FROM (VALUES
    ('ravi.sharma@example.com', 26, 24),   -- matches the brief example (Gross 27692, Net 23892)
    ('priya.patel@example.com', 26, 26),   -- full attendance
    ('amit.verma@example.com',  26, 20),   -- partial
    ('sneha.iyer@example.com',  26, 25),   -- partial
    ('karan.mehta@example.com', 26, 0)     -- EDGE CASE: 0 days present
) AS v(Email, WorkingDays, DaysPresent)
JOIN dbo.Employees e ON e.Email = v.Email;
GO

PRINT 'Seed data inserted for the current month.';
GO
