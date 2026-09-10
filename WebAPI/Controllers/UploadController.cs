using DTOs;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("file")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Seleccione una imagen." });

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return BadRequest(new { error = "La imagen debe ser JPG o PNG." });

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var directory = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads");
            Directory.CreateDirectory(directory);
            await using (var stream = new FileStream(Path.Combine(directory, fileName), FileMode.CreateNew))
            {
                await file.CopyToAsync(stream, HttpContext.RequestAborted);
            }

            return Ok(new
            {
                url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/uploads/{fileName}",
                publicId = $"uploads/{fileName}"
            });
        }
    }
}