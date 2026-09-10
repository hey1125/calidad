using CoreApp;
using CoreApp.Classes;
using DataAccess.CRUD;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialEntityController : ControllerBase
    {
        [HttpPost]
        [Route("Create")]
        public ActionResult Create(FinancialEntity financialEntity)
        {
            try
            {
                var fem = new FinancialEntityManager();
                fem.Create(financialEntity);
                return Ok(financialEntity);
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
                var fem = new FinancialEntityManager();
                var entity = fem.GetEntityDetails(id);
                if (entity == null)
                    return NotFound($"No se encontró la entidad financiera con id '{id}'.");
                return Ok(entity);
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
                var fem = new FinancialEntityManager();
                var lstResult = fem.GetPendingEntities();
                if (lstResult == null || lstResult.Count == 0)
                    return Ok(new List<FinancialEntity>()); // Retornar lista vacía en lugar de NotFound
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
                var fem = new FinancialEntityManager();
                var lstResult = fem.GetActiveEntities();
                if (lstResult == null || lstResult.Count == 0)
                    return Ok(new List<FinancialEntity>()); // Retornar lista vacía en lugar de NotFound
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
                var fem = new FinancialEntityManager();
                var lstResult = fem.GetRejectedEntities();
                if (lstResult == null || lstResult.Count == 0)
                    return Ok(new List<FinancialEntity>()); // Retornar lista vacía en lugar de NotFound
                return Ok(lstResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        [Route("Approve")]
        public ActionResult Approve(FinancialEntity financialEntity)
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
                // Validar cédula del admin del banco
                if (string.IsNullOrWhiteSpace(financialEntity.NationalId))
                    return BadRequest("No se especificó la cédula del usuario administrador.");

                // Buscar usuario por cédula
                var finalUserManager = new FinalUserManager();
                var finalUser = finalUserManager.RetrieveByNationalId(financialEntity.NationalId);

                if (finalUser == null)
                    return BadRequest(new { message = $"No se encontró ningún usuario con la cédula '{financialEntity.NationalId}'." });

                if (finalUser.Status != "Active")
                    return BadRequest(new { message = $"El usuario con cédula '{financialEntity.NationalId}' no está activo." });

                // Aprobar banco
                var fem = new FinancialEntityManager();
                fem.ApproveEntity(financialEntity.Id, financialEntity.CommissionRate);

                // Insertar relación usuario-banco
                var relCrud = new FinalUserFinancialEntityRolCrudFactory();
                relCrud.Create(new FinalUserFinancialEntityRol
                {
                    FinalUserId = finalUser.Id,
                    FinancialEntityId = financialEntity.Id,
                    RolId = 1
                });

                // ===== Enviar correos =====
                var feCrud = new FinancialEntityCrudFactory();
                var entityDb = feCrud.RetrieveById<FinancialEntity>(financialEntity.Id);

                var emailManager = new EmailManager(sendGridConfig.ApiKey);

                // 1) Correo al usuario administrador
                var subjectUser = "Entidad bancaria aprobada en BilleTico";
                var bodyUser = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>¡Tu entidad fue aprobada!</h2>
  <p>Hola {finalUser.FirstName},</p>
  <p>La entidad <strong>{entityDb.Name}</strong> ha sido aprobada en BilleTico.</p>
  <ul>
    <li>Comisión establecida: <strong>{financialEntity.CommissionRate:0.##}%</strong></li>
    <li>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</li>
  </ul>
  <p>Ya puedes gestionar cobros y consultar estadísticas de pagos.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones automáticas</p>
</div>";
                emailManager.SendEmail(finalUser.FirstName, finalUser.Email, subjectUser, bodyUser);

                // 2) Correo al email de la entidad (si existe)
                if (!string.IsNullOrWhiteSpace(entityDb.Email))
                {
                    var subjectEntity = "Entidad aprobada en BilleTico";
                    var bodyEntity = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Entidad aprobada</h2>
  <p>Hola {entityDb.Name},</p>
  <p>Su entidad ha sido aprobada en BilleTico.</p>
  <ul>
    <li>Comisión asignada: <strong>{financialEntity.CommissionRate:0.##}%</strong></li>
    <li>Estado actual: <strong>Activo</strong></li>
  </ul>
  <p>Si no reconoce esta aprobación, responda a este correo.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones automáticas</p>
</div>";
                    emailManager.SendEmail(entityDb.Name ?? "Entidad", entityDb.Email, subjectEntity, bodyEntity);
                }
                // ===== Fin enviar correos =====

                return Ok($"La entidad bancaria fue aprobada y vinculada al usuario {finalUser.Email}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al aprobar la entidad bancaria: {ex.Message}" });
            }
        }


        [HttpPut]
        [Route("Reject")]
        public ActionResult Reject(FinancialEntity financialEntity)
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
                var fem = new FinancialEntityManager();

                // Recuperar la entidad para validar estado y obtener email/nombre
                var feCrud = new FinancialEntityCrudFactory();
                var entityDb = feCrud.RetrieveById<FinancialEntity>(financialEntity.Id);

                if (entityDb == null)
                    return NotFound("La entidad financiera no existe.");

                if (!string.Equals(entityDb.Status, "Pendiente", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Solo se pueden rechazar entidades en estado pendiente.");

                // Rechazar
                fem.RejectEntity(financialEntity.Id);

                // Enviar correo a la entidad (si hay email)
                if (!string.IsNullOrWhiteSpace(entityDb.Email))
                {
                    var emailManager = new EmailManager(sendGridConfig.ApiKey);

                    var subject = "Entidad financiera rechazada en BilleTico";
                    var body = $@"
<div style='font-family:Segoe UI,Arial,sans-serif;max-width:560px;margin:auto'>
  <h2 style='margin:0 0 12px'>Estado de solicitud</h2>
  <p>Hola {entityDb.Name},</p>
  <p>Su solicitud para registrar la entidad financiera <strong>{entityDb.Name}</strong> ha sido <strong>rechazada</strong>.</p>
  <p>Si considera que esto es un error o ya corrigió la información requerida, por favor envíe nuevamente la solicitud.</p>
  <ul>
    <li>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</li>
  </ul>
  <p style='margin-top:10px;color:#555'>Este mensaje fue generado automáticamente por BilleTico.</p>
  <hr style='border:none;border-top:1px solid #eee;margin:20px 0'>
  <p style='font-size:12px;color:#888'>BilleTico • Notificaciones</p>
</div>";

                    emailManager.SendEmail(entityDb.Name ?? "Entidad", entityDb.Email, subject, body);
                }

                return Ok("La entidad financiera fue rechazada y no puede operar en BilleTico.");
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
                var fem = new FinancialEntityManager();
                var result = fem.GetEntitiesByUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveByBankCode")]
        public ActionResult RetrieveByBankCode(string bankCode)
        {
            try
            {
                var fem = new FinancialEntityManager();
                var entity = fem.RetrieveByBankCode(bankCode);
                if (entity == null)
                    return NotFound($"No se encontró la entidad financiera con código bancario '{bankCode}'.");
                return Ok(entity);
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
                var fem = new FinancialEntityManager();
                var result = fem.GetActiveEntitiesByUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}