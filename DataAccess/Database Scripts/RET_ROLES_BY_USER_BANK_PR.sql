
CREATE OR ALTER PROCEDURE [dbo].[RET_ROLES_BY_USER_BANK_PR]
    @P_UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.Id,                
        R.UserId,            
        R.FinancialEntityId, 
        R.RolId              
    FROM dbo.TBL_FinalUserFinancialEntity_Rol AS R
    WHERE R.UserId = @P_UserId;
END
GO