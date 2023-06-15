namespace FResults.Reasoning;

public abstract class ErrorBase : Alert
{
    public abstract override string? AlertName { get; }
    public abstract override Type? AlertScope { get; }
    public abstract override string? Message { get; set; }
    public sealed override bool IsError { get => true; set => throw new NotSupportedException(); }
}

public class Error : ErrorBase
{
    public override string? AlertName { get; init; }
    public override Type? AlertScope { get; init; }
    public override string? Message { get; set; }
}
