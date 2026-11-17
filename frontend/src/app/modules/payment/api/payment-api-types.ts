export type TransactionType = 'Deposit' | 'Purchase';

export interface TransactionDto {
  type: TransactionType;
  name: string | null;
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

export interface ProductDto {
  name: string;
  price: number;
}

export interface OrderItemDto {
  entityId: string;
  name: string;
  price: number;
}

export interface ShoppingCartDto {
  items: OrderItemDto[];
  total: number;
}

export interface AddToCartDto {
  entityId: string;
}

export interface PurchasedTourDto {
  entityId: string;
  name: string;
  purchasedAt: string;
}

export interface MyProductDto {
  productId: string;
  name: string;
}

export interface CreateCouponDto {
  discountPercentage: number;
  expiresAt: string;
  productId: string | null;
}

export interface CreatedCouponDto {
  id: string;
}

export interface CouponDto {
  id: string;
  code: string;
  discountPercentage: number;
  expiresAt: string;
  productId: string | null;
}
