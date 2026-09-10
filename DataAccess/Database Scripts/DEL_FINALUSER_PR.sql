SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Stored Procedure to delete an existing FinalUser by Id
CREATE PROCEDURE DEL_FINALUSER_PR
    @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [dbo].[TBL_FinalUser]
    WHERE Id = @P_Id;
END
GO
