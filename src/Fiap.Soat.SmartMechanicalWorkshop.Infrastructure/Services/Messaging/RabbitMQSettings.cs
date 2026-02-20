namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Messaging;

public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "database.events.exchange";
    public string QueueName { get; set; } = "database.events";
    public string NotificationsExchangeName { get; set; } = "notifications.exchange";
    public string NotificationsQueueName { get; set; } = "service-order.notifications";
}
