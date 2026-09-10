CREATE PROCEDURE dbo.RET_ROLES_BY_COMMERCE_PR
    @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        r.UserId            AS UserId,
        u.NationalId        AS Cedula,
        u.FirstName         AS FirstName,
        u.LastName1         AS LastName,
        u.Email             AS Email,
        r.RolId             AS RoleId,
        rl.Name             AS RoleName
    FROM dbo.TBL_FinalUserCommerce_Rol r
    INNER JOIN dbo.TBL_FinalUser u ON u.Id = r.UserId
    INNER JOIN dbo.TBL_Rol rl      ON rl.Id = r.RolId
    WHERE r.CommerceId = @P_CommerceId
    ORDER BY rl.Name, u.FirstName, u.LastName1;
END
GO