SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to create a new FinalUser
CREATE PROCEDURE [dbo].[CRE_FINALUSER_PR]
    @P_NationalId                VARCHAR(9),
    @P_FirstName                 VARCHAR(40),
    @P_LastName1                 VARCHAR(40),
    @P_LastName2                 VARCHAR(40),
    @P_PasswordHash              VARCHAR(100),
    @P_Phone                     VARCHAR(8),
    @P_Email                     VARCHAR(250),
    @P_DateOfBirth               DATE,
    @P_LocationLat               DECIMAL(9,6),
    @P_LocationLng               DECIMAL(9,6),
    @P_SelfieUrl                VARCHAR(500) = NULL,  -- Optional parameter for Selfie URL, default is NULL
    @P_FacialVerificationPassed  BIT,
    @P_ProfilePhotoUrl           VARCHAR(500),
    @P_IdCardFrontPhotoUrl       VARCHAR(500),
    @P_IdCardBackPhotoUrl        VARCHAR(500),
    @P_Status                    VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [dbo].[TBL_FinalUser] (
        NationalId,
        FirstName,
        LastName1,
        LastName2,
        PasswordHash,
        Phone,
        Email,
        DateOfBirth,
        LocationLat,
        LocationLng,
        SelfieUrl,
        FacialVerificationPassed,
        ProfilePhotoUrl,
        IdCardFrontPhotoUrl,
        IdCardBackPhotoUrl,
        Status
    )
    VALUES (
        @P_NationalId,
        @P_FirstName,
        @P_LastName1,
        @P_LastName2,
        @P_PasswordHash,
        @P_Phone,
        @P_Email,
        @P_DateOfBirth,
        @P_LocationLat,
        @P_LocationLng,
        @P_SelfieUrl,
        @P_FacialVerificationPassed,
        @P_ProfilePhotoUrl,
        @P_IdCardFrontPhotoUrl,
        @P_IdCardBackPhotoUrl,
        @P_Status
    );
END
GO
