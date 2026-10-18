/* =====================================================================
   BilleTico - Objetos de base de datos que el codigo usa y que NO
   estan en "DataAccess/Database Scripts" del repositorio.

   Reconstruido a partir del codigo C# (CrudFactory + DTOs):
   nombres de parametros (@P_...) y nombres de columnas que leen los
   Build...() de cada CrudFactory.

   SEGURO DE CORRER VARIAS VECES:
   - Las tablas solo se crean si no existen.
   - Cada procedimiento solo se crea si NO existe (si un companero ya
     lo creo, se respeta el suyo y se salta).

   Orden recomendado: correr primero 00_DIAGNOSTICO.sql, luego este,
   y despues 00_DIAGNOSTICO.sql otra vez (debe salir vacio).
   ===================================================================== */
USE BilleTico;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =====================================================================
   1. TABLAS
   ===================================================================== */

IF OBJECT_ID('dbo.TBL_CommercePromotion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TBL_CommercePromotion (
        Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TBL_CommercePromotion PRIMARY KEY,
        Created          DATETIME       NOT NULL CONSTRAINT DF_TBL_CommercePromotion_Created DEFAULT (GETDATE()),
        Updated          DATETIME       NULL,
        CoPromoId        NVARCHAR(20)   NOT NULL CONSTRAINT UQ_TBL_CommercePromotion_CoPromoId UNIQUE,
        CommerceId       INT            NOT NULL CONSTRAINT FK_TBL_CommercePromotion_Commerce REFERENCES dbo.TBL_Commerce(Id),
        StartDate        DATETIME       NOT NULL,
        EndDate          DATETIME       NOT NULL,
        MinAmount        DECIMAL(18,2)  NULL,
        AmountPercentage DECIMAL(5,2)   NULL,
        AmountDiscount   DECIMAL(18,2)  NULL,
        TotalCoupon      INT            NULL,
        UsedCoupon       INT            NULL CONSTRAINT DF_TBL_CommercePromotion_UsedCoupon DEFAULT (0),
        Status           NVARCHAR(15)   NOT NULL,
        Description      NVARCHAR(250)  NULL,
        CONSTRAINT CK_TBL_CommercePromotion_Dates CHECK (EndDate >= StartDate)
    );
    CREATE INDEX IX_TBL_CommercePromotion_Commerce ON dbo.TBL_CommercePromotion (CommerceId, Status, StartDate, EndDate);
    PRINT 'Creada tabla TBL_CommercePromotion';
END
GO

IF OBJECT_ID('dbo.TBL_FinancialEntityPromotion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TBL_FinancialEntityPromotion (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TBL_FinancialEntityPromotion PRIMARY KEY,
        Created           DATETIME       NOT NULL CONSTRAINT DF_TBL_FinancialEntityPromotion_Created DEFAULT (GETDATE()),
        Updated           DATETIME       NULL,
        FePromoId         NVARCHAR(20)   NOT NULL CONSTRAINT UQ_TBL_FinancialEntityPromotion_FePromoId UNIQUE,
        FinancialEntityId INT            NOT NULL CONSTRAINT FK_TBL_FinancialEntityPromotion_FE REFERENCES dbo.TBL_FinancialEntity(Id),
        StartDate         DATETIME       NOT NULL,
        EndDate           DATETIME       NOT NULL,
        MinAmount         DECIMAL(18,2)  NULL,
        AmountPercentage  DECIMAL(5,2)   NULL,
        AmountDiscount    DECIMAL(18,2)  NULL,
        TotalCoupon       INT            NULL,
        UsedCoupon        INT            NULL CONSTRAINT DF_TBL_FinancialEntityPromotion_UsedCoupon DEFAULT (0),
        Status            NVARCHAR(15)   NOT NULL,
        Description       NVARCHAR(250)  NULL,
        CONSTRAINT CK_TBL_FinancialEntityPromotion_Dates CHECK (EndDate >= StartDate)
    );
    CREATE INDEX IX_TBL_FinancialEntityPromotion_FE ON dbo.TBL_FinancialEntityPromotion (FinancialEntityId, Status, StartDate, EndDate);
    PRINT 'Creada tabla TBL_FinancialEntityPromotion';
END
GO

