namespace FResults.Reasoning;

public class Warning : Alert
{
    public Warning(string message = "Undocumented Warning", bool isError = false) : base(message, isError)
    {
    }

    public Warning(string message, IAlert causedBy, bool isError = false) : base(message, causedBy, isError)
    {
    }
}
