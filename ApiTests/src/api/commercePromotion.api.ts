import { request } from '../http/httpClient';
import type { CommercePromotion } from '../types/commercePromotion';

type Id = number | string;

export const commercePromotionApi = {
  retrieveBestByCommerce: (commerceId: Id, amount: Id, refDate?: string) =>
    request<CommercePromotion>({
      method: 'GET',
      url: '/api/CommercePromotion/RetrieveBestByCommerce',
      params: { commerceId, amount, refDate },
    }),

  retrieveActiveByCommerce: (commerceId: Id, amount: Id, refDate?: string) =>
    request<CommercePromotion[]>({
      method: 'GET',
      url: '/api/CommercePromotion/RetrieveActiveByCommerce',
      params: { commerceId, amount, refDate },
    }),

  retrieveAllByCommerce: (commerceId: Id) =>
    request<CommercePromotion[]>({
      method: 'GET',
      url: '/api/CommercePromotion/RetrieveAllByCommerce',
      params: { commerceId },
    }),

  retrieveById: (id: Id) =>
    request<CommercePromotion>({ method: 'GET', url: `/api/CommercePromotion/RetrieveById/${id}` }),

  create: (body: Record<string, unknown>) =>
    request<CommercePromotion>({ method: 'POST', url: '/api/CommercePromotion/Create', data: body }),

  update: (id: Id, body: Record<string, unknown>) =>
    request<CommercePromotion>({ method: 'PUT', url: `/api/CommercePromotion/Update/${id}`, data: body }),

  remove: (id: Id) =>
    request<boolean>({ method: 'DELETE', url: `/api/CommercePromotion/Delete/${id}` }),
};