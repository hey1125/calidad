using CoreApp;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly BankAccountManager _manager;

        public BankAccountController()
        {
            _manager = new BankAccountManager();
        }

        // POST: api/BankAccount
        [HttpPost ("Create")]
        public IActionResult Create([FromBody] BankAccount account)
        {
            try
            {
                _manager.Create(account);
                return Ok(new { message = "Cuenta bancaria registrada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        
        [HttpGet("GetAccountsByUser")]
        public IActionResult GetAccountsByUser(int userId)
        {
            try
            {
                var accounts = _manager.GetAccountsByUser(userId);
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("Delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                _manager.Delete(id);
                return Ok(new { message = "Cuenta eliminada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
