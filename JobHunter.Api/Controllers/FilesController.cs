using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] JobHunter.Application.Contracts.Files.FileUploadRequest request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("File is empty. Please upload a file.");
        if (!_fileService.IsValidExtension(request.File.FileName))
            return BadRequest("Invalid file extension. Only pdf, jpg, jpeg, png, doc, docx allowed.");
        using var stream = request.File.OpenReadStream();
        var result = await _fileService.UploadAsync(stream, request.File.FileName, request.Folder, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Download([FromQuery] string fileName, [FromQuery] string folder, CancellationToken cancellationToken)
    {
        var length = await _fileService.GetFileLengthAsync(fileName, folder, cancellationToken);
        if (length == 0)
            return NotFound("File not found.");
        var stream = await _fileService.DownloadAsync(fileName, folder, cancellationToken);
        if (stream == null)
            return NotFound("File not found.");
        return File(stream, "application/octet-stream", fileName);
    }
}
