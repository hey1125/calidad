CREATE PROCEDURE CRE_FINALUSER_FINANCIALENTITY_ROL_PR
    @P_UserId INT,
    @P_FinancialEntityId INT,
    @P_RolId INT
AS
BEGIN
    INSERT INTO TBL_FinalUserFinancialEntity_Rol (
        UserId, FinancialEntityId, RolId, Created
    )
    VALUES (
        @P_UserId, @P_FinancialEntityId, @P_RolId, GETDATE()
    );
END
GO