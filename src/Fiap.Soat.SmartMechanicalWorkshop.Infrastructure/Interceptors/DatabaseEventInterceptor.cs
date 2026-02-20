using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Interceptors;

public class DatabaseEventInterceptor(IMessagePublisher messagePublisher, ILogger<DatabaseEventInterceptor> logger)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);
        await PublishDatabaseEventsAsync(context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChanges(eventData, result);
        PublishDatabaseEventsAsync(context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavingChanges(eventData, result);
    }

    private async Task PublishDatabaseEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                string eventType = entry.State switch
                {
                    EntityState.Added => "INSERT",
                    EntityState.Modified => "UPDATE",
                    EntityState.Deleted => "DELETE",
                    _ => "UNKNOWN"
                };

                string entityType = entry.Entity.GetType().Name;
                string entityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "unknown";

                var data = new Dictionary<string, object>();
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    object? propertyValue = property.CurrentValue;

                    if (propertyValue is not null)
                    {
                        data[propertyName] = propertyValue;
                    }
                }

                var databaseEvent = new DatabaseEvent(entityType, entityId, eventType, data);
                string routingKey = $"database.{eventType.ToLower()}.{entityType.ToLower()}";
                await messagePublisher.PublishAsync(routingKey, databaseEvent, cancellationToken);
                logger.LogInformation(
                    "Published database event: {EventType} on {EntityType} with ID {EntityId}",
                    eventType,
                    entityType,
                    entityId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing database events to RabbitMQ");
        }
    }
}
