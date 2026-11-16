namespace Payment.Application.Shopping;

public sealed record OrderItemDto(Guid EntityId, string Name, int Price);
