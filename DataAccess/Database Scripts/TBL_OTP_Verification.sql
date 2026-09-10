SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TBL_OTP_Verification] (
    [OtpId] INT IDENTITY(1,1)           NOT NULL,
    [CreatedAt] DATETIME2               NOT NULL CONSTRAINT DF_TBL_OTP_Verification_CreatedAt DEFAULT(GETDATE()),
    [UpdatedAt] DATETIME2               NULL     CONSTRAINT DF_TBL_OTP_Verification_UpdatedAt DEFAULT(GETDATE()),
    [UserId] INT                        NOT NULL,
    [VerificationType] VARCHAR(10)      NOT NULL,
    [Code] VARCHAR(6)                   NOT NULL,
    [ExpiresAt] DATETIME2               NOT NULL,
    [IsVerified] BIT                    NOT NULL,
    CONSTRAINT PK_TBL_OTP_Verification PRIMARY KEY CLUSTERED ([OtpId] ASC),
    CONSTRAINT FK_TBL_OTP_Verification_User FOREIGN KEY ([UserId])
        REFERENCES [dbo].[TBL_FinalUser]([Id]),
    CONSTRAINT CK_TBL_OTP_Verification_Type CHECK ([VerificationType] IN ('SMS', 'EMAIL'))
) ON [PRIMARY]
GO
