import type { AxiosResponse } from 'axios';
import { expect } from 'vitest';

export function expectStatus(res: AxiosResponse, status: number): void {
  expect(res.status, `unexpected status, body: ${JSON.stringify(res.data)}`).toBe(status);
}