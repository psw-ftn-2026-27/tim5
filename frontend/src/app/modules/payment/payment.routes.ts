import { Routes } from '@angular/router';
import { TouristWallets } from './depositing/tourist-wallets/tourist-wallets';
import { PaymentHome } from './payment-home/payment-home';
import { PurchasedTours } from './purchasing/purchased-tours/purchased-tours';
import { TouristDeposits } from './reversing/tourist-deposits/tourist-deposits';
import { ProductDetails } from './shopping/product-details/product-details';
import { ShoppingCart } from './shopping/shopping-cart/shopping-cart';
import { MyWallet } from './wallet-reading/my-wallet/my-wallet';
import { CouponCreation } from './coupon-creating/coupon-creation/coupon-creation';
import { GiftCards } from './gift-card-purchasing/gift-cards/gift-cards';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: MyWallet },
  { path: 'tourists', component: TouristWallets },
  { path: 'tourists/:touristId/deposits', component: TouristDeposits },
  { path: 'products/:entityId', component: ProductDetails },
  { path: 'cart', component: ShoppingCart },
  { path: 'purchases', component: PurchasedTours },
  { path: 'coupons', component: CouponCreation },
  { path: 'gift-cards', component: GiftCards },
];
