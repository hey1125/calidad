
CREATE OR ALTER PROCEDURE [dbo].[RET_CASHIERS_BY_COMMERCE_PR]
    @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        u.Id,
        u.NationalId AS Cedula,      
        u.FirstName,
        u.LastName1 AS LastName,
        u.Email
    FROM dbo.TBL_FinalUserCommerce_Rol r
    INNER JOIN dbo.TBL_FinalUser u
        ON u.Id = r.UserId
    WHERE r.CommerceId = @P_CommerceId
      AND r.RolId = 2                 -- Cajeros
    ORDER BY u.FirstName, u.LastName1;
END