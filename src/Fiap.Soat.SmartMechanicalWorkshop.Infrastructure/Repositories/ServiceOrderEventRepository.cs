using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.DTOs.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.ValueObjects;
using Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Fiap.Soat.SmartMechanicalWorkshop.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ServiceOrderEventRepository(AppDbContext appDbContext) : Repository<ServiceOrderEvent>(appDbContext), IServiceOrderEventRepository
{
    public async Task<ServiceOrderExecutionTimeReportDto> GetAverageExecutionTimesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = Query()
            .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate);

        var grouped = await query
            .GroupBy(e => e.ServiceOrderId)
            .Select(g => new
            {
                ServiceOrderId = g.Key,
                HasCancelled = g.Any(e => e.Status == ServiceOrderStatus.Cancelled),
                ReceivedAt = g.Where(e => e.Status == ServiceOrderStatus.Received).OrderBy(e => e.CreatedAt).Select(e => (DateTime?) e.CreatedAt).FirstOrDefault(),
                InProgressAt = g.Where(e => e.Status == ServiceOrderStatus.InProgress).OrderBy(e => e.CreatedAt).Select(e => (DateTime?) e.CreatedAt).FirstOrDefault(),
                CompletedAt = g.Where(e => e.Status == ServiceOrderStatus.Completed).OrderBy(e => e.CreatedAt).Select(e => (DateTime?) e.CreatedAt).FirstOrDefault(),
                DeliveredAt = g.Where(e => e.Status == ServiceOrderStatus.Delivered).OrderBy(e => e.CreatedAt).Select(e => (DateTime?) e.CreatedAt).FirstOrDefault()
            })
            .Where(x =>
                !x.HasCancelled &&
                x.ReceivedAt != null &&
                x.InProgressAt != null &&
                x.CompletedAt != null &&
                x.DeliveredAt != null
            )
            .ToListAsync(cancellationToken);

        int totalCount = grouped.Count;
        if (totalCount == 0) return new ServiceOrderExecutionTimeReportDto(0, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero);

        double avgTotal = grouped.Average(x => (x.DeliveredAt - x.ReceivedAt)?.TotalSeconds ?? 0);
        double avgAttendance = grouped.Average(x => (x.InProgressAt - x.ReceivedAt)?.TotalSeconds ?? 0);
        double avgExecution = grouped.Average(x => (x.CompletedAt - x.InProgressAt)?.TotalSeconds ?? 0);
        double avgDelivery = grouped.Average(x => (x.DeliveredAt - x.CompletedAt)?.TotalSeconds ?? 0);

        return new ServiceOrderExecutionTimeReportDto(
            totalCount,
            TimeSpan.FromSeconds(avgTotal),
            TimeSpan.FromSeconds(avgAttendance),
            TimeSpan.FromSeconds(avgExecution),
            TimeSpan.FromSeconds(avgDelivery));
    }
}

