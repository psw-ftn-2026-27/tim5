export type TransactionType = 'Deposit';

export interface TransactionDto {
  type: TransactionType;
  amount: number;
  occurredAt: string;
  isReversed: boolean;
  reversedAt: string | null;
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

export interface TouristDepositDto {
  id: string;
  amount: number;
  occurredAt: string;
  isReversed: boolean;
  reversedAt: string | null;
}

export interface TouristDepositsDto {
  username: string;
  balance: number;
  deposits: TouristDepositDto[];
}
