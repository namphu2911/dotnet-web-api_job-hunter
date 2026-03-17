namespace JobHunter.Application.Contracts.Files;

public class ResUploadFileDto
{
    public string FileName { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public ResUploadFileDto(string fileName, DateTime uploadedAt)
    {
        FileName = fileName;
        UploadedAt = uploadedAt;
    }
}
