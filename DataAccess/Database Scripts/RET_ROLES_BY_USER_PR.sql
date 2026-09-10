/****** Object:  StoredProcedure [dbo].[RET_ROLES_BY_USER_PR]    Script Date: 10/8/2025 01:31:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RET_ROLES_BY_USER_PR]
    @P_UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        CommerceId,
        RolId
    FROM [dbo].[TBL_FinalUserCommerce_Rol]
    WHERE UserId = @P_UserId;
END;
GO


