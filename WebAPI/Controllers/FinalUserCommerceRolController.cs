using CoreApp;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinalUserCommerceRolController : ControllerBase
    {
        [HttpGet]
        [Route("RetrieveByUser")]
        public ActionResult RetrieveByUser(int userId)
        {
            try
            {
                var manager = new FinalUserCommerceRolManager();
                var result = manager.RetrieveCommercesByUser(userId);

                if (result == null || result.Count == 0)
                    return NotFound(new { message = "El usuario no tiene roles asignados en comercios" });

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
                var manager = new FinalUserCommerceRolManager();
                var result = manager.RetrieveRolesByUser(userId);

                if (result == null || result.Count == 0)
                    return NotFound(new { message = "No tiene roles asignados" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("RolesByCommerce")]
        public IActionResult RolesByCommerce(int commerceId)
        {
            try
            {
                var manager = new FinalUserCommerceRolManager();
                var rows = manager.RetrieveRolesByCommerce(commerceId); // List<Dictionary<string, object>>
                return Ok(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("RetrieveAdminById")]
        public IActionResult RetrieveAdminById([FromQuery] int userId, [FromQuery] int commerceId)
        {
            try
            {
                var manager = new FinalUserCommerceRolManager();
                bool isAdmin = manager.IsAdmin(userId, commerceId);
                return Ok(new { isAdmin });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
