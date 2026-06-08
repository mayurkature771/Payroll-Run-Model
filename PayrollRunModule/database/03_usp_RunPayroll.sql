/* =====================================================================
   usp_RunPayroll  -  calculates and SAVES a payroll run for a period.

   Business rules:
     Gross Pay        = ROUND( (BasicSalary / TotalWorkingDays) * DaysPresent , 0 )
     PF Deduction     = 12% of BasicSalary
     Professional Tax = flat 200
     Net Pay          = Gross - PF - PT   (never below 0)

   Behaviour:
     - Validates @Month / @Year.
     - If a run already exists for the period -> THROW 50409 (API -> 409).
     - If there are no active employees to process -> THROW 50404.
     - Inserts the run header WITH final totals first (single insert),
       so it never updates the run -> immutability trigger stays happy.
     - Returns the run summary as a result set.
   ===================================================================== */

USE PayrollDb;
GO

IF OBJECT_ID('dbo.usp_RunPayroll','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_RunPayroll;
GO

CREATE PROCEDURE dbo.usp_RunPayroll
    @Month INT,
    @Year  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    /* ---- Input validation ---- */
    IF @Month IS NULL OR @Month NOT BETWEEN 1 AND 12
        THROW 50400, 'Invalid month. Month must be between 1 and 12.', 1;

    IF @Year IS NULL OR @Year NOT BETWEEN 2000 AND 2100
        THROW 50400, 'Invalid year. Year must be between 2000 and 2100.', 1;

    /* ---- Duplicate / immutability guard ---- */
    IF EXISTS (SELECT 1 FROM dbo.PayrollRuns WHERE [Month] = @Month AND [Year] = @Year)
        THROW 50409, 'A payroll run already exists for the selected month and year.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        /* ---- Per-employee calculation into a temp table ----
           LEFT JOIN so an active employee with MISSING attendance is
           still included (treated as 0 working / 0 present -> 0 net),
           rather than being silently dropped from the run.            */
        SELECT
            e.EmployeeId,
            e.BasicSalary,
            CAST(ISNULL(a.TotalWorkingDays, 0) AS TINYINT) AS WorkingDays,
            CAST(ISNULL(a.DaysPresent, 0)     AS TINYINT) AS DaysPresent,
            CAST(ROUND(
                    CASE WHEN ISNULL(a.TotalWorkingDays, 0) = 0 THEN 0
                         ELSE (e.BasicSalary / a.TotalWorkingDays) * a.DaysPresent
                    END, 0)
                 AS DECIMAL(18,2)) AS GrossPay,
            CAST(ROUND(e.BasicSalary * 0.12, 2) AS DECIMAL(18,2)) AS PFDeduction,
            CAST(200.00 AS DECIMAL(18,2)) AS ProfessionalTax,
            CAST(0 AS DECIMAL(18,2)) AS NetPay      -- filled in below
        INTO #Calc
        FROM dbo.Employees e
        LEFT JOIN dbo.Attendance a
               ON a.EmployeeId = e.EmployeeId
              AND a.[Month]    = @Month
              AND a.[Year]     = @Year
        WHERE e.IsActive = 1;

        IF NOT EXISTS (SELECT 1 FROM #Calc)
            THROW 50404, 'No active employees found to process for this period.', 1;

        /* Net Pay = Gross - PF - PT, clamped at 0 */
        UPDATE #Calc
        SET NetPay = CASE WHEN (GrossPay - PFDeduction - ProfessionalTax) < 0
                          THEN 0
                          ELSE (GrossPay - PFDeduction - ProfessionalTax)
                     END;

        /* ---- Insert run header with totals already aggregated ---- */
        DECLARE @RunId INT;

        INSERT INTO dbo.PayrollRuns ([Month], [Year], EmployeeCount, TotalNetPay, IsFinalised)
        SELECT @Month, @Year, COUNT(*), SUM(NetPay), 1
        FROM #Calc;

        SET @RunId = SCOPE_IDENTITY();

        /* ---- Insert immutable per-employee snapshot ---- */
        INSERT INTO dbo.PayrollDetails
            (RunId, EmployeeId, BasicSalary, WorkingDays, DaysPresent,
             GrossPay, PFDeduction, ProfessionalTax, NetPay)
        SELECT
            @RunId, EmployeeId, BasicSalary, WorkingDays, DaysPresent,
            GrossPay, PFDeduction, ProfessionalTax, NetPay
        FROM #Calc;

        DROP TABLE #Calc;

        COMMIT TRANSACTION;

        /* ---- Return the run summary ---- */
        SELECT RunId, [Month], [Year], RunDateUtc, EmployeeCount, TotalNetPay, IsFinalised
        FROM dbo.PayrollRuns
        WHERE RunId = @RunId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;  -- re-raise (50409 / 50404 / 50400 etc.) to the caller
    END CATCH
END
GO

PRINT 'Stored procedure usp_RunPayroll created successfully.';
GO
