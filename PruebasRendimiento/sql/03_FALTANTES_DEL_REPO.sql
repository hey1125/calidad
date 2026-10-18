-- Los 2 scripts del repo usan ALTER PROCEDURE, que falla si el procedimiento no existe.
-- Aqui van con CREATE OR ALTER (funciona exista o no).
USE BilleTico;
GO
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[ADD_CASHIER_TO_COMMERCE_PR]
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
GO

GO
GO

-- Stored Procedure to retrieve all OTP records for a given UserId
CREATE OR ALTER PROCEDURE [dbo].[RET_OTP_VERIFICATION_BY_USERID_PR]
    @P_UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [OtpId],
        [CreatedAt],
        [UpdatedAt],
        [UserId],
        [VerificationType],
        [Code],
        [ExpiresAt],
        [IsVerified]
    FROM [dbo].[TBL_OTP_Verification]
    WHERE [UserId] = @P_UserId
    ORDER BY [CreatedAt] DESC;  -- toma el más reciente
END
GO
