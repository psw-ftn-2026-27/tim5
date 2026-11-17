using Payment.Domain.Purchases;

namespace Payment.Application.Purchases;

public interface IPurchaseRepository
{
    void Add(Purchase purchase);
}
