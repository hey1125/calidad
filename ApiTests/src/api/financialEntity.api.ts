import { request } from '../http/httpClient';
import type { FinancialEntity } from '../types/financialEntity';

type Id = number | string;

export const financialEntityApi = {
  retrieveById: (id: Id) =>
    request<FinancialEntity>({ method: 'GET', url: '/api/FinancialEntity/RetrieveById', params: { id } }),

  retrieveByBankCode: (bankCode: string) =>
    request<FinancialEntity>({
      method: 'GET',
      url: '/api/FinancialEntity/RetrieveByBankCode',
      params: { bankCode },
    }),

  create: (body: Record<string, unknown>) =>
    request<FinancialEntity>({ method: 'POST', url: '/api/FinancialEntity/Create', data: body }),
};
