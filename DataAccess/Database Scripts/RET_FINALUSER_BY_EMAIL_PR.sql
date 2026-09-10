SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve FinalUser by Email
CREATE PROCEDURE RET_FINALUSER_BY_EMAIL_PR
    @P_Email VARCHAR(250)
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
    WHERE [Email] = @P_Email;
END
GO
