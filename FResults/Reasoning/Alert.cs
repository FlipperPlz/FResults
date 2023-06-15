namespace FResults.Reasoning;

public interface IAlert : IReason
{
    
    string? AlertName { get; }
    
    Type? AlertScope { get; }
    
    string? AlertScopeName => AlertScope?.Name;

    List<IAlert>? Reasons { get; }
    
    IEnumerable<Error> ErrorReasons { get; }
    
    IEnumerable<Warning> WarningReasons { get; }

    bool IsError { get; }
    
    //TODO: Write message for alert, maybe add alert weight
}

public abstract class Alert : IAlert
{
    public bool IsError { get; }

    public string? Message { get; protected set; }
    public string? AlertName { get; protected set;}
    public Type? AlertScope { get; protected set; }
    public Dictionary<string, object> Metadata { get; protected set; }
    public List<IAlert>? Reasons { get; protected set; }
    public IEnumerable<Error> ErrorReasons => Reasons?.OfType<Error>() ?? Enumerable.Empty<Error>();
    public IEnumerable<Warning> WarningReasons =>  Reasons?.OfType<Warning>() ?? Enumerable.Empty<Warning>();

    protected Alert(string? alertName, Type? alertScope, string? message, bool isError)
    {
        Metadata = new Dictionary<string, object>();
        AlertName = alertName;
        AlertScope = alertScope;
        Message = message;
        IsError = isError;
    }

    protected Alert(
        string? alertName,
        Type? alertScope,
        string? message,
        IAlert causedBy,
        bool isError
    ) : this(alertName, alertScope, message, isError) => CausedBy(causedBy);

    public IAlert CausedBy(IAlert error)
    {
        (Reasons ??= new List<IAlert>()).Add(error);
        return this;
    }

    public IAlert CausedBy(string message, bool isWarning = false)
    {
        IAlert causation = isWarning ? new Warning(message) : new Error(message);
        (Reasons ??= new List<IAlert>()).Add(causation);
        return this;
    }

}
