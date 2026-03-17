using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Resumes;
using JobHunter.Application.Contracts;
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
        // Validate user
        var user = await _userRepository.GetByIdAsync(dto.User, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with id {dto.User} not found");

        // Validate job
        var job = await _jobRepository.GetByIdAsync(dto.Job, cancellationToken);
        if (job == null)
            throw new InvalidOperationException($"Job with id {dto.Job} not found");

        var resume = new Resume
        {
            Email = dto.Email,
            Url = dto.Url,
            Status = Enum.TryParse<Domain.Enums.ResumeState>(dto.Status, true, out var st) ? st : Domain.Enums.ResumeState.Unknown,
            User = user,
            Job = job
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
        resume.Status = Enum.TryParse<Domain.Enums.ResumeState>(dto.Status, true, out var st) ? st : Domain.Enums.ResumeState.Unknown;
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

    public async Task<ResultPaginationDto<ResResumeDto>> GetListAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var allResumes = await _resumeRepository.GetAllAsync(cancellationToken);
        IEnumerable<Resume> filtered = allResumes;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            filtered = filtered.Where(r =>
                (r.Email != null && r.Email.ToLowerInvariant().Contains(f)) ||
                (r.Job != null && r.Job.Name.ToLowerInvariant().Contains(f)));
        }
        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new ResultPaginationDto<ResResumeDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(MapToResResumeDto).ToList()
        };
        return result;
    }

    public async Task<ResultPaginationDto<ResResumeDto>> GetByUserAsync(long userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var allResumes = await _resumeRepository.GetAllAsync(cancellationToken);
        var filtered = allResumes.Where(r => r.UserId == userId);
        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new ResultPaginationDto<ResResumeDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(MapToResResumeDto).ToList()
        };
        return result;
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
