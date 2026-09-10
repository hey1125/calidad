-- ========================================
-- Procedimiento: CRE_COMMERCE_PR
-- Descripción : Crea un nuevo comercio en la tabla TBL_Commerce
-- ========================================
CREATE PROCEDURE CRE_COMMERCE_PR
    @P_LegalId NVARCHAR(12),         -- Identificación legal del comercio
    @P_Name NVARCHAR(30),            -- Nombre del comercio
    @P_Phone NVARCHAR(8),            -- Teléfono del comercio
    @P_Email NVARCHAR(100),          -- Correo electrónico del comercio
    @P_Latitude DECIMAL(9,6),        -- Latitud geográfica
    @P_Longitude DECIMAL(9,6),       -- Longitud geográfica
    @P_Status NVARCHAR(10),          -- Estado inicial del comercio
    @P_IBAN NVARCHAR(22)             -- Número IBAN de la cuenta bancaria
AS
BEGIN
    -- Inserta un nuevo registro en TBL_Commerce con la fecha actual
    INSERT INTO TBL_Commerce (Created, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN)
    VALUES (GETDATE(), @P_LegalId, @P_Name, @P_Phone, @P_Email, @P_Latitude, @P_Longitude, @P_Status, @P_IBAN)
END
GO

-- ========================================
-- Procedimiento: RET_COMMERCE_BY_STATUS_PR
-- Descripción : Retorna todos los comercios filtrados por estado
-- ========================================
CREATE PROCEDURE RET_COMMERCE_BY_STATUS_PR
    @P_Status NVARCHAR(15)           -- Estado por el cual filtrar (ej. 'Activo')
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate
    FROM TBL_Commerce
    WHERE Status = @P_Status
    ORDER BY Created DESC            -- Ordena los resultados por fecha de creación (más reciente primero)
END
GO

-- ========================================
-- Procedimiento: RET_COMMERCE_BY_ID_PR
-- Descripción : Retorna los detalles de un comercio específico por su Id
-- ========================================
CREATE PROCEDURE RET_COMMERCE_BY_ID_PR
    @P_Id INT                        -- Identificador único del comercio
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate
    FROM TBL_Commerce
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: APPROVE_COMMERCE_PR
-- Descripción : Aprueba un comercio, asignando tasa de comisión y cambiando el estado a 'Activo'
-- ========================================
CREATE PROCEDURE APPROVE_COMMERCE_PR
    @P_Id INT,                       -- Identificador del comercio
    @P_CommissionRate DECIMAL(5,2)  -- Tasa de comisión a aplicar
AS
BEGIN
    UPDATE TBL_Commerce
    SET Status = 'Activo',
        CommissionRate = @P_CommissionRate,
        Updated = GETDATE()         -- Fecha de modificación registrada
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: REJECT_COMMERCE_PR
-- Descripción : Rechaza un comercio, cambiando su estado a 'Rechazado'
-- ========================================
CREATE PROCEDURE REJECT_COMMERCE_PR
    @P_Id INT                        -- Identificador del comercio
AS
BEGIN
    UPDATE TBL_Commerce
    SET Status = 'Rechazado',
        Updated = GETDATE()         -- Fecha de modificación registrada
    WHERE Id = @P_Id
END
GO

-- ========================================
-- Procedimiento: RET_COMMERCE_BY_LEGAL_ID_PR
-- Descripción : Retorna un comercio según su identificación legal
-- ========================================
CREATE PROCEDURE RET_COMMERCE_BY_LEGAL_ID_PR
    @P_LegalId NVARCHAR(12)         -- Identificación legal del comercio
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate
    FROM TBL_Commerce
    WHERE LegalId = @P_LegalId
END
GO

-- ========================================
-- Procedimiento: RET_COMMERCE_BY_IBAN_PR
-- Descripción : Retorna un comercio según su IBAN
-- ========================================

CREATE PROCEDURE RET_COMMERCE_BY_IBAN_PR
    @P_IBAN NVARCHAR(22)         -- Cuenta IBAN del comercio
AS
BEGIN
    SELECT Id, Created, Updated, LegalId, Name, Phone, Email, Latitude, Longitude, Status, IBAN, CommissionRate
    FROM TBL_Commerce
    WHERE IBAN = @P_IBAN
END
GO

-- ========================================
-- Procedimiento: CRE_COMMERCE_RETURN_ID_PR
-- Descripción : Retorna del ID autogenerado del nuevo comercio.
-- ========================================

CREATE PROCEDURE CRE_COMMERCE_RETURN_ID_PR
    @P_LegalId NVARCHAR(12),
    @P_Name NVARCHAR(100),
    @P_Phone NVARCHAR(8),
    @P_Email NVARCHAR(100),
    @P_Latitude DECIMAL(10,8),
    @P_Longitude DECIMAL(11,8),
    @P_Status NVARCHAR(15),
    @P_IBAN NVARCHAR(22)
AS
BEGIN
    INSERT INTO TBL_Commerce (
        LegalId,
        Name,
        Phone,
        Email,
        Latitude,
        Longitude,
        Status,
        IBAN,
        Created
    )
    VALUES (
        @P_LegalId,
        @P_Name,
        @P_Phone,
        @P_Email,
        @P_Latitude,
        @P_Longitude,
        @P_Status,
        @P_IBAN,
        GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS ID;
END
GO

-- ========================================
-- Procedimiento: CRE_FINALUSER_COMMERCE_ROL_PR
-- Descripción : Retorna del ID autogenerado del nuevo comercio.
-- ========================================

CREATE PROCEDURE CRE_FINALUSER_COMMERCE_ROL_PR
    @P_UserId INT,
    @P_CommerceId INT,
    @P_RolId INT
AS
BEGIN
    INSERT INTO TBL_FinalUserCommerce_Rol (
        UserId,
        CommerceId,
        RolId,
        Created
    )
    VALUES (
        @P_UserId,
        @P_CommerceId,
        @P_RolId,
        GETDATE()
    );
END
GO

-- ========================================
-- Procedimiento:  RET_COMMERCES_BY_USER_PR
-- Descripción : Retorna del ID autogenerado del nuevo comercio.
-- ========================================

CREATE PROCEDURE RET_COMMERCES_BY_USER_PR
    @P_UserId INT
AS
BEGIN
    SELECT C.*
    FROM TBL_Commerce C
    INNER JOIN TBL_FinalUserCommerce_Rol R ON C.Id = R.CommerceId
    WHERE R.UserId = @P_UserId
END;
