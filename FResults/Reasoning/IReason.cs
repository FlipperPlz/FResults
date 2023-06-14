namespace FResults.Reasoning;

public interface IReason
{
    string? Message { get; }

    Dictionary<string, object> Metadata { get; }
}
