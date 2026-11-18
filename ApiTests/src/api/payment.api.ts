import { request } from '../http/httpClient';
import type { BankStats, Payment } from '../types/payment';

type Id = number | string;

export const paymentApi = {
  // undefined params are omitted from the query string; an empty string is sent as-is
  getUserHistoryPayment: (nationalId?: string, startDate?: string, endDate?: string) =>
    request<Payment[]>({
      method: 'GET',
      url: '/api/Payment/GetUserHistoryPayment',
      params: { nationalId, startDate, endDate },
    }),

  getById: (id: Id) => request<Payment>({ method: 'GET', url: `/api/Payment/GetById/${id}` }),

  getCommercePayments: (commerceId: Id, startDate?: string, endDate?: string, status?: string) =>
    request<Payment[]>({
      method: 'GET',
      url: '/api/Payment/GetCommercePayments',
      params: { commerceId, startDate, endDate, status },
    }),

  getBankPayments: (bankCode: string, startDate?: string, endDate?: string) =>
    request<Payment[]>({
      method: 'GET',
      url: '/api/Payment/GetBankPayments',
      params: { bankCode, startDate, endDate },
    }),

  getBankStats: (bankCode: string) =>
    request<BankStats>({ method: 'GET', url: '/api/Payment/GetBankStats', params: { bankCode } }),
};