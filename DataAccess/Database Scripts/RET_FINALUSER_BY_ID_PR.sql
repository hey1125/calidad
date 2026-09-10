SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to retrieve a FinalUser by Id
CREATE PROCEDURE RET_FINALUSER_BY_ID_PR
    @P_Id INT
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
    WHERE [Id] = @P_Id;
END
GO
