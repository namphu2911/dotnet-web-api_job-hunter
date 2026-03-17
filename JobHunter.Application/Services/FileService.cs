
using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Files;
using Microsoft.Extensions.Configuration;

namespace JobHunter.Application.Services;

public class FileService : IFileService
{
    private readonly string _baseUri;
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };

    public FileService(IConfiguration configuration)
    {
        _baseUri = configuration["FileStorage:BasePath"] ?? "storage";
    }

    public bool IsValidExtension(string fileName)
    {
        return AllowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    public async Task EnsureDirectoryExistsAsync(string folder, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_baseUri, folder);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        await Task.CompletedTask;
    }

    public async Task<ResUploadFileDto> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        await EnsureDirectoryExistsAsync(folder, cancellationToken);
        var uniqueName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{fileName}";
        var filePath = Path.Combine(_baseUri, folder, uniqueName);
        using (var file = File.Create(filePath))
        {
            await fileStream.CopyToAsync(file, cancellationToken);
        }
        return new ResUploadFileDto(uniqueName, DateTime.UtcNow);
    }

    public async Task<Stream?> DownloadAsync(string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseUri, folder, fileName);
        if (!File.Exists(filePath)) return null;
        return await Task.FromResult(File.OpenRead(filePath));
    }

    public async Task<long> GetFileLengthAsync(string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseUri, folder, fileName);
        if (!File.Exists(filePath)) return 0;
        var info = new FileInfo(filePath);
        return await Task.FromResult(info.Length);
    }
}
