import { lifecycleCommerceId } from '../fixtures/promotions';

const DAY_MS = 24 * 60 * 60 * 1000;

// Builds a valid promotion payload (commerce 2) with a unique code; overrides let a test break one field
export function buildPromotion(overrides: Record<string, unknown> = {}): Record<string, unknown> {
  const now = Date.now();
  return {
    coPromoId: `TST-${now}`, // the DB column holds at most 20 characters
    commerceId: lifecycleCommerceId,
    startDate: new Date(now - DAY_MS).toISOString(),
    endDate: new Date(now + 30 * DAY_MS).toISOString(),
    minAmount: 100,
    amountPercentage: 5,
    amountDiscount: null,
    totalCoupon: 50,
    usedCoupon: 0,
    status: 'Activa',
    description: 'ApiTests temporary promotion',
    ...overrides,
  };
}