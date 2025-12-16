using AutoFixture;
using AutoMapper;
using Fiap.Soat.SmartMechanicalWorkshop.Application.Adapters.Gateways.Repositories;
using Fiap.Soat.SmartMechanicalWorkshop.Application.UseCases.ServiceOrders.Create;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.DTOs.ServiceOrders;
using Fiap.Soat.SmartMechanicalWorkshop.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace Fiap.Soat.SmartMechanicalWorkshop.Application.Tests.UseCases.ServiceOrders;

public sealed class CreateServiceOrderHandlerTests
{
    private readonly Mock<IAvailableServiceRepository> _availableServiceRepositoryMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPersonRepository> _personRepositoryMock = new();
    private readonly Mock<IServiceOrderRepository> _repositoryMock = new();
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
    private readonly Mock<ILogger<CreateServiceOrderHandler>> _loggerMock = new();
    private readonly CreateServiceOrderHandler _useCase;

    public CreateServiceOrderHandlerTests()
    {
        _useCase = new CreateServiceOrderHandler(
            _mapperMock.Object,
            _repositoryMock.Object,
            _personRepositoryMock.Object,
            _availableServiceRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = _fixture.Create<CreateServiceOrderCommand>();
        var entity = _fixture.Create<ServiceOrder>();
        _mapperMock.Setup(m => m.Map<ServiceOrder>(command)).Returns(entity);
        _personRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenVehicleDoesNotExist()
    {
        // Arrange
        var command = _fixture.Create<CreateServiceOrderCommand>();
        var entity = _fixture.Create<ServiceOrder>();
        _mapperMock.Setup(m => m.Map<ServiceOrder>(command)).Returns(entity);
        _personRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _vehicleRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Vehicle, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNotFound_WhenAnyServiceNotFound()
    {
        // Arrange
        var command = _fixture.Build<CreateServiceOrderCommand>()
            .With(x => x.ServiceIds, [Guid.NewGuid()])
            .Create();
        var entity = _fixture.Create<ServiceOrder>();
        _mapperMock.Setup(m => m.Map<ServiceOrder>(command)).Returns(entity);
        _personRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _vehicleRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Vehicle, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _availableServiceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((AvailableService?) null);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var command = _fixture.Build<CreateServiceOrderCommand>()
            .With(x => x.ServiceIds, [Guid.NewGuid()])
            .Create();
        var entity = _fixture.Create<ServiceOrder>();
        var availableService = _fixture.Create<AvailableService>();
        var createdEntity = _fixture.Create<ServiceOrder>();

        _mapperMock.Setup(m => m.Map<ServiceOrder>(command)).Returns(entity);
        _personRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Person, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _vehicleRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Vehicle, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _availableServiceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(availableService);
        _repositoryMock.Setup(r => r.AddAsync(entity, It.IsAny<CancellationToken>())).ReturnsAsync(createdEntity);

        // Act
        var result = await _useCase.Handle(command, CancellationToken.None);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Data.Should().Be(createdEntity);
    }
}
