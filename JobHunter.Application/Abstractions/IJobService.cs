using JobHunter.Application.Contracts.Jobs;
using JobHunter.Application.Contracts;

namespace JobHunter.Application.Abstractions;

public interface IJobService
{
    Task<ResJobDto> CreateAsync(ReqCreateJobDto dto, CancellationToken cancellationToken = default);
    Task<ResJobDto> UpdateAsync(ReqUpdateJobDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ResJobDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ResultPaginationDto<ResJobDto>> GetListAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default);
}
