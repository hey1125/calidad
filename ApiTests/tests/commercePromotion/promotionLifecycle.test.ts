import { afterAll, describe, expect, it } from 'vitest';
import { commercePromotionApi as api } from '../../src/api/commercePromotion.api';
import { buildPromotion } from '../../src/helpers/promotionBuilder';
import { expectStatus } from '../../src/helpers/assertions';

// Every promotion created here is tracked and deleted at the end, even if a test fails
const toCleanup: number[] = [];

async function createTracked(body: Record<string, unknown>) {
  const res = await api.create(body);
  if (res.status === 200 && typeof res.data === 'object' && res.data.id > 0) {
    toCleanup.push(res.data.id);
  }
  return res;
}

afterAll(async () => {
  for (const id of toCleanup) await api.remove(id);
});

describe('POST /api/CommercePromotion/Create: validation', () => {
  it('TC-PROMO-13 returns 400 when EndDate is before StartDate', async () => {
    const res = await createTracked(
      buildPromotion({ startDate: '2026-12-31T00:00:00Z', endDate: '2026-01-01T00:00:00Z' }),
    );
    expectStatus(res, 400);
    expect(res.data).toContain('EndDate debe ser mayor o igual que StartDate.');
  });

  it('TC-PROMO-14 returns 400 when the status is not Activa or Inactiva', async () => {
    const res = await createTracked(buildPromotion({ status: 'Foo' }));
    expectStatus(res, 400);
    expect(res.data).toContain("Status debe ser 'Activa' o 'Inactiva'.");
  });

  it('TC-PROMO-15 returns 400 when CommerceId is 0', async () => {
    const res = await createTracked(buildPromotion({ commerceId: 0 }));
    expectStatus(res, 400);
    expect(res.data).toContain('CommerceId inválido.');
  });
});

describe('CommercePromotion create / update / delete lifecycle (commerce 2)', () => {
  const original = buildPromotion();
  let createdId = 0;

  it('TC-PROMO-12 creates a promotion and it can be read back', async () => {
    const res = await createTracked(original);
    expectStatus(res, 200);
    expect(res.data.id).toBeGreaterThan(0);
    createdId = res.data.id;

    const saved = await api.retrieveById(createdId);
    expectStatus(saved, 200);
    expect(saved.data).toMatchObject({
      id: createdId,
      coPromoId: original.coPromoId,
      commerceId: original.commerceId,
      status: 'Activa',
    });
  });

  it('TC-PROMO-16 updates the promotion and the change is persisted', async () => {
    expect(createdId, 'TC-PROMO-12 must pass first').toBeGreaterThan(0);
    const res = await api.update(createdId, {
      ...original,
      description: 'Updated by ApiTests',
      amountPercentage: 15,
    });
    expectStatus(res, 200);

    const saved = await api.retrieveById(createdId);
    expectStatus(saved, 200);
    expect(saved.data).toMatchObject({ description: 'Updated by ApiTests', amountPercentage: 15 });
  });

  it('TC-PROMO-17 deletes the promotion and it can no longer be found', async () => {
    expect(createdId, 'TC-PROMO-12 must pass first').toBeGreaterThan(0);
    const res = await api.remove(createdId);
    expectStatus(res, 200);
    expect(res.data).toBe(true);

    const gone = await api.retrieveById(createdId);
    expectStatus(gone, 404);
  });
});

describe('DELETE /api/CommercePromotion/Delete/{id}', () => {
  it('TC-PROMO-18 returns 400 when the id is 0', async () => {
    const res = await api.remove(0);
    expectStatus(res, 400);
    expect(res.data).toBe('Id inválido.');
  });
});