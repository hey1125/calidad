import { describe, expect, it } from 'vitest';
import { financialEntityApi } from '../../src/api/financialEntity.api';
import { expectStatus } from '../../src/helpers/assertions';

describe('Known defects: financial entities', () => {
  // The validation fails before anything is written, so this test never creates data
  it('DEF-06 registering a bank with an invalid email should return 400, not 500', async () => {
    const res = await financialEntityApi.create({
      legalId: '3101000998',
      bankCode: '998',
      name: 'Banco Invalido ApiTests',
      phone: '22220998',
      email: 'no-es-un-correo',
      latitude: 9.9325,
      longitude: -84.0796,
      nationalId: '111111111',
    });
    expectStatus(res, 400);
    expect(res.data).toContain('Ingrese un correo electrónico válido');
  });
});
