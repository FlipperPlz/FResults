namespace FResults.Reasoning;


public class Error : Alert
{
    public Error(string? errorName = null, Type? errorScope = null, string? message = null) : base(errorName, errorScope, message, true)
    {
    }

    public Error(IAlert causedBy, string? message = null, string? errorName = null, Type? errorScope = null) : base(errorName, errorScope, message, causedBy, true)
    {
    }
}
