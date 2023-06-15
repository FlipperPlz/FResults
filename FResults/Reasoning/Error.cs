namespace FResults.Reasoning;

public abstract class ErrorBase : Alert
{
    public abstract override string? AlertName { get; set; }
    public abstract override Type? AlertScope { get; set; }
    public abstract override string? Message { get; set; }
    public abstract override bool IsError { get; }
}

public class Error : Alert
{
    public override string? AlertName { get; set; }
    public override Type? AlertScope { get; set; }
    public override string? Message { get; set; }
    public override bool IsError => true;
}
