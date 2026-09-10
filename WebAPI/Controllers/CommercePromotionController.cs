using CoreApp;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommercePromotionController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveActiveByCommerce")]
        public ActionResult RetrieveActiveByCommerce(int commerceId, decimal amount, DateTime? refDate = null)
        {
            try
            {
                if (commerceId <= 0) return BadRequest("CommerceId inválido.");
                if (amount <= 0) return BadRequest("El monto debe ser mayor a 0.");

                var mgr = new CommercePromotionManager();
                var promos = mgr.GetActiveByCommerce(commerceId, amount, refDate);

                if (promos == null || promos.Count == 0)
                    return NotFound("No hay promociones vigentes para el comercio especificado.");

                return Ok(promos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveBestByCommerce")]
        public ActionResult RetrieveBestByCommerce(int commerceId, decimal amount, DateTime? refDate = null)
        {
            try
            {
                if (commerceId <= 0) return BadRequest("CommerceId inválido.");
                if (amount <= 0) return BadRequest("El monto debe ser mayor a 0.");

                var mgr = new CommercePromotionManager();
                var best = mgr.GetBestByCommerce(commerceId, amount, refDate);

                if (best == null)
                    return NotFound("No hay promociones vigentes para el comercio especificado.");

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
        public ActionResult Create([FromBody] CommercePromotion promo)
        {
            try
            {
                if (promo == null) return BadRequest("Body inválido.");

                var mgr = new CommercePromotionManager();
                var created = mgr.Create(promo);

                if (created == null)
                    return StatusCode(500, "No se pudo crear la promoción.");

                // 201 Created con el objeto creado
                return Ok(created);
            }
            catch (ArgumentException aex)
            {
                // Validaciones del Manager
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
        public ActionResult Update(int id, [FromBody] CommercePromotion promo)
        {
            try
            {
                if (id <= 0) return BadRequest("Id inválido.");
                if (promo == null) return BadRequest("Body inválido.");
                // Forzamos el Id del body al de la ruta para evitar inconsistencias
                promo.Id = id;

                var mgr = new CommercePromotionManager();
                var ok = mgr.Update(promo);

                if (!ok)
                    return StatusCode(500, "No se pudo actualizar la promoción.");

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

                var mgr = new CommercePromotionManager();
                var ok = mgr.Delete(id);

                if (!ok)
                    return StatusCode(500, "No se pudo eliminar la promoción.");

                return Ok(ok);
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

        [HttpGet]
        [Route("RetrieveAllByCommerce")]
        public ActionResult RetrieveAllByCommerce(int commerceId)
        {
            try
            {
                if (commerceId <= 0) return BadRequest("commerceId inválido.");

                var mgr = new CommercePromotionManager();
                var promos = mgr.GetAllByCommerce(commerceId);

                if (promos == null || promos.Count == 0)
                    return NotFound("No hay promociones para el comercio especificado.");

                return Ok(promos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("RetrieveById/{id:int}")]
        public ActionResult RetrieveById(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("Id inválido.");

                var mgr = new CommercePromotionManager();
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



    }
}
