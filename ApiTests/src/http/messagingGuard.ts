import { env } from '../config/env';

// Endpoints that send real email/SMS or call external services (SendGrid, Twilio, AWS)
const BLOCKED = [
  'POST /api/FinalUser/Create',
  'POST /api/FinalUser/ValidateUser',
  'POST /api/FinalUser/RecoverPassword',
  'PUT /api/Commerce/Approve',
  'PUT /api/Commerce/Reject',
  'PUT /api/FinancialEntity/Approve',
  'PUT /api/FinancialEntity/Reject',
  'POST /api/Payment/Create',
  'PUT /api/Payment/Confirm',
].map((s) => s.toLowerCase());

export function assertNotMessaging(method: string, url: string): void {
  const path = url.split('?')[0];
  const key = `${method} ${path}`.toLowerCase();
  if (!env.allowMessaging && BLOCKED.includes(key)) {
    throw new Error(
      `Blocked by messagingGuard: ${method} ${path} may send a real email/SMS. ` +
        `Set ALLOW_MESSAGING=1 only if the team agrees.`,
    );
  }
}