CREATE OR ALTER PROCEDURE [dbo].[RET_USER_ROLE_IN_COMMERCE_PR]
  @P_UserId INT,
  @P_CommerceId INT,
  @P_RoleId INT
AS
BEGIN
  SET NOCOUNT ON;

  SELECT TOP 1 1 AS HasRole
  FROM dbo.TBL_FinalUserCommerce_Rol
  WHERE UserId = @P_UserId
    AND CommerceId = @P_CommerceId
    AND RolId = @P_RoleId;
END
