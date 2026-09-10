-- Usuario administrador de ejemplo para desarrollo local.
-- No modifica una cuenta existente.
IF NOT EXISTS (SELECT 1 FROM dbo.TBL_FinalUser WHERE Email = 'admin@billetico.com')
BEGIN
    EXEC dbo.CRE_FINALUSER_PR
        @P_NationalId = '999999999',
        @P_FirstName = 'Administrador',
        @P_LastName1 = 'Demo',
        @P_LastName2 = 'BilleTico',
        @P_PasswordHash = 'AdminDemo123!',
        @P_Phone = '88888888',
        @P_Email = 'admin@billetico.com',
        @P_DateOfBirth = '1990-01-01',
        @P_LocationLat = 9.928069,
        @P_LocationLng = -84.090725,
        @P_SelfieUrl = '',
        @P_FacialVerificationPassed = 0,
        @P_ProfilePhotoUrl = '',
        @P_IdCardFrontPhotoUrl = '',
        @P_IdCardBackPhotoUrl = '',
        @P_Status = 'Active';
END;
GO
SELECT Id, Email, Status FROM dbo.TBL_FinalUser WHERE Email = 'admin@billetico.com';