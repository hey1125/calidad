SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve all FinalUser records
CREATE PROCEDURE RET_ALL_FINALUSER_PR
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
    FROM [dbo].[TBL_FinalUser];
END
GO
