SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve all OTP records for a given UserId
ALTER PROCEDURE [dbo].[RET_OTP_VERIFICATION_BY_USERID_PR]
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
