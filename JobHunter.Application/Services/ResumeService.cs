using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Resumes;
using JobHunter.Application.Contracts;
using JobHunter.Application.Utilities;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJobRepository _jobRepository;

    public ResumeService(IResumeRepository resumeRepository, IUserRepository userRepository, IJobRepository jobRepository)
    {
        _resumeRepository = resumeRepository;
        _userRepository = userRepository;
        _jobRepository = jobRepository;
    }

    public async Task<ResCreateResumeDto> CreateAsync(ReqCreateResumeDto dto, CancellationToken cancellationToken = default)
    {
        var userId = FlexibleIdParser.ParseRequired(dto.User, "user");
        var jobId = FlexibleIdParser.ParseRequired(dto.Job, "job");

        // Validate user
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with id {userId} not found");

        // Validate job
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null)
            throw new InvalidOperationException($"Job with id {jobId} not found");

        if (!Enum.TryParse<Domain.Enums.ResumeState>(dto.Status, true, out var st))
            throw new InvalidOperationException($"Invalid status value: {dto.Status}");
        var resume = new Resume
        {
            Email = dto.Email,
            Url = dto.Url,
            Status = st,
            UserId = userId,
            JobId = jobId
        };
        await _resumeRepository.AddAsync(resume, cancellationToken);
        return new ResCreateResumeDto
        {
            Id = resume.Id,
            CreatedAt = resume.CreatedAt,
            CreatedBy = resume.CreatedBy
        };
    }

    public async Task<ResUpdateResumeDto> UpdateAsync(ReqUpdateResumeDto dto, CancellationToken cancellationToken = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (resume == null)
            throw new KeyNotFoundException($"Resume with id {dto.Id} not found");
        if (!Enum.TryParse<Domain.Enums.ResumeState>(dto.Status, true, out var st))
            throw new InvalidOperationException($"Invalid status value: {dto.Status}");
        resume.Status = st;
        await _resumeRepository.UpdateAsync(resume, cancellationToken);
        return new ResUpdateResumeDto
        {
            UpdatedAt = resume.UpdatedAt ?? DateTime.UtcNow,
            UpdatedBy = resume.UpdatedBy
        };
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _resumeRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ResResumeDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var resume = await _resumeRepository.GetByIdAsync(id, cancellationToken);
        if (resume == null) return null;
        return MapToResResumeDto(resume);
    }

    public async Task<ResultPaginationDto<ResResumeDto>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _resumeRepository.GetPagedAsync(filter, page, pageSize, sort, cancellationToken);
        var result = items.Select(MapToResResumeDto).ToList();
        return ResultPaginationDto<ResResumeDto>.Create(result, page, pageSize, total);
    }

    public async Task<ResultPaginationDto<ResResumeDto>> GetByUserAsync(long userId, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _resumeRepository.GetByUserPagedAsync(userId, page, pageSize, sort, cancellationToken);
        var result = items.Select(MapToResResumeDto).ToList();
        return ResultPaginationDto<ResResumeDto>.Create(result, page, pageSize, total);
    }

    private ResResumeDto MapToResResumeDto(Resume resume)
    {
        return new ResResumeDto
        {
            Id = resume.Id,
            Email = resume.Email,
            Url = resume.Url,
            Status = resume.Status.ToString(),
            CompanyName = resume.Job?.Company?.Name,
            User = resume.User is null ? null : new ResObjectIdNameDto { Id = resume.User.Id, Name = resume.User.Name },
            Job = resume.Job is null ? null : new ResObjectIdNameDto { Id = resume.Job.Id, Name = resume.Job.Name },
            UserId = resume.UserId,
            UserName = resume.User?.Name,
            JobId = resume.JobId,
            JobName = resume.Job?.Name,
            CreatedAt = resume.CreatedAt,
            CreatedBy = resume.CreatedBy,
            UpdatedAt = resume.UpdatedAt,
            UpdatedBy = resume.UpdatedBy
        };
    }
}
