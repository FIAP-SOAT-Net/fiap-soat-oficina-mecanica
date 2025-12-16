using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.ValueObjects;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceExtensions
{
    public static IServiceCollection AddServiceExtensions(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<IEmailService, EmailService>();
        _ = serviceCollection.AddScoped<IEmailTemplateProvider, EmailTemplateProvider>();
        _ = serviceCollection.AddScoped<IEmailSender, EmailSender>(opt =>
        {
            var emailSettings = opt.GetRequiredService<IOptions<EmailSettings>>().Value;
            var smtpClient = new SmtpClient
            {
                Host = emailSettings.SmtpHost,
                Port = emailSettings.SmtpPort,
                EnableSsl = emailSettings.EnableSsl,
                Credentials = new System.Net.NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword)
            };
            return new EmailSender(smtpClient);
        });

        return serviceCollection;
    }
}
