import { describe, it, expect } from 'vitest';
import { paymentApi as api } from '../../src/api/payment.api';
import { commerces } from '../../src/fixtures/commerces';
import {
  RANGE_JUL_SEP_2026,
  SEEDED_PAYMENTS_PER_USER,
  SEEDED_PAYMENTS_WITH_IBAN,
  seededBankCode,
  seededPayment,
  unknownPaymentId,
} from '../../src/fixtures/payments';
import { unknownNationalId, users } from '../../src/fixtures/users';
import { expectStatus } from '../../src/helpers/assertions';

const payer = users.active.nationalId;

describe('GET /api/Payment/GetUserHistoryPayment', () => {
  it('TC-PAY-01 returns 200 and all payments of the payer, with the commerce name', async () => {
  const res = await api.getUserHistoryPayment(payer);
  expectStatus(res, 200);
  expect(res.data).toHaveLength(SEEDED_PAYMENTS_PER_USER);
  for (const p of res.data) {
      expect(p.nationalId).toBe(payer);
      expect(p.commerceName).toBe(commerces.soda.name);
  }
  });

  it('TC-PAY-02 returns 200 and an empty list when the date range has no payments', async () => {
  const res = await api.getUserHistoryPayment(payer, '2020-01-01', '2020-12-31');
  expectStatus(res, 200);
  expect(res.data).toEqual([]);
  });

  it('TC-PAY-03 filters by date range, newest first', async () => {
  const { start, end } = RANGE_JUL_SEP_2026;
  const res = await api.getUserHistoryPayment(payer, start, end);
  expectStatus(res, 200);
  expect(res.data.map((p) => p.paymentId)).toEqual([`JMETER${payer}01`, `JMETER${payer}02`]);
  for (const p of res.data) {
      const day = p.paymentDate.slice(0, 10);
      expect(day >= start && day <= end).toBe(true);
  }
  });

  it('TC-PAY-04 returns 200 and an empty list for an unknown nationalId', async () => {
  const res = await api.getUserHistoryPayment(unknownNationalId);
  expectStatus(res, 200);
  expect(res.data).toEqual([]);
  });

  it('TC-PAY-05 returns 400 when the nationalId is empty', async () => {
  const res = await api.getUserHistoryPayment('');
  expectStatus(res, 400);
  });
});

describe('GET /api/Payment/GetById/{id}', () => {
  it('TC-PAY-06 returns 200 and the payment for an existing id', async () => {
  const res = await api.getById(seededPayment.id);
  expectStatus(res, 200);
  expect(res.data).toMatchObject({
      id: seededPayment.id,
      paymentId: seededPayment.paymentId,
      nationalId: seededPayment.nationalId,
      commerceId: seededPayment.commerceId,
      status: seededPayment.status,
      grossAmount: seededPayment.grossAmount,
  });
  });

  it('TC-PAY-07 returns 404 when the id does not exist', async () => {
  const res = await api.getById(unknownPaymentId);
  expectStatus(res, 404);
  expect(res.data).toBe('Pago no encontrado.');
  });

  it('TC-PAY-08 returns 400 when the id is 0', async () => {
  const res = await api.getById(0);
  expectStatus(res, 400);
  expect(res.data).toBe('ID de pago inválido.');
  });
});

describe('GET /api/Payment/GetCommercePayments', () => {
  it('TC-PAY-09 returns 200 and only payments of that commerce', async () => {
  const res = await api.getCommercePayments(commerces.soda.id);
  expectStatus(res, 200);
  expect(res.data.length).toBeGreaterThanOrEqual(40);
  for (const p of res.data) expect(p.commerceId).toBe(commerces.soda.id);
  });

  it('TC-PAY-10 filters by status', async () => {
  const res = await api.getCommercePayments(commerces.soda.id, undefined, undefined, 'Pendiente');
  expectStatus(res, 200);
  expect(res.data.length).toBeGreaterThanOrEqual(10);
  for (const p of res.data) expect(p.status).toBe('Pendiente');
  });

  it('TC-PAY-12 returns 400 when the commerceId is 0', async () => {
  const res = await api.getCommercePayments(0);
  expectStatus(res, 400);
  expect(res.data).toBe('ID de comercio inválido.');
  });
});

describe('GET /api/Payment/GetBankPayments and GetBankStats', () => {
  it('TC-PAY-11 returns 400 when the bank code does not have 3 digits', async () => {
    const res = await api.getBankPayments('12');
    expectStatus(res, 400);
    expect(res.data).toBe('El código de banco debe tener 3 dígitos.');
  });

  it('TC-PAY-13 returns 200 and only payments whose IBAN belongs to the bank', async () => {
    const res = await api.getBankPayments(seededBankCode);
    expectStatus(res, 200);
    expect(res.data.length).toBeGreaterThanOrEqual(SEEDED_PAYMENTS_WITH_IBAN);
    for (const p of res.data) expect(p.iban.substring(5, 8)).toBe(seededBankCode);
  });

  it('TC-PAY-14 returns 200 and consistent statistics for the bank', async () => {
    const res = await api.getBankStats(seededBankCode);
    expectStatus(res, 200);
    expect(res.data.totalPayments).toBeGreaterThanOrEqual(SEEDED_PAYMENTS_WITH_IBAN);
    expect(res.data.completedPayments).toBeGreaterThanOrEqual(SEEDED_PAYMENTS_WITH_IBAN);
    expect(res.data.completedPayments + res.data.pendingPayments).toBeLessThanOrEqual(
      res.data.totalPayments,
    );
  });

  it('TC-PAY-15 returns 400 when the bank code for stats is longer than 3 digits', async () => {
    const res = await api.getBankStats('1234');
    expectStatus(res, 400);
    expect(res.data).toBe('El código de banco debe tener 3 dígitos.');
  });
});
