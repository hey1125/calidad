SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to update an existing OTP record
CREATE PROCEDURE UPD_OTP_VERIFICATION_PR
    @P_OtpId            INT,
    @P_UserId           INT,
    @P_VerificationType VARCHAR(10),
    @P_Code             VARCHAR(6),
    @P_ExpiresAt        DATETIME2,
    @P_IsVerified       BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[TBL_OTP_Verification]
    SET
        [UserId]           = @P_UserId,
        [VerificationType] = @P_VerificationType,
        [Code]             = @P_Code,
        [ExpiresAt]        = @P_ExpiresAt,
        [IsVerified]       = @P_IsVerified,
        [UpdatedAt]        = GETDATE()
    WHERE
        [OtpId] = @P_OtpId;
END
GO