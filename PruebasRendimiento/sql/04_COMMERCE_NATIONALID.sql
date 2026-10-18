/* =====================================================================
   BilleTico - TBL_Commerce desactualizada respecto al codigo
   ---------------------------------------------------------------------
   El codigo C# (CommerceCrudFactory):
     - envia @P_NationalId a CRE_COMMERCE_PR y CRE_COMMERCE_RETURN_ID_PR
     - lee row["NationalId"] en BuildCommerce (para TODAS las consultas)
   Pero la tabla y los SP del repo no tienen esa columna, por eso:
     - registrar un comercio desde la app falla ("too many arguments")
     - cualquier consulta de comercios con datos daria error 500.
   Este script agrega la columna y actualiza los SP. Se puede correr
   varias veces.
   ===================================================================== */
USE BilleTico;
GO

IF COL_LENGTH('dbo.TBL_Commerce', 'NationalId') IS NULL
BEGIN
    ALTER TABLE dbo.TBL_Commerce
        ADD NationalId NVARCHAR(9) NOT NULL CONSTRAINT DF_TBL_Commerce_NationalId DEFAULT ('');
    PRINT 'Columna NationalId agregada a TBL_Commerce';
END
GO

CREATE OR ALTER PROCEDURE dbo.CRE_COMMERCE_PR
    @P_LegalId NVARCHAR(12), @P_Name NVARCHAR(100), @P_Phone NVARCHAR(8), @P_Email NVARCHAR(100),
    @P_Latitude DECIMAL(10,8), @P_Longitude DECIMAL(11,8), @P_Status NVARCHAR(15), @P_IBAN NVARCHAR(22),
    @P_NationalId NVARCHAR(9) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_Commerce (Created, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, NationalId)
    VALUES (GETDATE(), @P_LegalId, @P_Name, @P_Phone, @P_Email, @P_Latitude, @P_Longitude, @P_Status, @P_IBAN,
            ISNULL(@P_NationalId, ''));
END
GO

CREATE OR ALTER PROCEDURE dbo.CRE_COMMERCE_RETURN_ID_PR
    @P_LegalId NVARCHAR(12), @P_Name NVARCHAR(100), @P_Phone NVARCHAR(8), @P_Email NVARCHAR(100),
    @P_Latitude DECIMAL(10,8), @P_Longitude DECIMAL(11,8), @P_Status NVARCHAR(15), @P_IBAN NVARCHAR(22),
    @P_NationalId NVARCHAR(9) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_Commerce (Created, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, NationalId)
    VALUES (GETDATE(), @P_LegalId, @P_Name, @P_Phone, @P_Email, @P_Latitude, @P_Longitude, @P_Status, @P_IBAN,
            ISNULL(@P_NationalId, ''));
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_COMMERCE_BY_STATUS_PR @P_Status NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate,
           ISNULL(NationalId, '') AS NationalId
    FROM dbo.TBL_Commerce
    WHERE Status = @P_Status
    ORDER BY Created DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_COMMERCE_BY_ID_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate,
           ISNULL(NationalId, '') AS NationalId
    FROM dbo.TBL_Commerce
    WHERE Id = @P_Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_COMMERCE_BY_LEGAL_ID_PR @P_LegalId NVARCHAR(12)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate,
           ISNULL(NationalId, '') AS NationalId
    FROM dbo.TBL_Commerce
    WHERE LegalId = @P_LegalId;
END
GO

CREATE OR ALTER PROCEDURE dbo.RET_COMMERCE_BY_IBAN_PR @P_IBAN NVARCHAR(22)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate,
           ISNULL(NationalId, '') AS NationalId
    FROM dbo.TBL_Commerce
    WHERE IBAN = @P_IBAN;
END
GO

/* ---------------------------------------------------------------------
   Datos de prueba: 3 comercios activos (solo si no existen)
   --------------------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.TBL_Commerce WHERE LegalId = '3101000001')
    EXEC dbo.CRE_COMMERCE_PR @P_LegalId='3101000001', @P_Name='Soda La Esquina', @P_Phone='22220001',
         @P_Email='soda@billetico.com', @P_Latitude=9.9281, @P_Longitude=-84.0907,
         @P_Status='Activo', @P_IBAN='CR05015201001026284066', @P_NationalId='208600025';
IF NOT EXISTS (SELECT 1 FROM dbo.TBL_Commerce WHERE LegalId = '3101000002')
    EXEC dbo.CRE_COMMERCE_PR @P_LegalId='3101000002', @P_Name='Farmacia Central', @P_Phone='22220002',
         @P_Email='farmacia@billetico.com', @P_Latitude=9.9350, @P_Longitude=-84.0800,
         @P_Status='Activo', @P_IBAN='CR05015201001026284067', @P_NationalId='208600025';
IF NOT EXISTS (SELECT 1 FROM dbo.TBL_Commerce WHERE LegalId = '3101000003')
    EXEC dbo.CRE_COMMERCE_PR @P_LegalId='3101000003', @P_Name='Super Barrio', @P_Phone='22220003',
         @P_Email='super@billetico.com', @P_Latitude=9.9400, @P_Longitude=-84.1000,
         @P_Status='Activo', @P_IBAN='CR05015201001026284068', @P_NationalId='208600025';
UPDATE dbo.TBL_Commerce SET CommissionRate = 2.50 WHERE CommissionRate IS NULL;

SELECT Id, Name, Status, NationalId, CommissionRate FROM dbo.TBL_Commerce;
GO
