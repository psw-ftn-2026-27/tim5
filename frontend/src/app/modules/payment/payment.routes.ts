import { Routes } from '@angular/router';
import { TouristWallets } from './depositing/tourist-wallets/tourist-wallets';
import { PaymentHome } from './payment-home/payment-home';
import { MyWallet } from './wallet-reading/my-wallet/my-wallet';

export const paymentRoutes: Routes = [
  { path: '', component: PaymentHome },
  { path: 'wallet', component: MyWallet },
  { path: 'tourists', component: TouristWallets },
];
