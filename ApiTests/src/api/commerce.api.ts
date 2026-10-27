import { request } from '../http/httpClient';
import type { Commerce } from '../types/commerce';

export const commerceApi = {
  retrieveActive: () =>
    request<Commerce[]>({ method: 'GET', url: '/api/Commerce/RetrieveActive' }),

  retrievePending: () =>
    request<Commerce[]>({ method: 'GET', url: '/api/Commerce/RetrievePending' }),

  // id accepts a string on purpose, so a test can send a non-numeric value
  retrieveById: (id: number | string) =>
    request<Commerce | string>({ method: 'GET', url: '/api/Commerce/RetrieveById', params: { id } }),

  retrieveByLegalId: (legalId: string) =>
    request<Commerce | string>({
      method: 'GET',
      url: '/api/Commerce/RetrieveByLegalId',
      params: { legalId },
    }),
};