using AutoMapper;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.Quotes.Update;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.DTOs.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;

public sealed class UpdateServiceOrderStatusHandler(
    IMapper mapper,
    IMediator mediator,
    IServiceOrderRepository serviceOrderRepository,
    ILogger<UpdateServiceOrderStatusHandler> logger)
    : IRequestHandler<UpdateServiceOrderStatusCommand, Response<ServiceOrder>>, INotificationHandler<UpdateQuoteStatusNotification>
{
    public async Task<Response<ServiceOrder>> Handle(UpdateServiceOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            var entity = await serviceOrderRepository.GetByIdAsync(request.Id, cancellationToken);
            if (entity is null)
            {
                return ResponseFactory.Fail<ServiceOrder>("Service Order not found", HttpStatusCode.NotFound);
            }

            var previousStatus = entity.Status.ToString();
            _ = entity.ChangeStatus(request.Status);
            _ = await serviceOrderRepository.UpdateAsync(entity, cancellationToken);
            var response = (await serviceOrderRepository.GetDetailedAsync(request.Id, cancellationToken))!;

            logger.LogInformation(
                "Service order {OrderId} status updated from {PreviousStatus} to {NewStatus}",
                response.Id, previousStatus, response.Status);

            await mediator.Publish(new UpdateServiceOrderStatusNotification(request.Id, response), cancellationToken);
            return ResponseFactory.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating service order {OrderId} status to {Status}", request.Id, request.Status);
            throw;
        }
    }

    public Task Handle(UpdateQuoteStatusNotification notification, CancellationToken cancellationToken) =>
        mediator.Send(new UpdateServiceOrderStatusCommand(notification.Quote.ServiceOrderId, ServiceOrder.GetNextStatus(notification.Quote.Status)), cancellationToken);
}
