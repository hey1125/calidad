import { describe, expect, it } from 'vitest';
import { financialEntityApi as api } from '../../src/api/financialEntity.api';
import {
  testBank,
  unknownBankCode,
  unknownFinancialEntityId,
} from '../../src/fixtures/financialEntities';
import { expectStatus } from '../../src/helpers/assertions';

describe('GET /api/FinancialEntity/RetrieveByBankCode', () => {
  it('TC-FE-01 returns 200 and the bank with its data and commission', async () => {
    const res = await api.retrieveByBankCode(testBank.bankCode);
    expectStatus(res, 200);
    expect(res.data).toMatchObject({
      bankCode: testBank.bankCode,
      legalId: testBank.legalId,
      name: testBank.name,
      status: testBank.status,
      commissionRate: testBank.commissionRate,
    });
    expect(res.data.id).toBeGreaterThan(0);
  });

  it('TC-FE-02 returns 404 with a message when the bank code does not exist', async () => {
    const res = await api.retrieveByBankCode(unknownBankCode);
    expectStatus(res, 404);
    expect(res.data).toBe(
      `No se encontró la entidad financiera con código bancario '${unknownBankCode}'.`,
    );
  });
});

describe('GET /api/FinancialEntity/RetrieveById', () => {
  it('TC-FE-03 returns 200 and the same bank that the bank code lookup returns', async () => {
    const byCode = await api.retrieveByBankCode(testBank.bankCode);
    expectStatus(byCode, 200);

    const res = await api.retrieveById(byCode.data.id);
    expectStatus(res, 200);
    expect(res.data).toMatchObject({
      id: byCode.data.id,
      bankCode: testBank.bankCode,
      name: testBank.name,
    });
  });

  it('TC-FE-04 returns 404 with a message when the id does not exist', async () => {
    const res = await api.retrieveById(unknownFinancialEntityId);
    expectStatus(res, 404);
    expect(res.data).toBe(
      `No se encontró la entidad financiera con id '${unknownFinancialEntityId}'.`,
    );
  });

  it('TC-FE-05 returns 400 when the id is not a number', async () => {
    const res = await api.retrieveById('abc');
    expectStatus(res, 400);
  });
});
