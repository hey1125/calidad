/****** Object:  StoredProcedure [dbo].[CRE_FINALUSER_COMMERCE_ROL_PR]    Script Date: 10/8/2025 01:27:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CRE_FINALUSER_COMMERCE_ROL_PR]
    @P_UserId INT,
    @P_CommerceId INT,
    @P_RolId INT
AS
BEGIN
    INSERT INTO TBL_FinalUserCommerce_Rol (
        UserId,
        CommerceId,
        RolId,
        Created
    )
    VALUES (
        @P_UserId,
        @P_CommerceId,
        @P_RolId,
        GETDATE()
    );
END;
GO


