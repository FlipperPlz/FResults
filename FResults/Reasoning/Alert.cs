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
    public abstract string? AlertName { get; set; }
    public abstract Type? AlertScope { get; set; }
    public abstract string? Message { get; set; }
    public abstract bool IsError { get; }

    public Dictionary<string, object> Metadata { get; protected set; }
    public List<IAlert>? Reasons { get; protected set; }
    public IEnumerable<Error> ErrorReasons => Reasons?.OfType<Error>() ?? Enumerable.Empty<Error>();
    public IEnumerable<Warning> WarningReasons =>  Reasons?.OfType<Warning>() ?? Enumerable.Empty<Warning>();

    protected Alert() => Metadata = new Dictionary<string, object>();

    protected Alert(
        IAlert causedBy
    ) : this() => CausedBy(causedBy);

    public IAlert CausedBy(IAlert error)
    {
        (Reasons ??= new List<IAlert>()).Add(error);
        return this;
    }

    public IAlert CausedBy(string message, bool isWarning = false)
    {
        IAlert causation = isWarning ? new Warning { Message = message } : new Error { Message = message };
        (Reasons ??= new List<IAlert>()).Add(causation);
        return this;
    }

}
