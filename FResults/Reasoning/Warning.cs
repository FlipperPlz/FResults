namespace FResults.Reasoning;

using System.Text.Json.Serialization;

public abstract class WarningBase : Alert
{
    public abstract override string? AlertName { get; }
    public abstract override Type? AlertScope { get; }
    public abstract override string? Message { get; }
    public abstract override bool IsError { get; }

    public override string ToString() => IsError ? $"({AlertScope}) [ERR_WARN] {AlertName}: {Message}" : $"({AlertScope}) [WARN] {AlertName}: {Message}";
}

public class Warning : WarningBase
{
    public override string? AlertName { get; init; }
    public override Type? AlertScope { get; init; }
    public override string? Message { get; set; }
    public override bool IsError { get; set; }
}
