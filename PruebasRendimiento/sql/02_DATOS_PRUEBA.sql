/* =====================================================================
   BilleTico - Datos de prueba para la prueba de rendimiento (opcional)
   Crea pagos para los usuarios del CSV y una promocion activa, para que
   los servicios 03 y 05 devuelvan datos reales y no listas vacias.
   Requiere: haber corrido 01_CREAR_FALTANTES.sql y tener al menos un
   comercio en TBL_Commerce.
   ===================================================================== */
USE BilleTico;
GO
SET NOCOUNT ON;

DECLARE @CommerceId INT =
    (SELECT TOP 1 Id FROM dbo.TBL_Commerce WHERE Status IN ('Activo','Active','Activa') ORDER BY Id);
IF @CommerceId IS NULL SET @CommerceId = (SELECT TOP 1 Id FROM dbo.TBL_Commerce ORDER BY Id);
IF @CommerceId IS NULL
BEGIN
    RAISERROR('No hay comercios en TBL_Commerce. Registre uno desde la app y vuelva a correr este script.', 16, 1);
    RETURN;
END

-- 8 pagos por usuario del CSV, repartidos en el ultimo ano (solo si aun no tiene pagos JMETER)
DECLARE @Usuarios TABLE (NationalId VARCHAR(9));
INSERT INTO @Usuarios VALUES ('208600025'), ('999999999'), ('200000000');

DECLARE @n INT = 1;
WHILE @n <= 8
BEGIN
    INSERT INTO dbo.TBL_Payment (PaymentId, NationalId, CommerceId, IBAN, PaymentDate, GrossAmount,
                                 AmountWDiscount, CoCommisionAmount, FeCommisionAmount, Status, Description)
    SELECT CONCAT('JMETER', u.NationalId, RIGHT('0' + CAST(@n AS VARCHAR(2)), 2)),
           u.NationalId, @CommerceId,
           CASE WHEN @n % 3 = 0 THEN NULL ELSE 'CR05015201001026284066' END,
           DATEADD(DAY, -45 * @n, GETDATE()),
           1500 * @n, 0, 37.5 * @n, 7.5 * @n,
           CASE WHEN @n % 3 = 0 THEN 'Pendiente' ELSE 'Pagado' END,
           CONCAT('Pago de prueba JMeter #', @n)
    FROM @Usuarios u
    WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_Payment p
                      WHERE p.PaymentId = CONCAT('JMETER', u.NationalId, RIGHT('0' + CAST(@n AS VARCHAR(2)), 2)));
    SET @n += 1;
END

-- Una promocion activa del comercio (10% o 1000 colones, la mayor), vigente todo el ano
IF NOT EXISTS (SELECT 1 FROM dbo.TBL_CommercePromotion WHERE CoPromoId = 'JMETER-PROMO')
    INSERT INTO dbo.TBL_CommercePromotion (CoPromoId, CommerceId, StartDate, EndDate, MinAmount,
                                           AmountPercentage, AmountDiscount, TotalCoupon, UsedCoupon, Status, Description)
    VALUES ('JMETER-PROMO', @CommerceId, DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, 3, GETDATE()), 500,
            10, 1000, 100000, 0, 'Activa', 'Promo de prueba de rendimiento');

SELECT 'Pagos de prueba' AS Dato, COUNT(*) AS Cantidad FROM dbo.TBL_Payment WHERE PaymentId LIKE 'JMETER%'
UNION ALL
SELECT 'Comercio usado', @CommerceId
UNION ALL
SELECT 'Promos activas del comercio', COUNT(*) FROM dbo.TBL_CommercePromotion WHERE CommerceId = @CommerceId AND Status = 'Activa';
GO
