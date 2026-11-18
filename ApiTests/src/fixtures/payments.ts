// Seeded payments in the local BilleTico DB (PruebasRendimiento/sql/02_DATOS_PRUEBA.sql).
// Each of 5 payers has 8 payments on commerce 1, PaymentDate = seed day minus 45 * n days.
// The seed ran on 2026-10-26, so the date-range test below depends on that.
export const SEEDED_PAYMENTS_PER_USER = 8;

export const seededPayment = {
  id: 1,
  paymentId: 'JMETER20860002501',
  nationalId: '208600025',
  commerceId: 1,
  status: 'Pagado',
  grossAmount: 1500,
} as const;

// Bank code = characters 6 to 8 of the IBAN CR05 0152 ... of the seeded commerce
export const seededBankCode = '152';

// Contains the payments #1 (Sep 11) and #2 (Jul 28) of every payer, and nothing else
export const RANGE_JUL_SEP_2026 = { start: '2026-07-01', end: '2026-09-30' } as const;

export const unknownPaymentId = 999999;

// Only paid payments carry an IBAN in the seed (pending ones have IBAN = NULL),
// so a bank filter sees 30 of the 40 payments: 5 payers x 6 paid payments
export const SEEDED_PAYMENTS_WITH_IBAN = 30;