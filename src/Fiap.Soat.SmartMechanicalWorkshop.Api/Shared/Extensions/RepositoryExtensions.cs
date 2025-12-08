using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Api.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositoryExtensions(this IServiceCollection serviceCollection)
    {
        _ = serviceCollection.AddScoped<IAvailableServiceRepository, AvailableServiceRepository>();
        _ = serviceCollection.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
        _ = serviceCollection.AddScoped<ISupplyRepository, SupplyRepository>();
        _ = serviceCollection.AddScoped<IVehicleRepository, VehicleRepository>();
        _ = serviceCollection.AddScoped<IPersonRepository, PersonRepository>();
        _ = serviceCollection.AddScoped<IQuoteRepository, QuoteRepository>();
        _ = serviceCollection.AddScoped<IServiceOrderEventRepository, ServiceOrderEventRepository>();
        _ = serviceCollection.AddScoped<IAddressRepository, AddressRepository>();

        return serviceCollection;
    }
}
