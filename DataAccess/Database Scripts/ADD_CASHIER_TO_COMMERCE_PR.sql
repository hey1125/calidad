ALTER PROCEDURE [dbo].[ADD_CASHIER_TO_COMMERCE_PR]
  @P_NationalId NVARCHAR(50),
  @P_CommerceId INT,
  @P_RoleId INT
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @UserId INT = (SELECT TOP 1 Id FROM TBL_FinalUser WHERE NationalId = @P_NationalId);

  IF @UserId IS NULL
  BEGIN
    RAISERROR('Usuario no encontrado por cédula', 16, 1);
    RETURN;
  END

  IF EXISTS (SELECT 1
             FROM TBL_FinalUserCommerce_Rol
             WHERE UserId = @UserId AND CommerceId = @P_CommerceId AND RolId = @P_RoleId)
  BEGIN
    RAISERROR('DUPLICATE', 16, 1);
    RETURN;
  END

  INSERT INTO TBL_FinalUserCommerce_Rol (Created, UserId, CommerceId, RolId)
  VALUES (GETDATE(), @UserId, @P_CommerceId, @P_RoleId);
END
