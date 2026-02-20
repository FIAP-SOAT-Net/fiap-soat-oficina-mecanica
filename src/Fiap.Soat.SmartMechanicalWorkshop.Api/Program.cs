using Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Extensions;
using Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.HealthChecks;
using Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Middlewares;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Mappers;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Shared;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.ValueObjects;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Data;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Interceptors;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Services.Messaging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

_ = builder.Host.UseSerilog((context, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SmartMechanicalWorkshop"));

_ = builder.Logging.ClearProviders();
_ = builder.Services.AddControllers();
_ = builder.Services.AddEndpointsApiExplorer();
_ = builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Configure RabbitMQ
_ = builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));
_ = builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
_ = builder.Services.AddScoped<DatabaseEventInterceptor>();

_ = builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<DatabaseEventInterceptor>();
    options.UseMySql(
        builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetValue<string>("ConnectionStrings:DefaultConnection")),
        mySqlOptions =>
            mySqlOptions.MigrationsAssembly("Fiap.Soat.SmartMechanicalWorkshop.Infrastructure")
    ).AddInterceptors(interceptor);
});

_ = builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

// Configure Data Protection to use persistent storage
var dataProtectionKeysPath = builder.Configuration.GetValue<string>("DataProtection:KeysPath") ?? "/app/keys";
if (Directory.Exists(dataProtectionKeysPath) || builder.Environment.IsProduction())
{
    var dataProtectionBuilder = builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("SmartMechanicalWorkshop");

    // In production, you should configure a proper key encryption mechanism
    // For now, we'll accept unencrypted keys in the file system
    // For better security, consider using:
    // - .ProtectKeysWithCertificate() with a proper certificate
    // - Azure Key Vault: .ProtectKeysWithAzureKeyVault()
    // - AWS KMS, etc.
}

_ = builder.Services.AddServiceExtensions();
_ = builder.Services.AddRepositoryExtensions();
_ = builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));
_ = builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpdateServiceOrderStatusCommand).Assembly));
_ = builder.Services.AddHttpContextAccessor();
_ = builder.Services.AddHealthChecks()
    .AddCheck<DetailedHealthCheck>("detailed")
    .AddDbContextCheck<AppDbContext>("database");
_ = builder.Services.AddRouting(options => options.LowercaseUrls = true);
_ = builder.Services.AddAuthenticationExtension(builder.Configuration);
_ = builder.Services.AddSwaggerExtension(builder.Configuration);
_ = builder.Services.AddMemoryCache();
_ = builder.Services.AddInterfaceAdapters();

var app = builder.Build();

_ = app.UseMiddleware<RequestLoggingEnrichmentMiddleware>();

_ = app.UseSwagger();
_ = app.UseSwaggerUI(c =>
{
    c.EnableTryItOutByDefault();
    c.DisplayRequestDuration();
});

_ = app.UseReDoc(c =>
{
    c.RoutePrefix = "docs";
    c.DocumentTitle = "Smart Mechanical Workshop API Documentation";
    c.SpecUrl = "/swagger/v1/swagger.json";
});
_ = app.UseMiddleware<ExceptionMiddleware>();
_ = app.UseHttpsRedirection();
_ = app.UseAuthorization();
_ = app.MapControllers();
_ = app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
