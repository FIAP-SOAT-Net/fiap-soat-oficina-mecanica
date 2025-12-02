using AutoFixture;
using AutoMapper;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Services;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.DTOs.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using Fiap.Soat.SmartMechanicalWorkshop.Tests.Shared.Factories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.Tests.UseCases.ServiceOrders;

public sealed class UpdateServiceOrderStatusHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IServiceOrderRepository> _repositoryMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly INewRelicInstrumentationService _newRelicService = new FakeNewRelicInstrumentationService();
    private readonly Mock<ILogger<UpdateServiceOrderStatusHandler>> _loggerMock = new();
    private readonly UpdateServiceOrderStatusHandler _service;

    public UpdateServiceOrderStatusHandlerTests()
    {
        _service = new UpdateServiceOrderStatusHandler(_mapperMock.Object, _mediatorMock.Object, _repositoryMock.Object, _newRelicService, _loggerMock.Object);
    }

    [Fact]
    public async Task PatchAsync_ShouldReturnNotFound_WhenNotFound()
    {
        // Arrange
        var input = _fixture.Create<UpdateServiceOrderStatusCommand>();
        _repositoryMock.Setup(r => r.GetByIdAsync(input.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceOrder?) null);

        // Act
        var result = await _service.Handle(input, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(x => x.Publish(It.IsAny<UpdateServiceOrderStatusNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }
}
