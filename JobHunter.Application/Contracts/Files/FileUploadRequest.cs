using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace JobHunter.Application.Contracts.Files;

public class FileUploadRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;

    [Required]
    public string Folder { get; set; } = null!;
}
