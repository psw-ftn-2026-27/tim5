namespace Payment.Application.Reversing;

public sealed record TouristDepositsDto(string Username, int Balance, List<TouristDepositDto> Deposits);
