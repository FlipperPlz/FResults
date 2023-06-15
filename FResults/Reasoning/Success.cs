﻿namespace FResults.Reasoning;

public class Success : IReason
{
    public string? Message => null;
    public Dictionary<string, object> Metadata { get; } = new();
    
}
