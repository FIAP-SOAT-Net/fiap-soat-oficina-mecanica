using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;

public sealed class UpdateServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository,
    IAvailableServiceRepository availableServiceRepository,
    ILogger<UpdateServiceOrderHandler> logger) : IRequestHandler<UpdateServiceOrderCommand, Response<ServiceOrder>>
{
    public async Task<Response<ServiceOrder>> Handle(UpdateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var entity = await serviceOrderRepository.GetAsync(request.Id, cancellationToken);
            if (entity is null)
            {
                return ResponseFactory.Fail<ServiceOrder>("Service Order not found", HttpStatusCode.NotFound);
            }

            var services = new List<AvailableService>();
            foreach (var service in request.ServiceIds)
            {
                var foundService = await availableServiceRepository.GetByIdAsync(service, cancellationToken);
                if (foundService is null)
                {
                    return ResponseFactory.Fail<ServiceOrder>($"Available Service with ID {service} not found",
                        HttpStatusCode.NotFound);
                }

                services.Add(foundService);
            }

            var updatedEntity = await serviceOrderRepository.UpdateAsync(request.Id, request.Title, request.Description, services, cancellationToken);

            logger.LogInformation(
                "Service order {OrderId} modified successfully with {ServicesCount} services",
                updatedEntity.Id, request.ServiceIds.Count);

            return ResponseFactory.Ok(updatedEntity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating service order {OrderId}", request.Id);
            throw;
        }
    }
}
