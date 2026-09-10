CREATE OR ALTER PROCEDURE [dbo].[ADD_CASHIER_BY_CEDULA_PR]
    @P_NationalId NVARCHAR(50),
    @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT;

    SELECT @UserId = Id
    FROM dbo.TBL_FinalUser
    WHERE NationalId = @P_NationalId;

    IF @UserId IS NULL
    BEGIN
        RAISERROR('No existe un usuario con esa cédula.', 16, 1);
        RETURN;
    END

    -- Evitar duplicados
    IF EXISTS (
        SELECT 1
        FROM dbo.TBL_FinalUserCommerce_Rol
        WHERE UserId = @UserId
          AND CommerceId = @P_CommerceId
          AND RolId = 2 -- Cajero
    )
        RETURN;

    INSERT INTO dbo.TBL_FinalUserCommerce_Rol (Created, UserId, CommerceId, RolId)
    VALUES (GETDATE(), @UserId, @P_CommerceId, 2);
END