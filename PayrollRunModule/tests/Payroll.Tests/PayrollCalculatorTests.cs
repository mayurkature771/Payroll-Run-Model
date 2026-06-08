using Payroll.Api.Common;
using Xunit;

namespace Payroll.Tests;

public class PayrollCalculatorTests
{
    // The exact example from the assessment brief:
    // Ravi Sharma | Basic 30000 | Working days 26 | Present 24
    // Gross 27692, PF 3600, PT 200, Net 23892
    [Fact]
    public void Brief_Example_Produces_Expected_NetPay()
    {
        var (gross, pf, pt, net) = PayrollCalculator.Compute(30000m, 26, 24);

        Assert.Equal(27692m, gross);
        Assert.Equal(3600m, pf);
        Assert.Equal(200m, pt);
        Assert.Equal(23892m, net);
    }

    [Fact]
    public void Pf_Is_Twelve_Percent_Of_Basic()
    {
        Assert.Equal(5400m, PayrollCalculator.CalculatePf(45000m));
    }

    [Theory]
    [InlineData(26, 26)] // full attendance -> gross == basic
    public void Full_Attendance_Gross_Equals_Basic(int workingDays, int present)
    {
        Assert.Equal(50000m, PayrollCalculator.CalculateGross(50000m, workingDays, present));
    }

    [Fact]
    public void Zero_Days_Present_Gives_Zero_Net_Not_Negative()
    {
        var (gross, _, _, net) = PayrollCalculator.Compute(30000m, 26, 0);

        Assert.Equal(0m, gross);
        Assert.Equal(0m, net); // clamped at 0, never negative
    }

    [Fact]
    public void Zero_Working_Days_Does_Not_Divide_By_Zero()
    {
        // Missing attendance edge case -> treated as 0
        Assert.Equal(0m, PayrollCalculator.CalculateGross(30000m, 0, 0));
    }

    [Fact]
    public void Net_Is_Gross_Minus_Pf_Minus_ProfessionalTax()
    {
        var gross = PayrollCalculator.CalculateGross(60000m, 22, 20); // 54545
        var pf = PayrollCalculator.CalculatePf(60000m);              // 7200
        var net = PayrollCalculator.CalculateNet(gross, pf, PayrollCalculator.ProfessionalTax);

        Assert.Equal(gross - pf - 200m, net);
    }
}
