/* =====================================================================
   Employee Payroll Run Module - Database Schema
   Target: SQL Server (LocalDB or full instance)
   Run order: 01_schema.sql -> 02_seed.sql -> 03_usp_RunPayroll.sql
   This script is re-runnable (drops objects before creating).
   ===================================================================== */

IF DB_ID('PayrollDb') IS NULL
    CREATE DATABASE PayrollDb;
GO

USE PayrollDb;
GO

/* ---- Drop in dependency (child -> parent) order ---- */
IF OBJECT_ID('dbo.PayrollDetails','U') IS NOT NULL DROP TABLE dbo.PayrollDetails;
IF OBJECT_ID('dbo.PayrollRuns','U')    IS NOT NULL DROP TABLE dbo.PayrollRuns;
IF OBJECT_ID('dbo.Attendance','U')     IS NOT NULL DROP TABLE dbo.Attendance;
IF OBJECT_ID('dbo.Employees','U')      IS NOT NULL DROP TABLE dbo.Employees;
IF OBJECT_ID('dbo.Departments','U')    IS NOT NULL DROP TABLE dbo.Departments;
GO

/* ---------------------------------------------------------------
   Departments
   --------------------------------------------------------------- */
CREATE TABLE dbo.Departments
(
    DepartmentId INT IDENTITY(1,1) NOT NULL,
    Name         NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Departments PRIMARY KEY (DepartmentId),
    CONSTRAINT UQ_Departments_Name UNIQUE (Name)
);
GO

/* ---------------------------------------------------------------
   Employees
   --------------------------------------------------------------- */
CREATE TABLE dbo.Employees
(
    EmployeeId    INT IDENTITY(1,1) NOT NULL,
    FullName      NVARCHAR(150)     NOT NULL,
    Email         NVARCHAR(150)     NOT NULL,
    DepartmentId  INT               NOT NULL,
    BasicSalary   DECIMAL(18,2)     NOT NULL,
    DateOfJoining DATE              NOT NULL,
    IsActive      BIT               NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
    CONSTRAINT PK_Employees PRIMARY KEY (EmployeeId),
    CONSTRAINT UQ_Employees_Email UNIQUE (Email),
    CONSTRAINT CK_Employees_BasicSalary CHECK (BasicSalary >= 0),
    CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId)
        REFERENCES dbo.Departments (DepartmentId)
);
GO

/* ---------------------------------------------------------------
   Attendance  (one monthly summary row per employee per period)
   --------------------------------------------------------------- */
CREATE TABLE dbo.Attendance
(
    AttendanceId     INT IDENTITY(1,1) NOT NULL,
    EmployeeId       INT               NOT NULL,
    [Month]          TINYINT           NOT NULL,
    [Year]           SMALLINT          NOT NULL,
    TotalWorkingDays TINYINT           NOT NULL,
    DaysPresent      TINYINT           NOT NULL,
    CONSTRAINT PK_Attendance PRIMARY KEY (AttendanceId),
    CONSTRAINT FK_Attendance_Employees FOREIGN KEY (EmployeeId)
        REFERENCES dbo.Employees (EmployeeId),
    CONSTRAINT UQ_Attendance_Period UNIQUE (EmployeeId, [Month], [Year]),
    CONSTRAINT CK_Attendance_Month CHECK ([Month] BETWEEN 1 AND 12),
    CONSTRAINT CK_Attendance_Year  CHECK ([Year]  BETWEEN 2000 AND 2100),
    CONSTRAINT CK_Attendance_WorkingDays CHECK (TotalWorkingDays BETWEEN 1 AND 31),
    CONSTRAINT CK_Attendance_DaysPresent CHECK (DaysPresent >= 0 AND DaysPresent <= TotalWorkingDays)
);
GO

/* ---------------------------------------------------------------
   PayrollRuns  (one finalised run per Month/Year)
   The UNIQUE on (Month,Year) is what makes a second run for the
   same period impossible -> drives the 409 Conflict.
   --------------------------------------------------------------- */