IF OBJECT_ID('dbo.TBL_Payment', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TBL_Payment (
        Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TBL_Payment PRIMARY KEY,
        Created           DATETIME       NOT NULL CONSTRAINT DF_TBL_Payment_Created DEFAULT (GETDATE()),
        Updated           DATETIME       NULL,
        PaymentId         NVARCHAR(25)   NOT NULL CONSTRAINT UQ_TBL_Payment_PaymentId UNIQUE,
        NationalId        VARCHAR(9)     NOT NULL,
        CommerceId        INT            NOT NULL CONSTRAINT FK_TBL_Payment_Commerce REFERENCES dbo.TBL_Commerce(Id),
        IBAN              NVARCHAR(22)   NULL,           -- NULL mientras esta Pendiente
        CoPromoId         NVARCHAR(20)   NULL,
        FePromoId         NVARCHAR(20)   NULL,
        PaymentDate       DATETIME       NOT NULL,
        GrossAmount       DECIMAL(18,2)  NOT NULL,
        AmountWDiscount   DECIMAL(18,2)  NULL,           -- monto del descuento aplicado
        CoCommisionAmount DECIMAL(18,2)  NULL,
        FeCommisionAmount DECIMAL(18,2)  NULL,
        -- Calculados por la BD (el DTO dice "CAMPOS CALCULADOS AUTOMATICAMENTE POR LA BD")
        NetAmount         AS CAST(GrossAmount - ISNULL(AmountWDiscount, 0) AS DECIMAL(18,2)) PERSISTED,
        CommerceProfit    AS CAST(GrossAmount - ISNULL(AmountWDiscount, 0)
                                  - ISNULL(CoCommisionAmount, 0) - ISNULL(FeCommisionAmount, 0) AS DECIMAL(18,2)) PERSISTED,
        Status            NVARCHAR(15)   NOT NULL CONSTRAINT DF_TBL_Payment_Status DEFAULT ('Pendiente'),
        Description       NVARCHAR(250)  NULL,
        CONSTRAINT CK_TBL_Payment_OnePromo CHECK (CoPromoId IS NULL OR FePromoId IS NULL)
    );
    CREATE INDEX IX_TBL_Payment_NationalId ON dbo.TBL_Payment (NationalId, PaymentDate);
    CREATE INDEX IX_TBL_Payment_Commerce   ON dbo.TBL_Payment (CommerceId, PaymentDate);
    PRINT 'Creada tabla TBL_Payment';
END
GO

/* =====================================================================
   2. PROCEDIMIENTOS - PAGOS (PaymentCrudFactory)
   Patron: si ya existe -> SET NOEXEC ON (se salta el CREATE)
   ===================================================================== */

IF OBJECT_ID('dbo.CRE_PAYMENT_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.CRE_PAYMENT_PR
    @P_PaymentId NVARCHAR(25), @P_NationalId VARCHAR(9), @P_CommerceId INT,
    @P_IBAN NVARCHAR(22) = NULL, @P_CoPromoId NVARCHAR(20) = NULL, @P_FePromoId NVARCHAR(20) = NULL,
    @P_PaymentDate DATETIME, @P_GrossAmount DECIMAL(18,2),
    @P_AmountWDiscount DECIMAL(18,2) = 0, @P_CoCommisionAmount DECIMAL(18,2) = 0,
    @P_FeCommisionAmount DECIMAL(18,2) = NULL,
    @P_Status NVARCHAR(15) = 'Pendiente', @P_Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_Payment (PaymentId, NationalId, CommerceId, IBAN, CoPromoId, FePromoId, PaymentDate,
                                 GrossAmount, AmountWDiscount, CoCommisionAmount, FeCommisionAmount, Status, Description)
    VALUES (@P_PaymentId, @P_NationalId, @P_CommerceId, NULLIF(@P_IBAN, ''), NULLIF(@P_CoPromoId, ''), NULLIF(@P_FePromoId, ''),
            @P_PaymentDate, @P_GrossAmount, @P_AmountWDiscount, @P_CoCommisionAmount, @P_FeCommisionAmount,
            ISNULL(@P_Status, 'Pendiente'), @P_Description);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.UPD_PAYMENT_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.UPD_PAYMENT_PR @P_Id INT, @P_Status NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TBL_Payment SET Status = @P_Status, Updated = GETDATE() WHERE Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_PAYMENT_BY_ID_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_PAYMENT_BY_ID_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.* FROM dbo.TBL_Payment p WHERE p.Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.DEL_PAYMENT_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.DEL_PAYMENT_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TBL_Payment WHERE Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_ALL_PAYMENTS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_ALL_PAYMENTS_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.* FROM dbo.TBL_Payment p ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.GET_USER_PENDING_PAYMENTS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_USER_PENDING_PAYMENTS_PR @P_NationalId VARCHAR(9)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, c.Name AS CommerceName
    FROM dbo.TBL_Payment p
    INNER JOIN dbo.TBL_Commerce c ON c.Id = p.CommerceId
    WHERE p.NationalId = @P_NationalId AND p.Status = 'Pendiente'
    ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

-- Historial (solo los "Pagado"; el C# filtra igual). Usado por la pagina "Historial de pagos".
IF OBJECT_ID('dbo.GET_USER_PAYMENT_HISTORY_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_USER_PAYMENT_HISTORY_PR
    @P_NationalId VARCHAR(9), @P_StartDate DATETIME = NULL, @P_EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, c.Name AS CommerceName,
           COALESCE(cp.Description, fp.Description) AS PromotionDescription
    FROM dbo.TBL_Payment p
    INNER JOIN dbo.TBL_Commerce c ON c.Id = p.CommerceId
    LEFT JOIN dbo.TBL_CommercePromotion cp ON cp.CoPromoId = p.CoPromoId
    LEFT JOIN dbo.TBL_FinancialEntityPromotion fp ON fp.FePromoId = p.FePromoId
    WHERE p.NationalId = @P_NationalId
      AND p.Status = 'Pagado'
      AND (@P_StartDate IS NULL OR p.PaymentDate >= @P_StartDate)
      AND (@P_EndDate   IS NULL OR p.PaymentDate <  DATEADD(DAY, 1, CAST(@P_EndDate AS DATE)))
    ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

-- Historial completo (todos los estados). Usado por la pagina de pagos del usuario.
IF OBJECT_ID('dbo.GET_USER_PAYMENT_HISTORY_MODULE_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_USER_PAYMENT_HISTORY_MODULE_PR
    @P_NationalId VARCHAR(9), @P_StartDate DATETIME = NULL, @P_EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, c.Name AS CommerceName,
           COALESCE(cp.Description, fp.Description) AS PromotionDescription
    FROM dbo.TBL_Payment p
    INNER JOIN dbo.TBL_Commerce c ON c.Id = p.CommerceId
    LEFT JOIN dbo.TBL_CommercePromotion cp ON cp.CoPromoId = p.CoPromoId
    LEFT JOIN dbo.TBL_FinancialEntityPromotion fp ON fp.FePromoId = p.FePromoId
    WHERE p.NationalId = @P_NationalId
      AND (@P_StartDate IS NULL OR p.PaymentDate >= @P_StartDate)
      AND (@P_EndDate   IS NULL OR p.PaymentDate <  DATEADD(DAY, 1, CAST(@P_EndDate AS DATE)))
    ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.GET_COMMERCE_RECEIVED_PAYMENTS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_COMMERCE_RECEIVED_PAYMENTS_PR
    @P_CommerceId INT, @P_StartDate DATETIME = NULL, @P_EndDate DATETIME = NULL, @P_Status NVARCHAR(15) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*,
           LTRIM(RTRIM(u.FirstName + ' ' + u.LastName1 + ' ' + ISNULL(u.LastName2, ''))) AS PayerName,
           COALESCE(cp.Description, fp.Description) AS PromotionDescription
    FROM dbo.TBL_Payment p
    LEFT JOIN dbo.TBL_FinalUser u ON u.NationalId = p.NationalId
    LEFT JOIN dbo.TBL_CommercePromotion cp ON cp.CoPromoId = p.CoPromoId
    LEFT JOIN dbo.TBL_FinancialEntityPromotion fp ON fp.FePromoId = p.FePromoId
    WHERE p.CommerceId = @P_CommerceId
      AND (@P_Status    IS NULL OR p.Status = @P_Status)
      AND (@P_StartDate IS NULL OR p.PaymentDate >= @P_StartDate)
      AND (@P_EndDate   IS NULL OR p.PaymentDate <  DATEADD(DAY, 1, CAST(@P_EndDate AS DATE)))
    ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

-- Codigo de banco = caracteres 6-8 del IBAN (igual que BankAccountManager: IBAN.Substring(5,3))
IF OBJECT_ID('dbo.GET_BANK_PAYMENTS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_BANK_PAYMENTS_PR
    @P_BankCode NVARCHAR(3), @P_StartDate DATETIME = NULL, @P_EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, c.Name AS CommerceName
    FROM dbo.TBL_Payment p
    INNER JOIN dbo.TBL_Commerce c ON c.Id = p.CommerceId
    WHERE SUBSTRING(p.IBAN, 6, 3) = @P_BankCode
      AND (@P_StartDate IS NULL OR p.PaymentDate >= @P_StartDate)
      AND (@P_EndDate   IS NULL OR p.PaymentDate <  DATEADD(DAY, 1, CAST(@P_EndDate AS DATE)))
    ORDER BY p.PaymentDate DESC;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.GET_BANK_STATS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.GET_BANK_STATS_PR @P_BankCode NVARCHAR(3)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*)                                                    AS TotalPayments,
           ISNULL(SUM(p.NetAmount), 0)                                 AS TotalAmount,
           ISNULL(SUM(ISNULL(p.FeCommisionAmount, 0)), 0)              AS TotalCommissions,
           SUM(CASE WHEN p.Status = 'Pendiente' THEN 1 ELSE 0 END)     AS PendingPayments,
           SUM(CASE WHEN p.Status = 'Pagado'    THEN 1 ELSE 0 END)     AS CompletedPayments
    FROM dbo.TBL_Payment p
    WHERE SUBSTRING(p.IBAN, 6, 3) = @P_BankCode;
END
GO
SET NOEXEC OFF;
GO

-- Confirma un cobro pendiente: aplica la promo (si viene), calcula comisiones,
-- consume un cupon y lo marca como Pagado. Devuelve el pago actualizado.
IF OBJECT_ID('dbo.CONFIRM_PAYMENT_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.CONFIRM_PAYMENT_PR
    @P_Id INT, @P_IBAN NVARCHAR(22),
    @P_CoPromoId NVARCHAR(20) = NULL, @P_FePromoId NVARCHAR(20) = NULL,
    @P_RefDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @P_CoPromoId = NULLIF(@P_CoPromoId, '');
    SET @P_FePromoId = NULLIF(@P_FePromoId, '');
    SET @P_RefDate   = ISNULL(@P_RefDate, GETDATE());

    DECLARE @Gross DECIMAL(18,2), @CommerceId INT, @Status NVARCHAR(15);
    SELECT @Gross = GrossAmount, @CommerceId = CommerceId, @Status = Status
    FROM dbo.TBL_Payment WHERE Id = @P_Id;

    IF @Gross IS NULL      BEGIN RAISERROR('Pago no encontrado.', 16, 1); RETURN; END
    IF @Status <> 'Pendiente' BEGIN RAISERROR('El pago no esta pendiente.', 16, 1); RETURN; END

    DECLARE @Pct DECIMAL(5,2) = NULL, @Fix DECIMAL(18,2) = NULL, @Discount DECIMAL(18,2) = 0;

    IF @P_CoPromoId IS NOT NULL
        SELECT @Pct = AmountPercentage, @Fix = AmountDiscount
        FROM dbo.TBL_CommercePromotion
        WHERE CoPromoId = @P_CoPromoId AND CommerceId = @CommerceId AND Status = 'Activa'
          AND StartDate <= @P_RefDate AND EndDate >= CAST(GETDATE() AS DATE)
          AND (MinAmount IS NULL OR MinAmount <= @Gross)
          AND (TotalCoupon IS NULL OR ISNULL(UsedCoupon, 0) < TotalCoupon);
    ELSE IF @P_FePromoId IS NOT NULL
        SELECT @Pct = fp.AmountPercentage, @Fix = fp.AmountDiscount
        FROM dbo.TBL_FinancialEntityPromotion fp
        INNER JOIN dbo.TBL_FinancialEntity fe ON fe.Id = fp.FinancialEntityId
        WHERE fp.FePromoId = @P_FePromoId AND fe.BankCode = SUBSTRING(@P_IBAN, 6, 3) AND fp.Status = 'Activa'
          AND fp.StartDate <= @P_RefDate AND fp.EndDate >= CAST(GETDATE() AS DATE)
          AND (fp.MinAmount IS NULL OR fp.MinAmount <= @Gross)
          AND (fp.TotalCoupon IS NULL OR ISNULL(fp.UsedCoupon, 0) < fp.TotalCoupon);

    -- Misma regla que CommercePromotionManager.ComputeEffectiveDiscount: max(%, fijo), tope = monto
    SET @Discount = CASE WHEN ISNULL(@Gross * ISNULL(@Pct, 0) / 100, 0) > ISNULL(@Fix, 0)
                         THEN ISNULL(@Gross * ISNULL(@Pct, 0) / 100, 0) ELSE ISNULL(@Fix, 0) END;
    IF @Discount > @Gross SET @Discount = @Gross;
    IF @Pct IS NULL AND @Fix IS NULL BEGIN SET @P_CoPromoId = NULL; SET @P_FePromoId = NULL; END

    DECLARE @Net DECIMAL(18,2) = @Gross - @Discount;
    DECLARE @CoRate DECIMAL(5,2) = (SELECT ISNULL(CommissionRate, 0) FROM dbo.TBL_Commerce WHERE Id = @CommerceId);
    DECLARE @FeRate DECIMAL(5,2) = (SELECT TOP 1 ISNULL(CommissionRate, 0) FROM dbo.TBL_FinancialEntity
                                    WHERE BankCode = SUBSTRING(@P_IBAN, 6, 3));

    BEGIN TRAN;
        UPDATE dbo.TBL_Payment
        SET IBAN = @P_IBAN, CoPromoId = @P_CoPromoId, FePromoId = @P_FePromoId,
            AmountWDiscount   = ROUND(@Discount, 2),
            CoCommisionAmount = ROUND(@Net * ISNULL(@CoRate, 0) / 100, 2),
            FeCommisionAmount = ROUND(@Net * ISNULL(@FeRate, 0) / 100, 2),
            Status = 'Pagado', Updated = GETDATE()
        WHERE Id = @P_Id;

        IF @P_CoPromoId IS NOT NULL
            UPDATE dbo.TBL_CommercePromotion SET UsedCoupon = ISNULL(UsedCoupon, 0) + 1, Updated = GETDATE()
            WHERE CoPromoId = @P_CoPromoId;
        IF @P_FePromoId IS NOT NULL
            UPDATE dbo.TBL_FinancialEntityPromotion SET UsedCoupon = ISNULL(UsedCoupon, 0) + 1, Updated = GETDATE()
            WHERE FePromoId = @P_FePromoId;
    COMMIT;

    SELECT p.* FROM dbo.TBL_Payment p WHERE p.Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

/* =====================================================================
   3. PROCEDIMIENTOS - PROMOCIONES DE COMERCIO (CommercePromotionCrudFactory)
   Estados usados por el codigo: 'Activa' / 'Inactiva'
   ===================================================================== */

IF OBJECT_ID('dbo.CRE_COMMERCE_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.CRE_COMMERCE_PROMOTION_PR
    @P_CoPromoId NVARCHAR(20), @P_CommerceId INT, @P_StartDate DATETIME, @P_EndDate DATETIME,
    @P_Status NVARCHAR(15), @P_MinAmount DECIMAL(18,2) = NULL, @P_AmountPercentage DECIMAL(5,2) = NULL,
    @P_AmountDiscount DECIMAL(18,2) = NULL, @P_TotalCoupon INT = NULL, @P_UsedCoupon INT = NULL,
    @P_Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_CommercePromotion (CoPromoId, CommerceId, StartDate, EndDate, MinAmount, AmountPercentage,
                                           AmountDiscount, TotalCoupon, UsedCoupon, Status, Description)
    VALUES (@P_CoPromoId, @P_CommerceId, @P_StartDate, @P_EndDate, @P_MinAmount, @P_AmountPercentage,
            @P_AmountDiscount, @P_TotalCoupon, ISNULL(@P_UsedCoupon, 0), @P_Status, @P_Description);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.UPD_COMMERCE_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.UPD_COMMERCE_PROMOTION_PR
    @P_Id INT, @P_CoPromoId NVARCHAR(20), @P_CommerceId INT, @P_StartDate DATETIME, @P_EndDate DATETIME,
    @P_Status NVARCHAR(15), @P_MinAmount DECIMAL(18,2) = NULL, @P_AmountPercentage DECIMAL(5,2) = NULL,
    @P_AmountDiscount DECIMAL(18,2) = NULL, @P_TotalCoupon INT = NULL, @P_UsedCoupon INT = NULL,
    @P_Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TBL_CommercePromotion
    SET CoPromoId = @P_CoPromoId, CommerceId = @P_CommerceId, StartDate = @P_StartDate, EndDate = @P_EndDate,
        Status = @P_Status, MinAmount = @P_MinAmount, AmountPercentage = @P_AmountPercentage,
        AmountDiscount = @P_AmountDiscount, TotalCoupon = @P_TotalCoupon,
        UsedCoupon = ISNULL(@P_UsedCoupon, UsedCoupon), Description = @P_Description, Updated = GETDATE()
    WHERE Id = @P_Id;
    SELECT @P_Id AS UpdatedId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.DEL_COMMERCE_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.DEL_COMMERCE_PROMOTION_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TBL_CommercePromotion WHERE Id = @P_Id;
    SELECT @P_Id AS DeletedId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_COMMERCE_PROMOTION_BY_ID_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_COMMERCE_PROMOTION_BY_ID_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TBL_CommercePromotion WHERE Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_COMMERCE_PROMOTIONS_BY_COMMERCE_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_COMMERCE_PROMOTIONS_BY_COMMERCE_PR @P_CommerceId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TBL_CommercePromotion WHERE CommerceId = @P_CommerceId ORDER BY StartDate DESC;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_ACTIVE_COMMERCE_PROMOTIONS_BY_COMMERCE_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_ACTIVE_COMMERCE_PROMOTIONS_BY_COMMERCE_PR
    @P_CommerceId INT, @P_Amount FLOAT, @P_RefDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Ref DATETIME = ISNULL(@P_RefDate, GETDATE());
    SELECT *
    FROM dbo.TBL_CommercePromotion
    WHERE CommerceId = @P_CommerceId
      AND Status = 'Activa'
      AND @Ref BETWEEN StartDate AND DATEADD(DAY, 1, CAST(EndDate AS DATE))
      AND (MinAmount IS NULL OR MinAmount <= @P_Amount)
      AND (TotalCoupon IS NULL OR ISNULL(UsedCoupon, 0) < TotalCoupon)
    ORDER BY EndDate;
END
GO
SET NOEXEC OFF;
GO

/* =====================================================================
   4. PROCEDIMIENTOS - PROMOCIONES DE ENTIDAD FINANCIERA
   ===================================================================== */

IF OBJECT_ID('dbo.CRE_FINANCIAL_ENTITY_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.CRE_FINANCIAL_ENTITY_PROMOTION_PR
    @P_FePromoId NVARCHAR(20), @P_FinancialEntityId INT, @P_StartDate DATETIME, @P_EndDate DATETIME,
    @P_Status NVARCHAR(15), @P_MinAmount DECIMAL(18,2) = NULL, @P_AmountPercentage DECIMAL(5,2) = NULL,
    @P_AmountDiscount DECIMAL(18,2) = NULL, @P_TotalCoupon INT = NULL, @P_UsedCoupon INT = NULL,
    @P_Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_FinancialEntityPromotion (FePromoId, FinancialEntityId, StartDate, EndDate, MinAmount,
                                                  AmountPercentage, AmountDiscount, TotalCoupon, UsedCoupon, Status, Description)
    VALUES (@P_FePromoId, @P_FinancialEntityId, @P_StartDate, @P_EndDate, @P_MinAmount,
            @P_AmountPercentage, @P_AmountDiscount, @P_TotalCoupon, ISNULL(@P_UsedCoupon, 0), @P_Status, @P_Description);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.UPD_FINANCIAL_ENTITY_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.UPD_FINANCIAL_ENTITY_PROMOTION_PR
    @P_Id INT, @P_FePromoId NVARCHAR(20), @P_FinancialEntityId INT, @P_StartDate DATETIME, @P_EndDate DATETIME,
    @P_Status NVARCHAR(15), @P_MinAmount DECIMAL(18,2) = NULL, @P_AmountPercentage DECIMAL(5,2) = NULL,
    @P_AmountDiscount DECIMAL(18,2) = NULL, @P_TotalCoupon INT = NULL, @P_UsedCoupon INT = NULL,
    @P_Description NVARCHAR(250) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TBL_FinancialEntityPromotion
    SET FePromoId = @P_FePromoId, FinancialEntityId = @P_FinancialEntityId, StartDate = @P_StartDate,
        EndDate = @P_EndDate, Status = @P_Status, MinAmount = @P_MinAmount, AmountPercentage = @P_AmountPercentage,
        AmountDiscount = @P_AmountDiscount, TotalCoupon = @P_TotalCoupon,
        UsedCoupon = ISNULL(@P_UsedCoupon, UsedCoupon), Description = @P_Description, Updated = GETDATE()
    WHERE Id = @P_Id;
    SELECT @P_Id AS UpdatedId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.DEL_FINANCIAL_ENTITY_PROMOTION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.DEL_FINANCIAL_ENTITY_PROMOTION_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TBL_FinancialEntityPromotion WHERE Id = @P_Id;
    SELECT @P_Id AS DeletedId;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_FE_PROMOTION_BY_ID_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_FE_PROMOTION_BY_ID_PR @P_Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TBL_FinancialEntityPromotion WHERE Id = @P_Id;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_FE_PROMOTIONS_BY_ENTITY_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_FE_PROMOTIONS_BY_ENTITY_PR @P_FinancialEntityId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TBL_FinancialEntityPromotion WHERE FinancialEntityId = @P_FinancialEntityId ORDER BY StartDate DESC;
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_ACTIVE_FE_PROMOTIONS_BY_IBAN_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_ACTIVE_FE_PROMOTIONS_BY_IBAN_PR
    @P_IBAN NVARCHAR(22), @P_Amount FLOAT, @P_RefDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Ref DATETIME = ISNULL(@P_RefDate, GETDATE());
    SELECT fp.*
    FROM dbo.TBL_FinancialEntityPromotion fp
    INNER JOIN dbo.TBL_FinancialEntity fe ON fe.Id = fp.FinancialEntityId
    WHERE fe.BankCode = SUBSTRING(@P_IBAN, 6, 3)
      AND fp.Status = 'Activa'
      AND fp.StartDate <= @Ref
      AND fp.EndDate >= CAST(GETDATE() AS DATE)
      AND (fp.MinAmount IS NULL OR fp.MinAmount <= @P_Amount)
      AND (fp.TotalCoupon IS NULL OR ISNULL(fp.UsedCoupon, 0) < fp.TotalCoupon)
    ORDER BY fp.EndDate;
END
GO
SET NOEXEC OFF;
GO

/* =====================================================================
   5. PROCEDIMIENTOS - OTP (la tabla TBL_OTP_Verification ya tiene script)
   ===================================================================== */

IF OBJECT_ID('dbo.CRE_OTP_VERIFICATION_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.CRE_OTP_VERIFICATION_PR
    @P_UserId INT, @P_VerificationType VARCHAR(10), @P_Code VARCHAR(6),
    @P_ExpiresAt DATETIME2, @P_IsVerified BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.TBL_OTP_Verification (UserId, VerificationType, Code, ExpiresAt, IsVerified)
    VALUES (@P_UserId, @P_VerificationType, @P_Code, @P_ExpiresAt, ISNULL(@P_IsVerified, 0));
END
GO
SET NOEXEC OFF;
GO

IF OBJECT_ID('dbo.RET_ALL_OTP_VERIFICATIONS_PR','P') IS NOT NULL SET NOEXEC ON;
GO
CREATE PROCEDURE dbo.RET_ALL_OTP_VERIFICATIONS_PR
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TBL_OTP_Verification ORDER BY CreatedAt DESC;
END
GO
SET NOEXEC OFF;
GO

PRINT '=== Listo. Corra 00_DIAGNOSTICO.sql de nuevo: la primera consulta debe salir vacia. ===';
GO
