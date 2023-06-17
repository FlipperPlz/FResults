namespace FResults.Reasoning;

using System.Text.Json.Serialization;

public abstract class ErrorBase : Alert
{
    [JsonIgnore]
    public abstract override string? AlertName { get; }
    public abstract override Type? AlertScope { get; }
    public abstract override string? Message { get; }
    public sealed override bool IsError { get => true; set => throw new NotSupportedException(); }
}

public class Error : ErrorBase
{
    public override string? AlertName { get; init; }
    public override Type? AlertScope { get; init; }
    public override string? Message { get; set; }

    public override string ToString() => $"({AlertScope}) [ERR] {AlertName}: {Message}";

}
