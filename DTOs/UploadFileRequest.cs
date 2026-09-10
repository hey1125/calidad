using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DTOs
{
    public class UploadFileRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public string FileName { get; set; }
    }
}
