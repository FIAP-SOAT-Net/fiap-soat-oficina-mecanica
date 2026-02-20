using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;

public sealed class NotifyServiceOrderCompletionHandler(
    ILogger<NotifyServiceOrderCompletionHandler> logger,
    IMessagePublisher messagePublisher,
    IConfiguration configuration) : INotificationHandler<UpdateServiceOrderStatusNotification>
{
    public async Task Handle(UpdateServiceOrderStatusNotification notification, CancellationToken cancellationToken)
    {
        var serviceOrder = notification.ServiceOrder;
        if (serviceOrder.Status != ServiceOrderStatus.Delivered) return;

        var clientEmail = serviceOrder.Client.Email;
        var clientName = serviceOrder.Client.Fullname;
        var serviceOrderId = serviceOrder.Id;
        var title = serviceOrder.Title;
        var description = serviceOrder.Description;

        var message = new
        {
            ClientEmail = clientEmail,
            ClientName = clientName,
            ServiceOrderId = serviceOrderId,
            Title = title,
            Description = description
        };

        logger.LogInformation("Publishing notification for delivered service order: {ServiceOrderId}", serviceOrderId);

        var notificationsExchange = configuration.GetValue<string>("RabbitMQ:NotificationsExchangeName") ?? "notifications.exchange";
        string routingKey = $"service-order.delivered.{serviceOrderId}";

        await messagePublisher.PublishAsync(notificationsExchange, routingKey, message, cancellationToken);
    }
}
