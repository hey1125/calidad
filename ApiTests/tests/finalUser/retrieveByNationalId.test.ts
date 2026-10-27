import { describe, it, expect } from 'vitest';
import { finalUserApi } from '../../src/api/finalUser.api';
import { unknownNationalId, users } from '../../src/fixtures/users';
import { expectStatus } from '../../src/helpers/assertions';

describe('GET /api/FinalUser/RetrieveByNationalId', () => {
  it.each([
    { id: 'TC-USER-01', user: users.active },
    { id: 'TC-USER-02', user: users.prueba },
  ])('$id returns 200 and the user for nationalId $user.nationalId', async ({ user }) => {
    const res = await finalUserApi.retrieveByNationalId(user.nationalId);
    expectStatus(res, 200);
    expect(res.data).toMatchObject({ id: user.id, nationalId: user.nationalId, email: user.email });
  });

  it('TC-USER-03 returns 404 when the nationalId does not exist', async () => {
    const res = await finalUserApi.retrieveByNationalId(unknownNationalId);
    expectStatus(res, 404);
    expect(res.data).toBe('Usuario no encontrado.');
  });

  it('TC-USER-04 returns 400 when the nationalId parameter is omitted', async () => {
    const res = await finalUserApi.retrieveByNationalId();
    expectStatus(res, 400);
  });
});