using JobHunter.Application.Contracts.Files;

namespace JobHunter.Application.Abstractions;

public interface IFileService
{
    Task<ResUploadFileDto> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string fileName, string folder, CancellationToken cancellationToken = default);
    Task<long> GetFileLengthAsync(string fileName, string folder, CancellationToken cancellationToken = default);
    bool IsValidExtension(string fileName);
    Task EnsureDirectoryExistsAsync(string folder, CancellationToken cancellationToken = default);
}
