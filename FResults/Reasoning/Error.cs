namespace FResults.Reasoning;


public class Error : Alert
{
    public override string? AlertName { get; init; }
    public override Type? AlertScope { get; init; }
    public override string? Message { get; init; }
    public override bool IsError => true;
}
