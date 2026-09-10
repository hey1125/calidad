SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TBL_FinalUser] (
    [Id] INT IDENTITY(1,1)         NOT NULL,
    [CreatedAt] DATETIME2          NOT NULL CONSTRAINT DF_TBL_User_CreatedAt DEFAULT(GETDATE()),
    [UpdatedAt] DATETIME2          NULL     CONSTRAINT DF_TBL_User_UpdatedAt DEFAULT(GETDATE()),
    [NationalId] VARCHAR(9)        NOT NULL,
    [FirstName] VARCHAR(40)        NOT NULL,
    [LastName1] VARCHAR(40)        NOT NULL,
    [LastName2] VARCHAR(40)        NOT NULL,
    [PasswordHash] VARCHAR(100)    NOT NULL,
    [Phone] VARCHAR(8)             NOT NULL,
    [Email] VARCHAR(250)           NOT NULL,
    [DateOfBirth] DATE             NOT NULL,
    [LocationLat] DECIMAL(9,6)     NOT NULL,
    [LocationLng] DECIMAL(9,6)     NOT NULL,
    [SelfieUrl] VARCHAR(500)       NULL,
    [FacialVerificationPassed] BIT NOT NULL,
    [ProfilePhotoUrl] VARCHAR(500)       NOT NULL,
    [IdCardFrontPhotoUrl] VARCHAR(500)   NOT NULL,
    [IdCardBackPhotoUrl] VARCHAR(500)    NOT NULL,
    [Status] VARCHAR(10)           NOT NULL,
    CONSTRAINT PK_TBL_FinalUser PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT CK_TBL_FinalUser_Status CHECK ([Status] IN ('Pending','Active','Inactive'))
) ON [PRIMARY]
GO
