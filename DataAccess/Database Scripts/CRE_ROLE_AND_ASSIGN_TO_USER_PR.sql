CREATE PROCEDURE dbo.CRE_ROLE_AND_ASSIGN_TO_USER_PR
    @P_RoleName   NVARCHAR(100),
    @P_NationalId NVARCHAR(50),
    @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserId INT = (SELECT TOP 1 Id FROM dbo.TBL_FinalUser WHERE NationalId = @P_NationalId);
    IF @UserId IS NULL
    BEGIN
        -- Usuario no existe
        RAISERROR('USER_NOT_FOUND', 16, 1);
        RETURN;
    END

    DECLARE @RoleId INT = (SELECT TOP 1 Id FROM dbo.TBL_Rol WHERE Name = @P_RoleName);
    DECLARE @NewRole BIT = 0;

    IF @RoleId IS NULL
    BEGIN
        INSERT INTO dbo.TBL_Rol (Created, Name) VALUES (GETDATE(), @P_RoleName);
        SET @RoleId = SCOPE_IDENTITY();
        SET @NewRole = 1;
    END

    -- ¿ya tenía ese rol en ese comercio?
    IF EXISTS (
        SELECT 1
        FROM dbo.TBL_FinalUserCommerce_Rol
        WHERE UserId = @UserId AND CommerceId = @P_CommerceId AND RolId = @RoleId
    )
    BEGIN
        RAISERROR('ASSIGNMENT_EXISTS', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.TBL_FinalUserCommerce_Rol (Created, UserId, CommerceId, RolId)
    VALUES (GETDATE(), @UserId, @P_CommerceId, @RoleId);

    SELECT @RoleId AS RoleId, @NewRole AS NewRoleCreated, @UserId AS UserId;
END
GO