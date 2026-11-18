export interface Payment {
  id: number;
  paymentId: string;
  nationalId: string;
  commerceId: number;
  iban: string;
  coPromoId: string | null;
  fePromoId: string | null;
  paymentDate: string;
  grossAmount: number;
  amountWDiscount: number | null;
  coCommisionAmount: number | null;
  feCommisionAmount: number | null;
  netAmount: number;
  commerceProfit: number;
  status: string;
  description: string | null;
  commerceName?: string | null;
  payerName?: string | null;
}

export interface BankStats {
  totalPayments: number;
  totalAmount: number;
  totalCommissions: number;
  pendingPayments: number;
  completedPayments: number;
}