namespace FResults.Reasoning;

public class Error : Alert
{
    public Error(string message = "Undocumented Error") : base(message, true)
    {
    }

    public Error(string message, IAlert causedBy) : base(message, causedBy, true)
    {
    }
}
