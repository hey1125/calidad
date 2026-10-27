import { describe, it, expect } from 'vitest';
import { finalUserApi } from '../../src/api/finalUser.api';
import { paymentApi } from '../../src/api/payment.api';
import { users } from '../../src/fixtures/users';
import { expectStatus } from '../../src/helpers/assertions';

describe('Known defects: security (expected to FAIL)', () => {
  it('DEF-01 the login response must not expose the password', async () => {
    const res = await finalUserApi.login({
      email: users.active.email,
      password: users.active.password,
    });
    expectStatus(res, 200);
    expect(res.data).not.toHaveProperty('passwordHash');
  });

  it('DEF-02 reading a user payment history without credentials must be rejected', async () => {
    // The test client sends no token or credentials at all
    const res = await paymentApi.getUserHistoryPayment(users.active.nationalId);
    expect([401, 403]).toContain(res.status);
  });
});