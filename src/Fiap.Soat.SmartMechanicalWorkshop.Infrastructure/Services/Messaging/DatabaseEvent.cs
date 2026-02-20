namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Messaging;

public class DatabaseEvent
{
    public DatabaseEvent() { }

    public DatabaseEvent(string entityType, string entityId, string eventType, Dictionary<string, object> data)
    : this()
    {
        EntityType = entityType;
        EntityId = entityId;
        EventType = eventType;
        Data = data;
        EntityId = Guid.NewGuid().ToString();
        Timestamp = DateTime.UtcNow;
    }

    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
}
