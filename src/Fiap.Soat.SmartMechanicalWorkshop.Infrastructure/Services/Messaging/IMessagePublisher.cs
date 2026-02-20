namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default) where T : class;
}
