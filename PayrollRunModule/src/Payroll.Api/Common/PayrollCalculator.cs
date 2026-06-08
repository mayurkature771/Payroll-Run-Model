namespace Payroll.Api.Common;

/// <summary>
/// Pure C# implementation of the payroll formula.
/// The stored procedure is the source of truth at run-time, but this class
/// encodes the exact same rules so the logic is unit-testable in isolation.
/// </summary>
public static class PayrollCalculator
{
    public const decimal PfRate = 0.12m;          // 12% of basic salary
    public const decimal ProfessionalTax = 200m;  // flat per month

    /// <summary>(Basic / TotalWorkingDays) * DaysPresent, rounded to the nearest rupee.</summary>
    public static decimal CalculateGross(decimal basicSalary, int totalWorkingDays, int daysPresent)
    {
        if (totalWorkingDays <= 0) return 0m; // guard against divide-by-zero / missing attendance
        var gross = (basicSalary / totalWorkingDays) * daysPresent;
        return Math.Round(gross, 0, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculatePf(decimal basicSalary)
        => Math.Round(basicSalary * PfRate, 2, MidpointRounding.AwayFromZero);

    /// <summary>Gross - PF - Professional Tax, never below zero.</summary>
    public static decimal CalculateNet(decimal gross, decimal pf, decimal professionalTax)
    {
        var net = gross - pf - professionalTax;
        return net < 0 ? 0m : net;
    }

    /// <summary>Convenience: compute all components for one employee.</summary>
    public static (decimal Gross, decimal Pf, decimal Pt, decimal Net) Compute(
        decimal basicSalary, int totalWorkingDays, int daysPresent)
    {
        var gross = CalculateGross(basicSalary, totalWorkingDays, daysPresent);
        var pf = CalculatePf(basicSalary);
        var net = CalculateNet(gross, pf, ProfessionalTax);
        return (gross, pf, ProfessionalTax, net);
    }
}
