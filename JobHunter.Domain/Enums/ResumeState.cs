using System.Text.Json.Serialization;
namespace JobHunter.Domain.Enums;

public enum ResumeState
{
    PENDING,
    REVIEWING,
    APPROVED,
    REJECTED
}
