using CoreApp;
using CoreApp.Classes;
using DataAccess.CRUD;
using DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommerceController : ControllerBase
    {
        [HttpPost]
        [Route("Create")]

        /*
        {"legalId": "string",
         "name": "string",
         "phone": "string",
         "email": "string",
         "latitude": 0,
         "longitude": 0,
         "status": "string",
         "iban": "string",
         "nationalId": "string"
        }*/

        public ActionResult Create(Commerce commerce)
        {
            try
            {
                var cm = new CommerceManager();
                cm.Create(commerce);
                return Ok(commerce);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByLegalId")]
        public ActionResult RetrieveByLegalId(string legalId)
        {
            try
            {
                var cm = new CommerceManager();
                var commerce = cm.RetrieveByLegalId(legalId);
                if (commerce == null)
                    return NotFound($"No se encontró el comercio con cédula jurídica '{legalId}'.");
                return Ok(commerce);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                var cm = new CommerceManager();
                var commerce = cm.RetrieveById(id);
                if (commerce == null)
                    return NotFound($"No se encontró el comercio con id '{id}'.");
                return Ok(commerce);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrievePending")]
        public ActionResult RetrievePending()
        {
            try
            {
                var cm = new CommerceManager();
                var lstResult = cm.GetPendingCommerce();
                if (lstResult == null)
                    return NotFound($"No hay comercios pendientes.");
                return Ok(lstResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveActive")]
        public ActionResult RetrieveActive()
        {
            try
            {
                var cm = new CommerceManager();
                var lstResult = cm.GetActiveCommerces();
                if (lstResult == null)
                    return NotFound($"No hay comercios activos.");
                return Ok(lstResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveRejected")]
        public ActionResult RetrieveRejected()
        {
            try
            {
                var cm = new CommerceManager();
                var lstResult = cm.GetRejectedCommerces();
                if (lstResult == null)
                    return NotFound($"No hay comercios rechazados.");
                return Ok(lstResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Approve")]
        public ActionResult Approve(Commerce commerce)
        {
            // Cargar SendGrid API Key (igual que en UserController.Create)
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            SendGridSettings sendGridConfig = new SendGridSettings
            {
                ApiKey = config["SendGrid:ApiKey"]
            };

            try
            {
                // Validar cédula del admin del comercio
                if (string.IsNullOrWhiteSpace(commerce.NationalId))
                    return BadRequest("No se especificó la cédula del usuario administrador.");

                // Buscar usuario por cédula
                var finalUserManager = new FinalUserManager();
                var finalUser = finalUserManager.RetrieveByNationalId(commerce.NationalId);

                if (finalUser == null)
                    return BadRequest(new { message = $"No se encontró ningún usuario con la cédula '{commerce.NationalId}'." });

                if (finalUser.Status != "Active")
                    return BadRequest(new { message = $"El usuario con cédula '{commerce.NationalId}' no está activo." });

                // Aprobar comercio
                var cm = new CommerceManager();
                cm.ApproveCommerce(commerce.Id, commerce.CommissionRate);

                // Insertar relación usuario-comercio
                var relCrud = new FinalUserCommerceRolCrudFactory();
                relCrud.Create(new FinalUserCommerceRol
                {
                    FinalUserId = finalUser.Id,
                    CommerceId = commerce.Id,
                    RolId = 1
                });

                // ===== NUEVO: enviar correos =====
                // Recuperar datos completos del comercio para el correo (nombre, email, etc.)
                var cCrud = new CommerceCrudFactory();
                var commerceDb = cCrud.RetrieveById<Commerce>(commerce.Id);

                var emailManager = new EmailManager(sendGridConfig.ApiKey);

                // 1) Correo al usuario administrador (finalUser)
                var subjectUser = "Comercio aprobado en BilleTico";
                var bodyUser = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>¡Tu comercio fue aprobado!</h2>
  <p>Hola {finalUser.FirstName},</p>
  <p>El comercio <strong>{commerceDb.Name}</strong> ha sido aprobado en BilleTico.</p>
  <ul>
    <li>Comisión establecida: <strong>{commerce.CommissionRate:0.##}%</strong></li>
    <li>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</li>
  </ul>
  <p>Ya puedes gestionar cobros y configurar tu cuenta.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones automáticas</p>
</div>";
                emailManager.SendEmail(finalUser.FirstName, finalUser.Email, subjectUser, bodyUser);

                // 2) (Opcional recomendado) Correo al correo del comercio
                if (!string.IsNullOrWhiteSpace(commerceDb.Email))
                {
                    var subjectCommerce = "¡Bienvenido! Tu comercio fue aprobado";
                    var bodyCommerce = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Comercio aprobado</h2>
  <p>Hola {commerceDb.Name},</p>
  <p>Tu comercio ha sido aprobado en BilleTico.</p>
  <ul>
    <li>Comisión asignada: <strong>{commerce.CommissionRate:0.##}%</strong></li>
    <li>Estado actual: <strong>Activo</strong></li>
  </ul>
  <p>Si no reconoces esta aprobación, por favor responde a este correo.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones automáticas</p>
</div>";
                    // Nombre destinatario: usa el mismo "Name" del comercio
                    emailManager.SendEmail(commerceDb.Name ?? "Comercio", commerceDb.Email, subjectCommerce, bodyCommerce);
                }
                // ===== FIN NUEVO =====

                return Ok($"El comercio fue aprobado y vinculado al usuario {finalUser.Email}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al aprobar el comercio: {ex.Message}" });
            }
        }


        [HttpPut]
        [Route("Reject")]
        public ActionResult Reject(Commerce commerce)
        {
            // Cargar SendGrid API Key (como en UserController.Create)
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            SendGridSettings sendGridConfig = new SendGridSettings
            {
                ApiKey = config["SendGrid:ApiKey"]
            };

            try
            {
                var cm = new CommerceManager();

                // Recuperar datos del comercio (para validar estado y obtener email/nombre)
                var cCrud = new CommerceCrudFactory();
                var commerceDb = cCrud.RetrieveById<Commerce>(commerce.Id);
                if (commerceDb == null)
                    return NotFound("El comercio no existe.");

                if (!string.Equals(commerceDb.Status, "Pendiente", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Solo se pueden rechazar comercios en estado pendiente.");

                // Rechazar
                cm.RejectCommerce(commerce.Id);

                // Enviar correo al comercio (si tiene email)
                if (!string.IsNullOrWhiteSpace(commerceDb.Email))
                {
                    var emailManager = new EmailManager(sendGridConfig.ApiKey);

                    var subject = "Comercio rechazado en BilleTico";
                    var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Estado de solicitud de comercio</h2>
  <p>Hola {commerceDb.Name},</p>
  <p>Tu solicitud para registrar el comercio <strong>{commerceDb.Name}</strong> ha sido <strong>rechazada</strong>.</p>
  <p>Si consideras que esto es un error o ya corregiste la información, por favor vuelve a enviar la solicitud.</p>
  <ul>
    <li>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</li>
  </ul>
  <p style='margin-top:10px;color:#555'>Este mensaje fue generado automáticamente por BilleTico.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones</p>
</div>";
                    emailManager.SendEmail(commerceDb.Name ?? "Comercio", commerceDb.Email, subject, body);
                }

                return Ok("El comercio fue rechazado y no puede operar en BilleTico.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByUser")]
        public ActionResult RetrieveByUser(int userId)
        {
            try
            {
                var cm = new CommerceManager();
                var result = cm.GetCommercesByUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveActiveByUser")]
        public ActionResult RetrieveActiveByUser(int userId)
        {
            try
            {
                var cm = new CommerceManager();
                var result = cm.GetActiveCommercesByUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}