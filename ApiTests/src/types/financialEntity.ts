export interface FinancialEntity {
  id: number;
  created: string;
  updated: string;
  legalId: string;
  bankCode: string;
  name: string;
  phone: string;
  email: string;
  latitude: number;
  longitude: number;
  status: string;
  commissionRate: number;
  nationalId: string;
}

export interface FinancialEntityPromotion {
  id: number;
  created: string;
  updated: string | null;
  fePromoId: string;
  financialEntityId: number;
  startDate: string;
  endDate: string;
  minAmount: number | null;
  amountPercentage: number | null;
  amountDiscount: number | null;
  totalCoupon: number | null;
  usedCoupon: number | null;
  status: string;
  description: string | null;
}
