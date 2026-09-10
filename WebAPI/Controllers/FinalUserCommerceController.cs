using CoreApp;
using DataAccess.CRUD;
using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinalUserCommerceController : ControllerBase
    {
        [HttpGet("GetMyCommerces")]
        public IActionResult GetMyCommerces([FromHeader] int LoggedUserId)
        {
            try
            {
                var crud = new FinalUserCommerceRolCrudFactory();
                var myCommerces = crud.RetrieveCommercesByUser<Commerce>(LoggedUserId);
                return Ok(myCommerces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
