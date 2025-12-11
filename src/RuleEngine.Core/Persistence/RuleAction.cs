namespace RuleEngine.Core;

public class RuleAction
{
    public string ActionCode { get; set; } // e.g., "ADD_FEE", "REJECT"
    public Dictionary<string, string> Parameters { get; set; }
}