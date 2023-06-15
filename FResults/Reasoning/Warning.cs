namespace FResults.Reasoning;

public class Warning : Alert
{
    
    public Warning(string? errorName = null, Type? errorScope = null, string? message = null) : base(errorName, errorScope, message, true)
    {
    }

    public Warning(IAlert causedBy, string? message = null, string? errorName = null, Type? errorScope = null) : base(errorName, errorScope, message, causedBy, true)
    {
    }
    
}
