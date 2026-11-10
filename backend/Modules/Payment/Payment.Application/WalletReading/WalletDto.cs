namespace Payment.Application.WalletReading;

public sealed record WalletDto(int Balance, List<TransactionDto> Transactions);
