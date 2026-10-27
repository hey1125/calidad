import { afterAll, beforeAll, describe, expect, it } from 'vitest';
import type { AxiosResponse } from 'axios';
import { financialEntityApi } from '../../src/api/financialEntity.api';
import { financialEntityPromotionApi as api } from '../../src/api/financialEntityPromotion.api';
import {
  FE_PROMO_TEST_DESCRIPTION,
  testBank,
  testBankIban,
  unknownBankIban,
} from '../../src/fixtures/financialEntities';
import { buildFePromotion } from '../../src/helpers/fePromotionBuilder';
import { expectStatus } from '../../src/helpers/assertions';
import type { FinancialEntityPromotion } from '../../src/types/financialEntity';

// Business rules of bank promotions (FinancialEntityPromotionManager + RET_ACTIVE_FE_PROMOTIONS_BY_IBAN_PR):
// - the bank is resolved from the IBAN (characters 6 to 8)
// - only "Activa" promotions with coupons left and minAmount <= amount apply
// - the best one gives the largest discount: max(amount * percentage, fixed discount)
//
// Every promotion is created through the API on the test bank 987 and deleted at the end.

const NO_PROMOS = 'No hay promociones vigentes para el IBAN especificado.';

let entityId = 0;
const toCleanup: number[] = [];
const created: Record<string, AxiosResponse<FinancialEntityPromotion>> = {};
const sent: Record<string, Record<string, unknown>> = {};

async function createTracked(body: Record<string, unknown>) {
  const res = await api.create(body);
  if (res.status === 201 && typeof res.data === 'object' && res.data.id > 0) {
    toCleanup.push(res.data.id);
  }
  return res;
}

const code = (key: string) => sent[key].fePromoId;

beforeAll(async () => {
  const bank = await financialEntityApi.retrieveByBankCode(testBank.bankCode);
  if (bank.status !== 200) {
    throw new Error('Test bank 987 not found: run ApiTests/sql/seed_financial_entity.sql once.');
  }
  entityId = bank.data.id;

  // Remove leftovers of an interrupted run, so they cannot win the "best promotion" checks
  const existing = await api.retrieveAllByEntity(entityId);
  if (existing.status === 200) {
    for (const p of existing.data) {
      if (p.description === FE_PROMO_TEST_DESCRIPTION) await api.remove(p.id);
    }
  }

  sent.pct10 = buildFePromotion(entityId, { amountPercentage: 10, minAmount: 1000 });
  sent.fixed1500 = buildFePromotion(entityId, { amountDiscount: 1500, minAmount: 5000 });
  sent.inactive = buildFePromotion(entityId, { amountPercentage: 50, status: 'Inactiva' });
  sent.noCoupons = buildFePromotion(entityId, { amountPercentage: 90, totalCoupon: 5, usedCoupon: 5 });

  for (const key of Object.keys(sent)) created[key] = await createTracked(sent[key]);
});

afterAll(async () => {
  for (const id of toCleanup) await api.remove(id);
});

describe('POST /api/FinancialEntityPromotion/Create', () => {
  it('TC-FEP-01 returns 201 and the created promotion for each valid body', () => {
    for (const key of Object.keys(sent)) {
      const res = created[key];
      expectStatus(res, 201);
      expect(res.data.id).toBeGreaterThan(0);
      expect(res.data).toMatchObject({
        fePromoId: sent[key].fePromoId,
        financialEntityId: entityId,
        status: sent[key].status,
      });
    }
  });

  it('TC-FEP-12 returns 400 when UsedCoupon is greater than TotalCoupon', async () => {
    const res = await createTracked(buildFePromotion(entityId, { totalCoupon: 5, usedCoupon: 6 }));
    expectStatus(res, 400);
    expect(res.data).toContain('UsedCoupon no puede ser mayor que TotalCoupon.');
  });

  it('TC-FEP-13 returns 400 when the fixed discount is negative', async () => {
    const res = await createTracked(buildFePromotion(entityId, { amountDiscount: -100 }));
    expectStatus(res, 400);
    expect(res.data).toContain('AmountDiscount no puede ser negativo.');
  });
});

describe('GET /api/FinancialEntityPromotion/RetrieveBestByIBAN: best discount', () => {
  it('TC-FEP-02 amount 2000: only the 10% promotion reaches its minimum, so it is the best', async () => {
    const res = await api.retrieveBestByIban(testBankIban, 2000);
    expectStatus(res, 200);
    expect(res.data.fePromoId).toBe(code('pct10'));
    expect(res.data.financialEntityId).toBe(entityId);
  });

  it('TC-FEP-03 amount 8000: the fixed 1500 beats 10% (800), so the fixed one wins', async () => {
    const res = await api.retrieveBestByIban(testBankIban, 8000);
    expectStatus(res, 200);
    expect(res.data.fePromoId).toBe(code('fixed1500'));
  });

  it('TC-FEP-04 amount 20000: 10% (2000) beats the fixed 1500, so the percentage wins', async () => {
    const res = await api.retrieveBestByIban(testBankIban, 20000);
    expectStatus(res, 200);
    expect(res.data.fePromoId).toBe(code('pct10'));
  });

  it('TC-FEP-07 returns 404 when the amount is below every minimum', async () => {
    const res = await api.retrieveBestByIban(testBankIban, 500);
    expectStatus(res, 404);
    expect(res.data).toBe(NO_PROMOS);
  });

  it('TC-FEP-08 returns 404 when the IBAN belongs to a bank that does not exist', async () => {
    const res = await api.retrieveBestByIban(unknownBankIban, 8000);
    expectStatus(res, 404);
    expect(res.data).toBe(NO_PROMOS);
  });

  it.each([
    ['TC-FEP-09', 0],
    ['TC-FEP-10', -100],
  ])('%s returns 400 when the amount is %d', async (_id, amount) => {
    const res = await api.retrieveBestByIban(testBankIban, amount);
    expectStatus(res, 400);
    expect(res.data).toBe('El monto debe ser mayor a 0.');
  });

  // ASP.NET model validation rejects the empty IBAN before the controller runs
  it('TC-FEP-11 returns 400 with a validation error on iban when the IBAN is empty', async () => {
    const res = await api.retrieveBestByIban('', 8000);
    expectStatus(res, 400);
    expect(res.data).toMatchObject({ errors: { iban: expect.any(Array) } });
  });
});

describe('GET /api/FinancialEntityPromotion/RetrieveActiveByIBAN: which promotions apply', () => {
  it('TC-FEP-05 amount 8000: returns both active ones, never the inactive or the one without coupons', async () => {
    const res = await api.retrieveActiveByIban(testBankIban, 8000);
    expectStatus(res, 200);
    const codes = res.data.map((p) => p.fePromoId);
    expect(codes).toEqual(expect.arrayContaining([code('pct10'), code('fixed1500')]));
    expect(codes).not.toContain(code('inactive'));
    expect(codes).not.toContain(code('noCoupons'));
    for (const p of res.data) expect(p.status).toBe('Activa');
  });

  it('TC-FEP-06 amount 2000: leaves out the promotion whose minimum is 5000', async () => {
    const res = await api.retrieveActiveByIban(testBankIban, 2000);
    expectStatus(res, 200);
    const codes = res.data.map((p) => p.fePromoId);
    expect(codes).toContain(code('pct10'));
    expect(codes).not.toContain(code('fixed1500'));
  });
});
