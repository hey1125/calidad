import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios';
import https from 'node:https';
import { env } from '../config/env';
import { assertNotMessaging } from './messagingGuard';

const isLocal = /localhost|127\.0\.0\.1/.test(env.baseUrl);

const client = axios.create({
  baseURL: env.baseUrl,
  timeout: env.timeoutMs,
  headers: { 'Content-Type': 'application/json' },
  httpsAgent: new https.Agent({ rejectUnauthorized: !isLocal }), // local dev certificate only
  validateStatus: () => true, // never throw on 4xx/5xx: each test asserts the status itself
});

export function request<T = unknown>(
  config: AxiosRequestConfig & { method: string; url: string },
): Promise<AxiosResponse<T>> {
  assertNotMessaging(config.method, config.url);
  return client.request<T>(config);
}