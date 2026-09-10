SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to delete an OTP record by its OtpId
CREATE PROCEDURE DEL_OTP_VERIFICATION_PR
    @P_OtpId INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[TBL_OTP_Verification]
    WHERE [OtpId] = @P_OtpId;
END
GO