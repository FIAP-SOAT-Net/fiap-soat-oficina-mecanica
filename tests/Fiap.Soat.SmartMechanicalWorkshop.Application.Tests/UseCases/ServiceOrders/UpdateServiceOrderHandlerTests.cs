using AutoFixture;
using AutoMapper;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Update;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.DTOs.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.Tests.UseCases.ServiceOrders;

public sealed class UpdateServiceOrderHandlerTests
{
    private readonly Mock<IAvailableServiceRepository> _availableServiceRepositoryMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IServiceOrderRepository> _repositoryMock = new();
    private readonly Mock<ILogger<UpdateServiceOrderHandler>> _loggerMock = new();
    private readonly UpdateServiceOrderHandler _useCase;

    public UpdateServiceOrderHandlerTests()
    {
        _useCase = new UpdateServiceOrderHandler(_repositoryMock.Object, _availableServiceRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenNotFound()
    {
        // Arrange
        var command = _fixture.Create<UpdateServiceOrderCommand>();
        _repositoryMock.Setup(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync((ServiceOrder?) null);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenAvailableServiceNotFound()
    {
        // Arrange
        var command = _fixture.Build<UpdateServiceOrderCommand>()
            .With(x => x.ServiceIds, [Guid.NewGuid()])
            .Create();
        var entity = _fixture.Create<ServiceOrder>();
        _repositoryMock.Setup(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _availableServiceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AvailableService?) null);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdated_WhenValid()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = _fixture.Build<UpdateServiceOrderCommand>()
            .With(x => x.ServiceIds, [serviceId])
            .Create();
        var entity = _fixture.Create<ServiceOrder>();
        var availableService = _fixture.Create<AvailableService>();
        var updatedEntity = _fixture.Create<ServiceOrder>();

        _repositoryMock.Setup(r => r.GetAsync(command.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _availableServiceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId, It.IsAny<CancellationToken>())).ReturnsAsync(availableService);
        _repositoryMock.Setup(r =>
                r.UpdateAsync(command.Id, command.Title, command.Description, It.IsAny<IReadOnlyList<AvailableService>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEntity);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(updatedEntity);
    }
}
