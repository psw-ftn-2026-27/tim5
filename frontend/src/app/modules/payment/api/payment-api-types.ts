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

export interface TouristBalanceDto {
  touristId: string;
  username: string;
  balance: number;
}

export interface DepositDto {
  amount: number;
}
