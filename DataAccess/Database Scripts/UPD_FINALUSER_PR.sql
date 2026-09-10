SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to update an existing FinalUser
CREATE PROCEDURE UPD_FINALUSER_PR
    @P_Id                        INT,
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
    @P_SelfieUrl                 VARCHAR(500),
    @P_FacialVerificationPassed  BIT,
    @P_ProfilePhotoUrl           VARCHAR(500),
    @P_IdCardFrontPhotoUrl       VARCHAR(500),
    @P_IdCardBackPhotoUrl        VARCHAR(500),
    @P_Status                    VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[TBL_FinalUser]
    SET
        NationalId               = @P_NationalId,
        FirstName                = @P_FirstName,
        LastName1                = @P_LastName1,
        LastName2                = @P_LastName2,
        PasswordHash             = @P_PasswordHash,
        Phone                    = @P_Phone,
        Email                    = @P_Email,
        DateOfBirth              = @P_DateOfBirth,
        LocationLat              = @P_LocationLat,
        LocationLng              = @P_LocationLng,
        SelfieUrl                = @P_SelfieUrl,
        FacialVerificationPassed = @P_FacialVerificationPassed,
        ProfilePhotoUrl          = @P_ProfilePhotoUrl,
        IdCardFrontPhotoUrl      = @P_IdCardFrontPhotoUrl,
        IdCardBackPhotoUrl       = @P_IdCardBackPhotoUrl,
        Status                   = @P_Status,
        UpdatedAt                = GETDATE()
    WHERE
        Id = @P_Id;
END
GO
