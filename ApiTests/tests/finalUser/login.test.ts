import { describe, it, expect } from 'vitest';
import { finalUserApi } from '../../src/api/finalUser.api';
import { users } from '../../src/fixtures/users';
import { expectStatus } from '../../src/helpers/assertions';

describe('POST /api/FinalUser/Login', () => {
    it('TC-LOGIN-01 returns 200 and the user for valid credentials', async () => {
        const res = await finalUserApi.login({ email: users.active.email, password: users.active.password });
        expectStatus(res, 200);
        expect(res.data).toMatchObject({
            id: users.active.id,
            nationalId: users.active.nationalId,
            firstName: users.active.firstName,
            email: users.active.email,
            status: 'Active',
        });
    });

    it('TC-LOGIN-02 returns 401 when the password is wrong', async () => {
        const res = await finalUserApi.login({ email: users.active.email, password: 'wrong-password' });
        expectStatus(res, 401);
        expect(res.data).toBe('Contraseña incorrecta.');
    });

    it('TC-LOGIN-03 returns 401 when the email is not registered', async () => {
        const res = await finalUserApi.login({ email: 'nobody@example.com', password: 'Prueba123' });
        expectStatus(res, 401);
        expect(res.data).toBe('Correo no registrado.');
    });

    it('TC-LOGIN-04 returns 400 when email and password are missing', async () => {
        const res = await finalUserApi.login({});
        expectStatus(res, 400);
        expect(res.data).toMatchObject({ status: 400 });
        expect(res.data).toHaveProperty('errors');
    });

    it('TC-LOGIN-05 returns 400 when only the password is missing', async () => {
        const res = await finalUserApi.login({ email: users.active.email });
        expectStatus(res, 400);
        expect(res.data).toHaveProperty('errors');
    });

    it.each([
        { id: 'TC-LOGIN-06', user: users.prueba },
        { id: 'TC-LOGIN-07', user: users.admin },
    ])('$id returns 200 for $user.email', async ({ user }) => {
        const res = await finalUserApi.login({ email: user.email, password: user.password });
        expectStatus(res, 200);
        expect(res.data).toMatchObject({ id: user.id, email: user.email, status: 'Active' });
    });
});