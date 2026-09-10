using Core.Managers;   // PaymentManager
using CoreApp;
using CoreApp.Classes;
using DataAccess.CRUD;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        // HU 7.1 - Historial de pagos del usuario final
        [HttpGet]
        [Route("GetUserHistory")]
        public ActionResult GetUserHistory(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalId))
                    return BadRequest("La cédula es requerida.");

                var pm = new PaymentManager();
                var payments = pm.GetUserPaymentHistory(nationalId.Trim(), startDate, endDate);

                return Ok(payments ?? new List<Payment>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo historial de usuario: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetUserHistoryPayment")]
        public ActionResult GetUserHistoryPayment(string nationalId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nationalId))
                    return BadRequest("La cédula es requerida.");

                var pm = new PaymentManager();
                var payments = pm.GetUserPaymentHistoryModule(nationalId.Trim(), startDate, endDate);

                return Ok(payments ?? new List<Payment>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo historial de usuario: {ex.Message}");
            }
        }

        // HU 7.2 - Pagos recibidos por comercio
        [HttpGet]
        [Route("GetCommercePayments")]
        public ActionResult GetCommercePayments(int commerceId, DateTime? startDate = null, DateTime? endDate = null, string? status = null)
        {
            try
            {
                if (commerceId <= 0)
                    return BadRequest("ID de comercio inválido.");

                var pm = new PaymentManager();
                var payments = pm.GetCommerceReceivedPayments(commerceId, startDate, endDate, status);

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo pagos del comercio: {ex.Message}");
            }
        }

        // HU 7.3 - Pagos por banco
        [HttpGet]
        [Route("GetBankPayments")]
        public ActionResult GetBankPayments(string bankCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bankCode) || bankCode.Length != 3)
                    return BadRequest("El código de banco debe tener 3 dígitos.");

                var pm = new PaymentManager();
                var payments = pm.GetBankPayments(bankCode.Trim(), startDate, endDate);

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo pagos del banco: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetBankStats")]
        public ActionResult GetBankStats(string bankCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bankCode) || bankCode.Length != 3)
                    return BadRequest("El código de banco debe tener 3 dígitos.");

                var pm = new PaymentManager();
                var stats = pm.GetBankStats(bankCode.Trim());

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo estadísticas del banco: {ex.Message}");
            }
        }

        // Crear cobro en estado 'Pendiente'

        [HttpPost]
        [Route("Create")]
        public ActionResult Create([FromBody] Payment payment)
        {
            // Cargar SendGrid API Key
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            SendGridSettings sendGridConfig = new SendGridSettings
            {
                ApiKey = config["SendGrid:ApiKey"]
            };

            try
            {
                if (payment == null)
                    return BadRequest("Datos del pago son requeridos.");

                if (string.IsNullOrWhiteSpace(payment.NationalId))
                    return BadRequest("La cédula del pagador es requerida.");

                if (payment.CommerceId <= 0)
                    return BadRequest("ID de comercio inválido.");

                if (payment.GrossAmount <= 0)
                    return BadRequest("El monto debe ser mayor a 0.");

                // === Reglas para COBRO (Pendiente) ===
                payment.IBAN = null;              // se elige al pagar
                payment.CoPromoId = null;         // se elige al pagar
                payment.FePromoId = null;         // se elige al pagar

                if (string.IsNullOrWhiteSpace(payment.Status))
                    payment.Status = "Pendiente";

                // Monto de descuento (en cobro pendiente debe ser 0)
                if (payment.AmountWDiscount == null || payment.AmountWDiscount < 0)
                    payment.AmountWDiscount = 0m;
                if (payment.AmountWDiscount > payment.GrossAmount)
                    payment.AmountWDiscount = payment.GrossAmount;

                // Comisión del comercio (si no vino, 0)
                payment.CoCommisionAmount ??= 0m;

                // Comisión financiera NO se calcula en cobro
                payment.FeCommisionAmount = null;

                var pm = new PaymentManager();
                pm.Create(payment); // genera PaymentId y PaymentDate (UTC)

                // ===== Notificar por correo al usuario (pagador) =====
                // 1) Buscar usuario por cédula (para obtener el email)
                var finalUserManager = new FinalUserManager();
                var finalUser = finalUserManager.RetrieveByNationalId(payment.NationalId);

                // 2) Buscar datos del comercio (nombre)
                var cCrud = new CommerceCrudFactory();
                var commerceDb = cCrud.RetrieveById<Commerce>(payment.CommerceId);

                if (finalUser != null && !string.IsNullOrWhiteSpace(finalUser.Email))
                {
                    var emailManager = new EmailManager(sendGridConfig.ApiKey);

                    // Preparar datos visuales
                    var cr = new System.Globalization.CultureInfo("es-CR");
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(
#if WINDOWS
                "Central America Standard Time"
#else
                        "America/Costa_Rica"
#endif
                    );
                    var createdLocal = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.SpecifyKind(payment.PaymentDate, DateTimeKind.Utc), tz);

                    string amount = payment.GrossAmount.ToString("C", cr);
                    string commerceName = commerceDb?.Name ?? "Comercio";

                    var subject = "Tienes un cobro pendiente en BilleTico";
                    var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Cobro pendiente</h2>
  <p>Hola {finalUser.FirstName},</p>
  <p>El comercio <strong>{commerceName}</strong> te ha enviado un cobro por <strong>{amount}</strong>.</p>
  <ul>
    <li><strong>ID del cobro:</strong> {payment.PaymentId}</li>
    <li><strong>Fecha de creación:</strong> {createdLocal:dd/MM/yyyy HH:mm}</li>
    <li><strong>Estado:</strong> Pendiente</li>
  </ul>
  <p style='margin-top:10px;color:#555'>
    Este es un recordatorio automático. Cuando estés listo para pagar, ingresa a BilleTico y continúa el proceso.
  </p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones</p>
</div>";

                    // Nombre del destinatario: el del usuario final
                    emailManager.SendEmail(finalUser.FirstName, finalUser.Email, subject, body);
                }

                return Ok(new { message = "Cobro registrado.", paymentId = payment.PaymentId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creando cobro: {ex.Message}");
            }
        }

        // Actualizar estado (ej. Pendiente -> Pagado/Rechazado)
        [HttpPut]
        [Route("UpdateStatus")]
        public ActionResult UpdateStatus([FromBody] Payment payment)
        {
            try
            {
                if (payment == null)
                    return BadRequest("Request requerido.");

                if (payment.Id <= 0)
                    return BadRequest("Id inválido.");

                if (string.IsNullOrWhiteSpace(payment.Status))
                    return BadRequest("Estado es requerido.");

                var pm = new PaymentManager();
                pm.UpdateStatus(payment.Id, payment.Status.Trim());

                return Ok(new { message = "Estado actualizado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error actualizando estado: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("Confirm")]
        public ActionResult Confirm([FromBody] ConfirmPaymentRequest req)
        {
            // Cargar SendGrid API Key
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            SendGridSettings sendGridConfig = new SendGridSettings
            {
                ApiKey = config["SendGrid:ApiKey"]
            };

            try
            {
                if (req == null) return BadRequest("Request requerido.");
                if (req.Id <= 0) return BadRequest("Id inválido.");
                if (string.IsNullOrWhiteSpace(req.IBAN)) return BadRequest("IBAN requerido.");
                if (!Regex.IsMatch(req.IBAN, @"^[A-Z]{2}\d{20}$")) return BadRequest("IBAN inválido (CR + 20 dígitos).");
                if (!string.IsNullOrWhiteSpace(req.CoPromoId) && !string.IsNullOrWhiteSpace(req.FePromoId))
                    return BadRequest("Solo se permite una promo (comercio o financiera).");

                var pm = new PaymentManager();
                var updated = pm.ConfirmPayment(req);

                if (updated == null) return StatusCode(500, "No se pudo confirmar el pago.");

                // ===== Notificaciones por email =====
                var emailManager = new EmailManager(sendGridConfig.ApiKey);

                var cr = new System.Globalization.CultureInfo("es-CR");
                var tz = TimeZoneInfo.FindSystemTimeZoneById(
#if WINDOWS
            "Central America Standard Time"
#else
                    "America/Costa_Rica"
#endif
                );
                var paidLocal = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.SpecifyKind(updated.PaymentDate, DateTimeKind.Utc), tz);

                string amountOriginal = updated.GrossAmount.ToString("C", cr);
                decimal amountFinalValue = updated.GrossAmount - (updated.AmountWDiscount ?? 0m);
                if (amountFinalValue < 0) amountFinalValue = 0;
                string amountFinal = amountFinalValue.ToString("C", cr);

                string promoLabel = "Sin promoción";
                if (!string.IsNullOrWhiteSpace(updated.CoPromoId)) promoLabel = $"Promo comercio: {updated.CoPromoId}";
                else if (!string.IsNullOrWhiteSpace(updated.FePromoId)) promoLabel = $"Promo entidad: {updated.FePromoId}";

                var cCrud = new CommerceCrudFactory();
                var commerce = cCrud.RetrieveById<Commerce>(updated.CommerceId);
                string commerceName = commerce?.Name ?? "Comercio";
                string commerceEmail = commerce?.Email;

                var finalUserMgr = new FinalUserManager();
                var finalUser = finalUserMgr.RetrieveByNationalId(updated.NationalId);

                string coCommission = (updated.CoCommisionAmount ?? 0m).ToString("C", cr);

                // ===== 1) Email al USUARIO FINAL =====
                if (finalUser != null && !string.IsNullOrWhiteSpace(finalUser.Email))
                {
                    string maskedIban = updated.IBAN?.Length >= 22
                        ? $"{updated.IBAN.Substring(0, 4)}{updated.IBAN.Substring(18)}"
                        : updated.IBAN;

                    var subjectUser = "Pago confirmado en BilleTico";
                    var bodyUser = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Pago confirmado</h2>
  <p>Hola {finalUser.FirstName},</p>
  <p>Tu pago en <strong>{commerceName}</strong> fue confirmado.</p>
  <ul>
    <li><strong>Monto original:</strong> {amountOriginal}</li>
    <li><strong>Monto final pagado:</strong> {amountFinal}</li>
    <li><strong>Promoción aplicada:</strong> {promoLabel}</li>
    <li><strong>Fecha y hora:</strong> {paidLocal:dd/MM/yyyy HH:mm}</li>
    <li><strong>Comercio:</strong> {commerceName}</li>
    <li><strong>Cuenta utilizada:</strong> {maskedIban}</li>
    <li><strong>ID de pago:</strong> {updated.PaymentId}</li>
  </ul>
  <p style='margin-top:10px;color:#555'>Gracias por usar BilleTico.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones</p>
</div>";
                    emailManager.SendEmail(finalUser.FirstName, finalUser.Email, subjectUser, bodyUser);
                }

                // ===== 2) Email al COMERCIO =====
                if (!string.IsNullOrWhiteSpace(commerceEmail))
                {
                    var subjectCo = "Pago recibido - BilleTico";
                    var bodyCo = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Pago confirmado en tu comercio</h2>
  <p>Hola {commerceName},</p>
  <p>Se confirmó un pago.</p>
  <ul>
    <li><strong>Monto original:</strong> {amountOriginal}</li>
    <li><strong>Monto final pagado por el cliente:</strong> {amountFinal}</li>
    <li><strong>Promoción aplicada:</strong> {promoLabel}</li>
    <li><strong>Fecha y hora:</strong> {paidLocal:dd/MM/yyyy HH:mm}</li>
    <li><strong>Comercio:</strong> {commerceName}</li>
    <li><strong>Comisión cobrada (comercio):</strong> {coCommission}</li>
    <li><strong>ID de pago:</strong> {updated.PaymentId}</li>
  </ul>
  <p style='margin-top:10px;color:#555'>Consulta más detalles en tu panel de BilleTico.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones</p>
</div>";
                    emailManager.SendEmail(commerceName, commerceEmail, subjectCo, bodyCo);
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error confirmando pago: {ex.Message}");
            }
        }

        // (Dejalo si querés para más adelante; arreglé la regex)
        /*[HttpPut]
        [Route("Confirm")]
        public ActionResult Confirm([FromBody] ConfirmPaymentRequest req)
        {
            try
            {
                if (req == null) return BadRequest("Request requerido.");
                if (req.Id <= 0) return BadRequest("Id inválido.");
                if (string.IsNullOrWhiteSpace(req.IBAN)) return BadRequest("IBAN requerido.");
                if (!Regex.IsMatch(req.IBAN, @"^[A-Z]{2}\d{20}$")) return BadRequest("IBAN inválido (CR + 20 dígitos).");
                if (!string.IsNullOrWhiteSpace(req.CoPromoId) && !string.IsNullOrWhiteSpace(req.FePromoId))
                    return BadRequest("Solo se permite una promo (comercio o financiera).");

                var pm = new PaymentManager();
                pm.ConfirmPayment(req);
                return Ok(new { message = "Pago confirmado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error confirmando pago: {ex.Message}");
            }
        }*/

        // Obtener por ID
        [HttpGet]
        [Route("GetById/{id:int}")]
        public ActionResult GetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest("ID de pago inválido.");

                var pm = new PaymentManager();
                var payment = pm.RetrieveById(id);

                if (payment == null)
                    return NotFound("Pago no encontrado.");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error obteniendo pago: {ex.Message}");
            }
        }

        // Datos de prueba (opcional)
        [HttpPost]
        [Route("CreateTestData")]
        public ActionResult CreateTestData()
        {
            try
            {
                var now = DateTime.UtcNow;
                var items = new List<Payment>
                {
                    // Sin promo: descuento = 0
                    new Payment { NationalId="123456789", CommerceId=1, GrossAmount=25000m, AmountWDiscount=0m, CoCommisionAmount=500m, Status="Pendiente", PaymentDate=now, Description="Cobro test 1" },
                    new Payment { NationalId="987654321", CommerceId=1, GrossAmount=15000m, AmountWDiscount=0m, CoCommisionAmount=350m, Status="Pendiente", PaymentDate=now, Description="Cobro test 2" }
                };

                var pm = new PaymentManager();
                foreach (var p in items) pm.Create(p);

                return Ok(new { message = $"Se crearon {items.Count} cobros de prueba." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creando datos de prueba: {ex.Message}");
            }
        }
    }
}