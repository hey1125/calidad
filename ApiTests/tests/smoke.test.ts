import { describe, it, expect } from 'vitest';
import { request } from '../src/http/httpClient';

interface SwaggerDocument {
  paths: Record<string, unknown>;
}

describe('smoke', () => {
  it('Swagger document is reachable and lists Login', async () => {
    const res = await request<SwaggerDocument>({
      method: 'GET',
      url: '/swagger/v1/swagger.json',
    });

    expect(res.status).toBe(200);
    expect(res.data.paths).toHaveProperty('/api/FinalUser/Login');
  });
});
