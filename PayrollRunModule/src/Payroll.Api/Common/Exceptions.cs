namespace Payroll.Api.Common;

/// <summary>Thrown when a payroll run already exists for the period (-> HTTP 409).</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Thrown when a business rule prevents the operation (-> HTTP 400).</summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
