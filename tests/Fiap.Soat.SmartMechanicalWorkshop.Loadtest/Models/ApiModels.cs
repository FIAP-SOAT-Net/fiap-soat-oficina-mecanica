namespace Fiap.Soat.SmartMechanicalWorkshop.Loadtest.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse(bool IsSuccess, string Data);

public record PersonDto(Guid Id, string Fullname, string Document, string Email, string PersonType, string Phone, AddressDto Address, List<VehicleDto>? Vehicles);

public record AddressDto(string Street, string City, string State, string ZipCode);

public record VehicleDto(Guid Id, string LicensePlate, int ManufactureYear, string Brand, string Model, Guid PersonId);

public record CreateServiceOrderRequest(Guid ClientId, Guid VehicleId, List<Guid> ServiceIds, string Title, string Description);

public record PatchServiceOrderRequest(string Status);

public record ServiceOrderDto(Guid Id, string Status, Guid ClientId, Guid VehicleId, string Title, string Description, DateTime CreatedAt, DateTime UpdatedAt, List<QuoteDto>? Quotes);

public record QuoteDto(Guid Id, decimal Total, string Status, Guid ServiceOrderId);

public record ApiResponse<T>(bool IsSuccess, T Data);

public record PaginatedData<T>(List<T> Items, int PageNumber, int PageSize, int TotalCount, int TotalPages);

public record AvailableServiceDto(Guid Id, string Name, decimal Price);


