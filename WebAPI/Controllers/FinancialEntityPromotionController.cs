using CoreApp;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialEntityPromotionController : ControllerBase
    {

        // =============================
        // Retrieve ALL by FinancialEntityId
        // =============================
        [HttpGet]
        [Route("RetrieveAllByEntity")]
        public ActionResult RetrieveAllByEntity(int financialEntityId)
        {
            try
            {
                if (financialEntityId <= 0) return BadRequest("financialEntityId inválido.");

                var mgr = new FinancialEntityPromotionManager();
                var promos = mgr.GetAllByEntity(financialEntityId);

                if (promos == null || promos.Count == 0)
                    return NotFound("No hay promociones para la entidad especificada.");

                return Ok(promos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // =============================
        // Retrieve by Id (para edición)
        // =============================
        [HttpGet]
        [Route("RetrieveById/{id:int}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Id inválido.");

                var mgr = new FinancialEntityPromotionManager();
                var promo = mgr.GetById(id);

                if (promo == null)
                    return NotFound("Promoción no encontrada.");

                return Ok(promo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveActiveByIBAN")]
        public ActionResult RetrieveActiveByIBAN(string iban, decimal amount, DateTime? refDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban)) return BadRequest("Debe indicar la cuenta IBAN.");
                if (amount <= 0) return BadRequest("El monto debe ser mayor a 0.");

                var mgr = new FinancialEntityPromotionManager();
                var promos = mgr.GetActiveByIBAN(iban, amount, refDate);

                if (promos == null || promos.Count == 0)
                    return NotFound("No hay promociones vigentes para el IBAN especificado.");

                return Ok(promos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveBestByIBAN")]
        public ActionResult RetrieveBestByIBAN(string iban, decimal amount, DateTime? refDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban)) return BadRequest("Debe indicar la cuenta IBAN.");
                if (amount <= 0) return BadRequest("El monto debe ser mayor a 0.");

                var mgr = new FinancialEntityPromotionManager();
                var best = mgr.GetBestByIBAN(iban, amount, refDate);

                if (best == null)
                    return NotFound("No hay promociones vigentes para el IBAN especificado.");

                return Ok(best);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // =============================
        // CREATE
        // =============================
        [HttpPost]
        [Route("Create")]
        public ActionResult Create([FromBody] FinancialEntityPromotion promo)
        {
            try
            {
                if (promo == null) return BadRequest("Body inválido.");

                var mgr = new FinancialEntityPromotionManager();
                var created = mgr.Create(promo);

                if (created == null)
                    return StatusCode(500, "No se pudo crear la promoción de entidad financiera.");

                // 201 Created con el objeto creado
                return StatusCode(201, created);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // =============================
        // UPDATE
        // =============================
        [HttpPut]
        [Route("Update/{id:int}")]
        public ActionResult Update(int id, [FromBody] FinancialEntityPromotion promo)
        {
            try
            {
                if (id <= 0) return BadRequest("Id inválido.");
                if (promo == null) return BadRequest("Body inválido.");

                // Aseguramos consistencia entre ruta y body
                promo.Id = id;

                var mgr = new FinancialEntityPromotionManager();
                var ok = mgr.Update(promo);

                if (!ok)
                    return StatusCode(500, "No se pudo actualizar la promoción de entidad financiera.");

                return Ok(promo);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // =============================
        // DELETE
        // =============================
        [HttpDelete]
        [Route("Delete/{id:int}")]
        public ActionResult Delete(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Id inválido.");

                var mgr = new FinancialEntityPromotionManager();
                var ok = mgr.Delete(id);

                if (!ok)
                    return StatusCode(500, "No se pudo eliminar la promoción de entidad financiera.");

                return NoContent();
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
