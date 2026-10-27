import { request } from '../http/httpClient';
import type { FinalUser } from '../types/finalUser';

export const finalUserApi = {
    // body is loosely typed on purpose, so negative tests can send incomplete payloads
    login: (body: Record<string, unknown>) =>
        request<FinalUser | string>({ method: 'POST', url: '/api/FinalUser/Login', data: body }),
};