-- Procedimientos de cuentas bancarias para dbo.TBL_Bank_Account.
-- Puede ejecutarse nuevamente sin borrar datos.
CREATE OR ALTER PROCEDURE dbo.CRE_BANK_ACCOUNT_PR
    @UserId INT,
    @BankId INT,
    @Iban VARCHAR(34)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_Bank_Account (user_id, bank_id, iban)
    VALUES (@UserId, @BankId, @Iban);
END;
GO

CREATE OR ALTER PROCEDURE dbo.RET_ALL_BANK_ACCOUNTS_BY_USER_PR
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT account_id AS Id, user_id AS UserId, bank_id AS BankId,
           iban AS IBAN, created AS Created, updated AS Updated
    FROM dbo.TBL_Bank_Account
    WHERE user_id = @UserId
    ORDER BY account_id;
END;
GO

CREATE OR ALTER PROCEDURE dbo.RET_BANK_ACCOUNT_BY_IBAN_AND_USER_PR
    @UserId INT,
    @Iban VARCHAR(34)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT account_id AS Id, user_id AS UserId, bank_id AS BankId,
           iban AS IBAN, created AS Created, updated AS Updated
    FROM dbo.TBL_Bank_Account
    WHERE user_id = @UserId AND iban = @Iban;
END;
GO

CREATE OR ALTER PROCEDURE dbo.DEL_BANK_ACCOUNT_BY_ID_PR
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TBL_Bank_Account WHERE account_id = @Id;
END;
GO