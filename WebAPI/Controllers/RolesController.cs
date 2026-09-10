using CoreApp;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        [HttpGet("List")]
        public IActionResult List([FromQuery] int commerceId)
        {
            try
            {
                var mgr = new RolesManager();
                var list = mgr.RetrieveCashiers(commerceId);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("AddByNationalId")]
        public IActionResult AddByNationalId([FromQuery] string nationalId, [FromQuery] int commerceId, [FromQuery] int roleId /* = 2 */)
        {
            try
            {
                var manager = new RolesManager();

                // Evitar que el admin se asigne a sí mismo (opcional, si envías el userId logueado):
                // if (manager.IsSameUser(nationalId, loggedUserId)) return BadRequest("No puedes agregarte a ti mismo.");

                manager.AddCashierByNationalId(nationalId, commerceId, roleId);
                return Ok(new { message = "Rol asignado correctamente." });
            }
            catch (DuplicateNameException dup)
            {
                return Conflict(new { message = dup.Message }); // 409
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


        [HttpDelete("Delete")]
        public IActionResult Delete([FromQuery] int userId, [FromQuery] int commerceId)
        {
            try
            {
                var manager = new RolesManager();
                manager.DeleteCashier(userId, commerceId);
                return Ok(new { message = "Empleado eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
