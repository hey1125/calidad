SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve FinalUser by NationalId
CREATE PROCEDURE RET_FINALUSER_BY_NATIONALID_PR
    @P_NationalId VARCHAR(250)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [Id],
        [CreatedAt],
        [UpdatedAt],
        [NationalId],
        [FirstName],
        [LastName1],
        [LastName2],
        [PasswordHash],
        [Phone],
        [Email],
        [DateOfBirth],
        [LocationLat],
        [LocationLng],
        [SelfieUrl],
        [FacialVerificationPassed],
        [ProfilePhotoUrl],
        [IdCardFrontPhotoUrl],
        [IdCardBackPhotoUrl],
        [Status]
    FROM [dbo].[TBL_FinalUser]
    WHERE [NationalId] = @P_NationalId;
END
GO
