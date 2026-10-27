// Test data from the local BilleTico DB (not production credentials)
export const users = {
    active: {
        id: 1,
        nationalId: '111111111',
        firstName: 'Esteban',
        email: 'ruizesteban@gmail.com',
        password: 'Prueba123',
    },
    admin: {
        id: 2,
        nationalId: '222222222',
        email: 'admin@billetico.com',
        password: 'AdminDemo123!',
    },
    prueba: {
        id: 3,
        nationalId: '202020202',
        email: 'prueba1@gmail.com',
        password: '654321',
    },
} as const;

export const unknownNationalId = '000000000';