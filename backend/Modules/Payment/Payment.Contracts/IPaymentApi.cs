namespace Payment.Contracts;

public interface IPaymentApi
{
    Task ListProductAsync(Guid entityId, string name, Guid ownerId, int price);

    Task<IReadOnlyDictionary<Guid, int>> GetTourPricesAsync(IReadOnlyCollection<Guid> tourIds);

    Task<bool> HasPurchasedAsync(Guid touristId, Guid tourId);
}
