/****** Object:  StoredProcedure [dbo].[CRE_FINALUSER_FINANCIALENTITY_ROL_PR]    Script Date: 10/8/2025 01:32:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CRE_FINALUSER_FINANCIALENTITY_ROL_PR]
    @P_UserId INT,
    @P_FinancialEntityId INT,
    @P_RolId INT
AS
BEGIN
    INSERT INTO TBL_FinalUserFinancialEntity_Rol (
        UserId, FinancialEntityId, RolId, Created
    )
    VALUES (
        @P_UserId, @P_FinancialEntityId, @P_RolId, GETDATE()
    );
END
GO


