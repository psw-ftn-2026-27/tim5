export type TransactionType = 'Deposit' | 'Purchase' | 'GiftCardPurchase';

export type GiftCardStatus = 'Valid' | 'Redeemed' | 'Expired';

export interface TransactionDto {
  type: TransactionType;
  name: string | null;
  amount: number;
  occurredAt: string;
  isReversed: boolean;
  reversedAt: string | null;
  giftCardId: string | null;
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

export interface PurchaseGiftCardDto {
  recipientUsername: string;
  amount: number;
  message: string;
}

export interface GiftCardDto {
  id: string;
  otherPartyUsername: string;
  amount: number;
  message: string;
  createdAt: string;
  expiresAt: string;
  status: GiftCardStatus;
}
