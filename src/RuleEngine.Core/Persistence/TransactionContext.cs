namespace RuleEngine.Core;

public class TransactionContext
{
    public string TransactionId { get; set; } 
    public decimal Amount { get; set; }
    public string Currency { get; set; } 
    public string Country { get; set; }
    
    public Dictionary<string, object> Attributes { get; set; } = new();
}