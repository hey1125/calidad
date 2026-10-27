export interface CommercePromotion {
  id: number;
  created: string;
  updated: string | null;
  coPromoId: string;
  commerceId: number;
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