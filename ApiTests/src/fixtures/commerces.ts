// Seeded commerces in the local BilleTico DB
export const commerces = {
  soda: { id: 1, legalId: '3101000001', name: 'Soda La Esquina', status: 'Activo' },
  farmacia: { id: 2, legalId: '3101000002', name: 'Farmacia Central', status: 'Activo' },
  superBarrio: { id: 3, legalId: '3101000003', name: 'Super Barrio', status: 'Activo' },
} as const;

export const unknownCommerceId = 999999;
export const unknownLegalId = '0000000000';