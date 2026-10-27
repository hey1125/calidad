// Seeded financial entity used only by the FinancialEntity / FinancialEntityPromotion suites.
// Load it once with ApiTests/sql/seed_financial_entity.sql (bank code 987 is not used by any payment,
// so it never affects the Payment tests that work with bank 152).
export const testBank = {
  bankCode: '987',
  legalId: '3101000987',
  name: 'Banco de Pruebas ApiTests',
  phone: '22220987',
  email: 'apitests@billetico.test',
  status: 'Activa',
  commissionRate: 1.5,
} as const;

// IBAN whose bank code (characters 6 to 8, same rule as the API) is the test bank: CR05 0 987 ...
export const testBankIban = 'CR05098701001026280001';

// IBAN of a bank that does not exist (bank code 000)
export const unknownBankIban = 'CR05000001001026280001';
export const unknownBankCode = '000';
export const unknownFinancialEntityId = 999999;

// Marks every promotion the tests create, so leftovers from an interrupted run can be cleaned up
export const FE_PROMO_TEST_DESCRIPTION = 'ApiTests FE promotion';
