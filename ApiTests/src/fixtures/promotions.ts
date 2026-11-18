import { commerces } from './commerces';

// Seeded promotion in the local BilleTico DB (commerce 1)
export const seededPromo = {
  id: 1,
  coPromoId: 'JMETER-PROMO',
  commerceId: commerces.soda.id,
  minAmount: 500,
  amountPercentage: 10,
  amountDiscount: 1000,
  status: 'Activa',
} as const;

// Fixed reference dates, so results never depend on today's date
export const REF_DATE_IN_RANGE = '2026-11-01'; // inside the seeded promo's validity window
export const REF_DATE_EXPIRED = '2028-01-01'; // after it expires

// Data isolation (test files run in parallel):
export const lifecycleCommerceId = commerces.farmacia.id; // create/update/delete tests only touch commerce 2
export const noPromoCommerceId = commerces.superBarrio.id; // commerce 3 never gets promotions

export const unknownPromoId = 999999;