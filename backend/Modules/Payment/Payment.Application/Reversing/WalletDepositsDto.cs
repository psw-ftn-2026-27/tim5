namespace Payment.Application.Reversing;

public sealed record WalletDepositsDto(int Balance, List<TouristDepositDto> Deposits);
