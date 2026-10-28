import { request } from '../http/httpClient';
import type { FinalUser } from '../types/finalUser';

export const finalUserApi = {
  // body is loosely typed on purpose, so negative tests can send incomplete payloads
  login: (body: Record<string, unknown>) =>
    request<FinalUser | string>({ method: 'POST', url: '/api/FinalUser/Login', data: body }),

  // nationalId is optional so a test can omit the query parameter entirely
  retrieveByNationalId: (nationalId?: string) =>
    request<FinalUser | string>({
      method: 'GET',
      url: '/api/FinalUser/RetrieveByNationalId',
      params: nationalId === undefined ? {} : { nationalId },
    }),
};
