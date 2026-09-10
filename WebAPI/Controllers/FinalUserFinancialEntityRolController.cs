using CoreApp;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinalUserFinancialEntityRolController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveByUser")]
        public ActionResult RetrieveByUser(int userId)
        {
            try
            {
                var manager = new FinalUserFinancialEntityRolManager();
                var result = manager.RetrieveEntitiesByUser(userId);

                if (result == null || result.Count == 0)
                    return NotFound(new { message = "El usuario no tiene roles asignados en bancos" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("RetrieveRolesByUser")]
        public ActionResult RetrieveRolesByUser(int userId)
        {
            try
            {
                var manager = new FinalUserFinancialEntityRolManager();
                var result = manager.RetrieveRolesByUser(userId);

                if (result == null || result.Count == 0)
                    return NotFound(new { message = "No tiene roles asignados en bancos" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
