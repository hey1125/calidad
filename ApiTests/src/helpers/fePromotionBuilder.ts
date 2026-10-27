import { FE_PROMO_TEST_DESCRIPTION } from '../fixtures/financialEntities';

const DAY_MS = 24 * 60 * 60 * 1000;
let counter = 0;

// Builds a valid, currently active bank promotion with a unique code (the column holds 20 characters).
// Overrides let each test set only the rule it is about.
export function buildFePromotion(
  financialEntityId: number,
  overrides: Record<string, unknown> = {},
): Record<string, unknown> {
  const now = Date.now();
  counter += 1;
  return {
    fePromoId: `FEP${counter}-${now}`,
    financialEntityId,
    startDate: new Date(now - DAY_MS).toISOString(),
    endDate: new Date(now + 30 * DAY_MS).toISOString(),
    minAmount: null,
    amountPercentage: null,
    amountDiscount: null,
    totalCoupon: null,
    usedCoupon: null,
    status: 'Activa',
    description: FE_PROMO_TEST_DESCRIPTION,
    ...overrides,
  };
}
