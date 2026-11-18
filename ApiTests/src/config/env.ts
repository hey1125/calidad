export const env = {
  baseUrl: process.env.API_BASE_URL ?? 'https://localhost:7163',
  timeoutMs: Number(process.env.API_TIMEOUT_MS ?? 15000),
  allowMessaging: process.env.ALLOW_MESSAGING === '1',
} as const;