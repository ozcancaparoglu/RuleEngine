using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RuleEngine.Core;

public class RuleDefinition
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string RuleName { get; set; }
    public int Priority { get; set; }
    public bool IsEnabled { get; set; }
    public string Expression { get; set; } // e.g. "Input.Amount > 100"
    public List<RuleAction> SuccessActions { get; set; }
}