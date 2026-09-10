-- ========================================
-- Crear tabla TBL_FinancialEntity si no existe
-- ========================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TBL_FinancialEntity' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[TBL_FinancialEntity](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Created] [datetime] NOT NULL DEFAULT GETDATE(),
        [Updated] [datetime] NULL,
        [LegalId] [nvarchar](12) NOT NULL,
        [BankCode] [nvarchar](3) NOT NULL,
        [Name] [nvarchar](100) NOT NULL,
        [Phone] [nvarchar](8) NOT NULL,
        [Email] [nvarchar](100) NOT NULL,
        [Latitude] [decimal](9,6) NOT NULL,
        [Longitude] [decimal](9,6) NOT NULL,
        [Status] [nvarchar](15) NOT NULL,
        [CommissionRate] [decimal](5,2) NULL DEFAULT 0.00,
     CONSTRAINT [PK_TBL_FinancialEntity] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

-- ========================================
-- Procedimiento: CRE_FINANCIAL_ENTITY_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'CRE_FINANCIAL_ENTITY_PR')
    DROP PROCEDURE CRE_FINANCIAL_ENTITY_PR
GO

CREATE PROCEDURE CRE_FINANCIAL_ENTITY_PR
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
    )
END
GO

-- ========================================
-- Procedimiento: RET_FINANCIAL_ENTITIES_BY_STATUS_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'RET_FINANCIAL_ENTITIES_BY_STATUS_PR')
    DROP PROCEDURE RET_FINANCIAL_ENTITIES_BY_STATUS_PR
GO

CREATE PROCEDURE RET_FINANCIAL_ENTITIES_BY_STATUS_PR
    @P_Status NVARCHAR(15)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, CommissionRate
    FROM TBL_FinancialEntity
    WHERE Status = @P_Status
    ORDER BY Created DESC
END
GO

-- ========================================
-- Procedimiento: RET_FINANCIAL_ENTITY_BY_ID_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'RET_FINANCIAL_ENTITY_BY_ID_PR')
    DROP PROCEDURE RET_FINANCIAL_ENTITY_BY_ID_PR
GO

CREATE PROCEDURE RET_FINANCIAL_ENTITY_BY_ID_PR
    @P_Id INT
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, CommissionRate
    FROM TBL_FinancialEntity
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: APPROVE_FINANCIAL_ENTITY_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'APPROVE_FINANCIAL_ENTITY_PR')
    DROP PROCEDURE APPROVE_FINANCIAL_ENTITY_PR
GO

CREATE PROCEDURE APPROVE_FINANCIAL_ENTITY_PR
    @P_Id INT,
    @P_CommissionRate DECIMAL(5,2)
AS
BEGIN
    UPDATE TBL_FinancialEntity
    SET Status = 'Activa',
        CommissionRate = @P_CommissionRate,
        Updated = GETDATE()
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: REJECT_FINANCIAL_ENTITY_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'REJECT_FINANCIAL_ENTITY_PR')
    DROP PROCEDURE REJECT_FINANCIAL_ENTITY_PR
GO

CREATE PROCEDURE REJECT_FINANCIAL_ENTITY_PR
    @P_Id INT
AS
BEGIN
    UPDATE TBL_FinancialEntity
    SET Status = 'Rechazada',
        Updated = GETDATE()
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR')
    DROP PROCEDURE RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR
GO

CREATE PROCEDURE RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR
    @P_BankCode NVARCHAR(3)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, CommissionRate
    FROM TBL_FinancialEntity
    WHERE BankCode = @P_BankCode
END
GO

-- ========================================
-- Procedimiento: RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR
-- ========================================
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR')
    DROP PROCEDURE RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR
GO

CREATE PROCEDURE RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR
    @P_LegalId NVARCHAR(12)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, CommissionRate
    FROM TBL_FinancialEntity
    WHERE LegalId = @P_LegalId
END
GO