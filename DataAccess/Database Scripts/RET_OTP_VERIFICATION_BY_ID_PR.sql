SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve an OTP record by its OtpId
CREATE PROCEDURE RET_OTP_VERIFICATION_BY_ID_PR
    @P_OtpId INT
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
    WHERE [OtpId] = @P_OtpId;
END
GO