CREATE TABLE dbo.PayrollRuns
(
    RunId         INT IDENTITY(1,1) NOT NULL,
    [Month]       TINYINT           NOT NULL,
    [Year]        SMALLINT          NOT NULL,
    RunDateUtc    DATETIME2(0)      NOT NULL CONSTRAINT DF_PayrollRuns_RunDate DEFAULT (SYSUTCDATETIME()),
    EmployeeCount INT               NOT NULL CONSTRAINT DF_PayrollRuns_EmpCount DEFAULT (0),
    TotalNetPay   DECIMAL(18,2)     NOT NULL CONSTRAINT DF_PayrollRuns_TotalNet DEFAULT (0),
    IsFinalised   BIT               NOT NULL CONSTRAINT DF_PayrollRuns_Finalised DEFAULT (1),
    CONSTRAINT PK_PayrollRuns PRIMARY KEY (RunId),
    CONSTRAINT UQ_PayrollRuns_Period UNIQUE ([Month], [Year]),
    CONSTRAINT CK_PayrollRuns_Month CHECK ([Month] BETWEEN 1 AND 12),
    CONSTRAINT CK_PayrollRuns_Year  CHECK ([Year]  BETWEEN 2000 AND 2100)
);
GO

/* ---------------------------------------------------------------
   PayrollDetails  (immutable per-employee snapshot for a run)
   Values are stored (snapshotted), NOT recomputed on read, so a
   later salary change never alters a finalised payslip.
   --------------------------------------------------------------- */
CREATE TABLE dbo.PayrollDetails
(
    PayrollDetailId INT IDENTITY(1,1) NOT NULL,
    RunId           INT               NOT NULL,
    EmployeeId      INT               NOT NULL,
    BasicSalary     DECIMAL(18,2)     NOT NULL,
    WorkingDays     TINYINT           NOT NULL,
    DaysPresent     TINYINT           NOT NULL,
    GrossPay        DECIMAL(18,2)     NOT NULL,
    PFDeduction     DECIMAL(18,2)     NOT NULL,
    ProfessionalTax DECIMAL(18,2)     NOT NULL,
    NetPay          DECIMAL(18,2)     NOT NULL,
    CONSTRAINT PK_PayrollDetails PRIMARY KEY (PayrollDetailId),
    CONSTRAINT FK_PayrollDetails_Runs FOREIGN KEY (RunId)
        REFERENCES dbo.PayrollRuns (RunId),
    CONSTRAINT FK_PayrollDetails_Employees FOREIGN KEY (EmployeeId)
        REFERENCES dbo.Employees (EmployeeId),
    CONSTRAINT UQ_PayrollDetails UNIQUE (RunId, EmployeeId)
);
GO

/* ---------------------------------------------------------------
   Indexes for the common lookups
   --------------------------------------------------------------- */
CREATE INDEX IX_Employees_Department ON dbo.Employees (DepartmentId);
CREATE INDEX IX_Attendance_Period    ON dbo.Attendance ([Year], [Month]);
CREATE INDEX IX_PayrollDetails_Run   ON dbo.PayrollDetails (RunId);
GO

/* ---------------------------------------------------------------
   Immutability guard: once a run is written it cannot be edited
   or deleted. The stored procedure inserts the run with its final
   totals already computed, so it never needs to UPDATE either table.
   --------------------------------------------------------------- */
IF OBJECT_ID('dbo.trg_PayrollRuns_Immutable','TR') IS NOT NULL
    DROP TRIGGER dbo.trg_PayrollRuns_Immutable;
GO
CREATE TRIGGER dbo.trg_PayrollRuns_Immutable
ON dbo.PayrollRuns
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    RAISERROR('A finalised payroll run is immutable and cannot be edited or deleted.', 16, 1);
END
GO

IF OBJECT_ID('dbo.trg_PayrollDetails_Immutable','TR') IS NOT NULL
    DROP TRIGGER dbo.trg_PayrollDetails_Immutable;
GO
CREATE TRIGGER dbo.trg_PayrollDetails_Immutable
ON dbo.PayrollDetails
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    RAISERROR('Payroll detail rows are immutable and cannot be edited or deleted.', 16, 1);
END
GO

PRINT 'Schema created successfully.';
GO
