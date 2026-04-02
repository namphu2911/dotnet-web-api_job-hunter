using JobHunter.Application.Contracts.Resumes;
using JobHunter.Application.Contracts;

namespace JobHunter.Application.Abstractions;

public interface IResumeService
{
    Task<ResCreateResumeDto> CreateAsync(ReqCreateResumeDto dto, CancellationToken cancellationToken = default);
    Task<ResUpdateResumeDto> UpdateAsync(ReqUpdateResumeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ResResumeDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ResultPaginationDto<ResResumeDto>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
    Task<ResultPaginationDto<ResResumeDto>> GetByUserAsync(long userId, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
}
