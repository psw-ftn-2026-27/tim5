export type TransactionType = 'Deposit';

export interface TransactionDto {
  type: TransactionType;
  amount: number;
  occurredAt: string;
}

export interface WalletDto {
  balance: number;
  transactions: TransactionDto[];
}
