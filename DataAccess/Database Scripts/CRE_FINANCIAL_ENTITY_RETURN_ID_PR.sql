CREATE PROCEDURE CRE_FINANCIAL_ENTITY_RETURN_ID_PR
    @P_LegalId NVARCHAR(12),
    @P_BankCode NVARCHAR(3),
    @P_Name NVARCHAR(100),
    @P_Phone NVARCHAR(8),
    @P_Email NVARCHAR(100),
    @P_Latitude DECIMAL(9,6),
    @P_Longitude DECIMAL(9,6),
    @P_Status NVARCHAR(15)
AS
BEGIN
    INSERT INTO TBL_FinancialEntity (
        LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, Created
    )
    VALUES (
        @P_LegalId, @P_BankCode, @P_Name, @P_Phone, @P_Email, @P_Latitude, @P_Longitude, @P_Status, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS ID;
END
GO