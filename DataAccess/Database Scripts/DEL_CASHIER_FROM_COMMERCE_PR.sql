/****** Object:  StoredProcedure [dbo].[DEL_CASHIER_FROM_COMMERCE_PR]    Script Date: 10/8/2025 01:24:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DEL_CASHIER_FROM_COMMERCE_PR]
    @P_UserId INT,
    @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[TBL_FinalUserCommerce_Rol]
    WHERE UserId = @P_UserId
      AND CommerceId = @P_CommerceId
      AND RolId = 2;
END;
GO


