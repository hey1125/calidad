/* =====================================================================
   BilleTico - Entidad financiera de prueba para ApiTests
   Se puede correr varias veces.

   1. Alinea la base con el codigo: FinancialEntityCrudFactory lee la columna
      NationalId, pero la tabla y los procedimientos del repo no la tienen, y
      cualquier consulta de un banco existente responde 500
      ("The given key 'NationalId' was not present in the dictionary.").
      Es el mismo arreglo que 04_COMMERCE_NATIONALID.sql hizo para comercios.
   2. Crea el banco 987 que usan las pruebas de FinancialEntity y
      FinancialEntityPromotion.

   Requiere: haber corrido PruebasRendimiento/sql/01_CREAR_FALTANTES.sql
   ===================================================================== */
USE BilleTico;
GO
SET NOCOUNT ON;

IF COL_LENGTH('dbo.TBL_FinancialEntity', 'NationalId') IS NULL
BEGIN
    ALTER TABLE dbo.TBL_FinancialEntity
        ADD NationalId NVARCHAR(9) NOT NULL CONSTRAINT DF_TBL_FinancialEntity_NationalId DEFAULT ('');
    PRINT 'Agregada la columna NationalId a TBL_FinancialEntity.';
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_FINANCIAL_ENTITY_BY_ID_PR @P_Id INT
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude,
           Status, CommissionRate, NationalId
    FROM dbo.TBL_FinancialEntity WHERE Id = @P_Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_FINANCIAL_ENTITY_BY_BANK_CODE_PR @P_BankCode NVARCHAR(3)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude,
           Status, CommissionRate, NationalId
    FROM dbo.TBL_FinancialEntity WHERE BankCode = @P_BankCode;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_FINANCIAL_ENTITY_BY_LEGAL_ID_PR @P_LegalId NVARCHAR(12)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude,
           Status, CommissionRate, NationalId
    FROM dbo.TBL_FinancialEntity WHERE LegalId = @P_LegalId;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_FINANCIAL_ENTITIES_BY_STATUS_PR @P_Status NVARCHAR(15)
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, BankCode, Name, Phone, Email, Latitude, Longitude,
           Status, CommissionRate, NationalId
    FROM dbo.TBL_FinancialEntity WHERE Status = @P_Status ORDER BY Created DESC;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TBL_FinancialEntity WHERE BankCode = '987')
BEGIN
    INSERT INTO dbo.TBL_FinancialEntity
        (LegalId, BankCode, Name, Phone, Email, Latitude, Longitude, Status, CommissionRate, NationalId)
    VALUES ('3101000987', '987', 'Banco de Pruebas ApiTests', '22220987',
            'apitests@billetico.test', 9.932500, -84.079600, 'Activa', 1.50, '111111111');
    PRINT 'Creada la entidad financiera de prueba (banco 987).';
END
ELSE
BEGIN
    UPDATE dbo.TBL_FinancialEntity
    SET Status = 'Activa', CommissionRate = 1.50, NationalId = '111111111'
    WHERE BankCode = '987';
    PRINT 'La entidad financiera de prueba (banco 987) ya existe; datos actualizados.';
END
GO

SELECT Id, BankCode, Name, Status, CommissionRate, NationalId
FROM dbo.TBL_FinancialEntity WHERE BankCode = '987';
GO
