import axios from 'axios';
import https from 'node:https';
import { describe, it, expect } from 'vitest';

const BASE = process.env.API_BASE_URL ?? 'https://localhost:7163';
const agent = new https.Agent({ rejectUnauthorized: false }); // local dev certificate only

describe('smoke', () => {
  it('Swagger document is reachable and lists Login', async () => {
    const res = await axios.get(`${BASE}/swagger/v1/swagger.json`, { httpsAgent: agent });
    expect(res.status).toBe(200);
    expect(res.data.paths).toHaveProperty('/api/FinalUser/Login');
  });
});