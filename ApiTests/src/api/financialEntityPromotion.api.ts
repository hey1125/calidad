import { request } from '../http/httpClient';
import type { FinancialEntityPromotion } from '../types/financialEntity';

type Id = number | string;

export const financialEntityPromotionApi = {
  retrieveBestByIban: (iban: string, amount: Id, refDate?: string) =>
    request<FinancialEntityPromotion>({
      method: 'GET',
      url: '/api/FinancialEntityPromotion/RetrieveBestByIBAN',
      params: { iban, amount, refDate },
    }),

  retrieveActiveByIban: (iban: string, amount: Id, refDate?: string) =>
    request<FinancialEntityPromotion[]>({
      method: 'GET',
      url: '/api/FinancialEntityPromotion/RetrieveActiveByIBAN',
      params: { iban, amount, refDate },
    }),

  retrieveAllByEntity: (financialEntityId: Id) =>
    request<FinancialEntityPromotion[]>({
      method: 'GET',
      url: '/api/FinancialEntityPromotion/RetrieveAllByEntity',
      params: { financialEntityId },
    }),

  create: (body: Record<string, unknown>) =>
    request<FinancialEntityPromotion>({
      method: 'POST',
      url: '/api/FinancialEntityPromotion/Create',
      data: body,
    }),

  remove: (id: Id) =>
    request<unknown>({ method: 'DELETE', url: `/api/FinancialEntityPromotion/Delete/${id}` }),
};
