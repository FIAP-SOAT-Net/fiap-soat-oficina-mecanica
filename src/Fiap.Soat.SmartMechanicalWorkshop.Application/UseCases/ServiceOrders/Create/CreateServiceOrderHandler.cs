using AutoMapper;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Create;

public sealed class CreateServiceOrderHandler(
    IMapper mapper,
    IServiceOrderRepository serviceOrderRepository,
    IPersonRepository personRepository,
    IAvailableServiceRepository availableServiceRepository,
    IVehicleRepository vehicleRepository,
    INewRelicInstrumentationService newRelicService,
    ILogger<CreateServiceOrderHandler> logger) : IRequestHandler<CreateServiceOrderCommand, Response<ServiceOrder>>
{
    public async Task<Response<ServiceOrder>> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var entity = mapper.Map<ServiceOrder>(request);
            if (!await personRepository.AnyAsync(x => x.Id == entity.ClientId, cancellationToken))
            {
                return ResponseFactory.Fail<ServiceOrder>("Person not found", HttpStatusCode.NotFound);
            }

            if (!await vehicleRepository.AnyAsync(x => x.Id == entity.VehicleId, cancellationToken))
            {
                return ResponseFactory.Fail<ServiceOrder>("Vehicle not found", HttpStatusCode.NotFound);
            }

            foreach (var serviceId in request.ServiceIds)
            {
                var availableService = await availableServiceRepository.GetByIdAsync(serviceId, cancellationToken);
                if (availableService is null)
                {
                    return ResponseFactory.Fail<ServiceOrder>($"Service with Id {serviceId} not found",
                        HttpStatusCode.NotFound);
                }

                _ = entity.AddAvailableService(availableService);
            }

            var createdEntity = await serviceOrderRepository.AddAsync(entity, cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            // Record custom event in New Relic
            newRelicService.RecordServiceOrderEvent(
                action: "created",
                orderId: createdEntity.Id,
                status: createdEntity.Status.ToString(),
                customerId: createdEntity.ClientId,
                duration: duration,
                additionalAttributes: new Dictionary<string, object>
                {
                    { "vehicleId", createdEntity.VehicleId.ToString() },
                    { "servicesCount", request.ServiceIds.Count }
                });

            logger.LogInformation(
                "Service order {OrderId} created successfully for customer {CustomerId} with status {Status}",
                createdEntity.Id, createdEntity.ClientId, createdEntity.Status);

            return ResponseFactory.Ok(createdEntity, HttpStatusCode.Created);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating service order for customer {CustomerId}", request.ClientId);
            newRelicService.NoticeError(ex, new Dictionary<string, object>
            {
                { "customerId", request.ClientId.ToString() },
                { "operation", "CreateServiceOrder" }
            });
            throw;
        }
    }
}
