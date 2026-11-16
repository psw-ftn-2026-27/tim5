namespace Payment.Application.Shopping;

public sealed record ShoppingCartDto(List<OrderItemDto> Items, int Total);
