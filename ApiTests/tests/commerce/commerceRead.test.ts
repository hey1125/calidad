import { describe, it, expect } from 'vitest';
import { commerceApi } from '../../src/api/commerce.api';
import { commerces, unknownCommerceId, unknownLegalId } from '../../src/fixtures/commerces';
import { expectStatus } from '../../src/helpers/assertions';

describe('GET /api/Commerce/RetrieveActive', () => {
  it('TC-COM-01 returns 200 and only active commerces, including the seeded ones', async () => {
    const res = await commerceApi.retrieveActive();
    expectStatus(res, 200);
    expect(res.data.length).toBeGreaterThanOrEqual(3);
    for (const c of res.data) expect(c.status).toBe('Activo');
    expect(res.data.map((c) => c.id)).toEqual(expect.arrayContaining([1, 2, 3]));
  });

  it('TC-COM-02 returns commerces with the expected fields and types', async () => {
    const res = await commerceApi.retrieveActive();
    expectStatus(res, 200);
    expect(res.data[0]).toEqual(
      expect.objectContaining({
        id: expect.any(Number),
        legalId: expect.any(String),
        name: expect.any(String),
        iban: expect.any(String),
        commissionRate: expect.any(Number),
      }),
    );
  });
});

describe('GET /api/Commerce/RetrieveById', () => {
  it('TC-COM-03 returns 200 and the commerce for an existing id', async () => {
    const res = await commerceApi.retrieveById(commerces.soda.id);
    expectStatus(res, 200);
    expect(res.data).toMatchObject(commerces.soda);
  });

  it('TC-COM-04 returns 404 when the id does not exist', async () => {
    const res = await commerceApi.retrieveById(unknownCommerceId);
    expectStatus(res, 404);
    expect(res.data).toContain('No se encontró el comercio');
  });

  it('TC-COM-05 returns 400 when the id is not a number', async () => {
    const res = await commerceApi.retrieveById('abc');
    expectStatus(res, 400);
  });
});

describe('GET /api/Commerce/RetrieveByLegalId', () => {
  it('TC-COM-06 returns 200 and the commerce for an existing legal id', async () => {
    const res = await commerceApi.retrieveByLegalId(commerces.farmacia.legalId);
    expectStatus(res, 200);
    expect(res.data).toMatchObject(commerces.farmacia);
  });

  it('TC-COM-07 returns 404 when the legal id does not exist', async () => {
    const res = await commerceApi.retrieveByLegalId(unknownLegalId);
    expectStatus(res, 404);
    expect(res.data).toContain('cédula jurídica');
  });
});

describe('GET /api/Commerce/RetrievePending', () => {
  it('TC-COM-08 returns 200 and a list containing only pending commerces', async () => {
    const res = await commerceApi.retrievePending();
    expectStatus(res, 200);
    expect(Array.isArray(res.data)).toBe(true);
    for (const c of res.data) expect(c.status).toBe('Pendiente');
  });
});