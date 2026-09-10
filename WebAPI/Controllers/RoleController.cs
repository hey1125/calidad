using CoreApp;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        [HttpPost("CreateAndAssign")]
        public IActionResult CreateAndAssign([FromQuery] string name, [FromQuery] string nationalId, [FromQuery] int commerceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(nationalId))
                    return BadRequest(new { message = "Debe indicar el nombre del rol y la cédula del usuario." });

                var mgr = new RoleManager();
                var result = mgr.CreateAndAssignToUser(name.Trim(), nationalId.Trim(), commerceId);

                return Ok(new
                {
                    message = "Rol creado/asignado correctamente.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                // Mapear errores del SP
                var msg = ex.Message ?? string.Empty;
                if (msg.Contains("USER_NOT_FOUND"))
                    return NotFound(new { message = "No existe un usuario con esa cédula." });
                if (msg.Contains("ASSIGNMENT_EXISTS"))
                    return Conflict(new { message = "Ese usuario ya tiene ese rol en este comercio." });

                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
