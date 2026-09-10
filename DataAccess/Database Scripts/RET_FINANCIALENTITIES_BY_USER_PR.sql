CREATE PROCEDURE RET_FINANCIALENTITIES_BY_USER_PR
    @P_UserId INT
AS
BEGIN
    SELECT F.*
    FROM TBL_FinancialEntity F
    INNER JOIN TBL_FinalUserFinancialEntity_Rol R ON F.Id = R.FinancialEntityId
    WHERE R.UserId = @P_UserId
END;
GO