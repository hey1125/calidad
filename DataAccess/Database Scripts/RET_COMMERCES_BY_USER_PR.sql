/****** Object:  StoredProcedure [dbo].[RET_COMMERCES_BY_USER_PR]    Script Date: 10/8/2025 01:29:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RET_COMMERCES_BY_USER_PR]
    @P_UserId INT
AS
BEGIN
    SELECT C.*
    FROM TBL_Commerce C
    INNER JOIN TBL_FinalUserCommerce_Rol R ON C.Id = R.CommerceId
    WHERE R.UserId = @P_UserId
END;
GO


