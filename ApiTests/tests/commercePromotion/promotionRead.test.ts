import { describe, it, expect } from 'vitest';
import { commercePromotionApi as api } from '../../src/api/commercePromotion.api';
import {
  noPromoCommerceId,
  REF_DATE_EXPIRED,
  REF_DATE_IN_RANGE,
  seededPromo,
  unknownPromoId,
} from '../../src/fixtures/promotions';
import { expectStatus } from '../../src/helpers/assertions';

describe('GET /api/CommercePromotion/RetrieveBestByCommerce', () => {
  it('TC-PROMO-01 returns 200 and the seeded promotion for a qualifying amount', async () => {
    const res = await api.retrieveBestByCommerce(seededPromo.commerceId, 1000, REF_DATE_IN_RANGE);
    expectStatus(res, 200);
    expect(res.data).toMatchObject({
      coPromoId: seededPromo.coPromoId,
      commerceId: seededPromo.commerceId,
      status: 'Activa',
    });
  });

  it('TC-PROMO-02 returns 404 when the amount is below the promotion minimum', async () => {
    const res = await api.retrieveBestByCommerce(seededPromo.commerceId, 100, REF_DATE_IN_RANGE);
    expectStatus(res, 404);
    expect(res.data).toBe('No hay promociones vigentes para el comercio especificado.');
  });

  it('TC-PROMO-03 returns 400 when commerceId is 0', async () => {
    const res = await api.retrieveBestByCommerce(0, 1000);
    expectStatus(res, 400);
    expect(res.data).toBe('CommerceId inválido.');
  });

  it('TC-PROMO-04 returns 400 when the amount is 0', async () => {
    const res = await api.retrieveBestByCommerce(seededPromo.commerceId, 0);
    expectStatus(res, 400);
    expect(res.data).toBe('El monto debe ser mayor a 0.');
  });

  it('TC-PROMO-05 returns 404 for a commerce that has no promotions', async () => {
    const res = await api.retrieveBestByCommerce(noPromoCommerceId, 1000, REF_DATE_IN_RANGE);
    expectStatus(res, 404);
  });

  it('TC-PROMO-19 returns 404 when the reference date is after the promotion expires', async () => {
    const res = await api.retrieveBestByCommerce(seededPromo.commerceId, 1000, REF_DATE_EXPIRED);
    expectStatus(res, 404);
  });
});

describe('GET /api/CommercePromotion/RetrieveActiveByCommerce', () => {
  it('TC-PROMO-06 returns 200 and only active promotions of that commerce', async () => {
    const res = await api.retrieveActiveByCommerce(seededPromo.commerceId, 1000, REF_DATE_IN_RANGE);
    expectStatus(res, 200);
    expect(res.data.map((p) => p.coPromoId)).toContain(seededPromo.coPromoId);
    for (const p of res.data) {
      expect(p.commerceId).toBe(seededPromo.commerceId);
      expect(p.status).toBe('Activa');
    }
  });
});

describe('GET /api/CommercePromotion/RetrieveAllByCommerce', () => {
  it('TC-PROMO-07 returns 200 and includes the seeded promotion', async () => {
    const res = await api.retrieveAllByCommerce(seededPromo.commerceId);
    expectStatus(res, 200);
    expect(res.data.map((p) => p.coPromoId)).toContain(seededPromo.coPromoId);
  });

  it('TC-PROMO-08 returns 400 when commerceId is negative', async () => {
    const res = await api.retrieveAllByCommerce(-1);
    expectStatus(res, 400);
    expect(res.data).toBe('commerceId inválido.');
  });
});

describe('GET /api/CommercePromotion/RetrieveById/{id}', () => {
  it('TC-PROMO-09 returns 200 and the promotion for an existing id', async () => {
    const res = await api.retrieveById(seededPromo.id);
    expectStatus(res, 200);
    expect(res.data).toMatchObject({
      id: seededPromo.id,
      coPromoId: seededPromo.coPromoId,
      commerceId: seededPromo.commerceId,
      minAmount: seededPromo.minAmount,
      amountPercentage: seededPromo.amountPercentage,
      amountDiscount: seededPromo.amountDiscount,
      status: seededPromo.status,
    });
  });

  it('TC-PROMO-10 returns 404 when the id does not exist', async () => {
    const res = await api.retrieveById(unknownPromoId);
    expectStatus(res, 404);
    expect(res.data).toBe('Promoción no encontrada.');
  });

  it('TC-PROMO-11 returns 400 when the id is 0', async () => {
    const res = await api.retrieveById(0);
    expectStatus(res, 400);
    expect(res.data).toBe('Id inválido.');
  });
});