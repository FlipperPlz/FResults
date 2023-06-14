namespace FResults.Reasoning;

public interface IAlert : IReason
{
    List<IAlert>? Reasons { get; }
    bool IsError { get; }
}

public abstract class Alert : IAlert
{
    public bool IsError { get; }

    public string? Message { get; protected set; }
    public Dictionary<string, object> Metadata { get; protected set; } = new();
    public List<IAlert>? Reasons { get; protected set; }

    protected Alert(string message, bool isError)
    {
        Message = message;
        IsError = isError;
    }

    protected Alert(string message, IAlert causedBy, bool isError) : this(message, isError) => CausedBy(causedBy);

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
