import { describe, it, expect } from 'vitest';
import { commercePromotionApi } from '../../src/api/commercePromotion.api';
import { paymentApi } from '../../src/api/payment.api';
import { unknownPromoId } from '../../src/fixtures/promotions';
import { users } from '../../src/fixtures/users';
import { expectStatus } from '../../src/helpers/assertions';

describe('Known defects: validation (hypotheses, confirm on run)', () => {
  it('DEF-03 a history range with startDate after endDate should be rejected with 400', async () => {
    const res = await paymentApi.getUserHistoryPayment(
      users.active.nationalId,
      '2026-12-31',
      '2026-01-01',
    );
    expectStatus(res, 400);
  });

  it('DEF-04 deleting a promotion that does not exist should return 404', async () => {
    const res = await commercePromotionApi.remove(unknownPromoId);
    expectStatus(res, 404);
  });

  it('DEF-05 bank stats for a bank without payments should return 200 with zero counts', async () => {
    const res = await paymentApi.getBankStats('999');
    expectStatus(res, 200);
    expect(res.data).toMatchObject({ totalPayments: 0, pendingPayments: 0, completedPayments: 0 });
  });
});