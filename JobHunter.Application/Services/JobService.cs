using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Jobs;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly ICompanyRepository _companyRepository;

    public JobService(IJobRepository jobRepository, ISkillRepository skillRepository, ICompanyRepository companyRepository)
    {
        _jobRepository = jobRepository;
        _skillRepository = skillRepository;
        _companyRepository = companyRepository;
    }


    public async Task<ResJobDto> CreateAsync(ReqCreateJobDto dto, CancellationToken cancellationToken = default)
    {
        // Validate company
        var company = await _companyRepository.GetByIdAsync(dto.Company, cancellationToken);
        if (company == null)
            throw new InvalidOperationException($"Company with id {dto.Company} not found");

        // Validate skills
        var skills = new List<Skill>();
        if (dto.Skills != null && dto.Skills.Count > 0)
        {
            skills = await _skillRepository.FindByIdsAsync(dto.Skills, cancellationToken);
            if (skills.Count != dto.Skills.Count)
                throw new InvalidOperationException("Some skills not found");
        }

        var job = new Job
        {
            Name = dto.Name,
            Location = dto.Location,
            Salary = (double)dto.Salary,
            Quantity = dto.Quantity,
            Level = Enum.TryParse<Domain.Enums.Level>(dto.Level, true, out var lvl) ? lvl : Domain.Enums.Level.Unknown,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Active = dto.Active,
            Company = company
        };
        foreach (var skill in skills)
            job.Skills.Add(skill);

        await _jobRepository.AddAsync(job, cancellationToken);
        return MapToResJobDto(job);
    }


    public async Task<ResJobDto> UpdateAsync(ReqUpdateJobDto dto, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (job == null)
            throw new KeyNotFoundException($"Job with id {dto.Id} not found");

        // Update fields
        job.Name = dto.Name;
        job.Location = dto.Location;
        job.Salary = (double)dto.Salary;
        job.Quantity = dto.Quantity;
        job.Level = Enum.TryParse<Domain.Enums.Level>(dto.Level, true, out var lvl) ? lvl : Domain.Enums.Level.Unknown;
        job.Description = dto.Description;
        job.StartDate = dto.StartDate;
        job.EndDate = dto.EndDate;
        job.Active = dto.Active;

        // Update company
        var company = await _companyRepository.GetByIdAsync(dto.Company, cancellationToken);
        if (company == null)
            throw new InvalidOperationException($"Company with id {dto.Company} not found");
        job.Company = company;

        // Update skills
        var skills = new List<Skill>();
        if (dto.Skills != null && dto.Skills.Count > 0)
        {
            skills = await _skillRepository.FindByIdsAsync(dto.Skills, cancellationToken);
            if (skills.Count != dto.Skills.Count)
                throw new InvalidOperationException("Some skills not found");
        }
        job.Skills.Clear();
        foreach (var skill in skills)
            job.Skills.Add(skill);

        await _jobRepository.UpdateAsync(job, cancellationToken);
        return MapToResJobDto(job);
    }


    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        await _jobRepository.DeleteAsync(id, cancellationToken);
    }


    public async Task<ResJobDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null) return null;
        return MapToResJobDto(job);
    }


    public async Task<ResultPaginationDto<ResJobDto>> GetListAsync(string? filter, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        // Simple filter: by name/location contains (case-insensitive)
        var allJobs = await _jobRepository.GetAllAsync(cancellationToken);
        IEnumerable<Job> filtered = allJobs;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var f = filter.Trim().ToLowerInvariant();
            filtered = filtered.Where(j =>
                (j.Name != null && j.Name.ToLowerInvariant().Contains(f)) ||
                (j.Location != null && j.Location.ToLowerInvariant().Contains(f)));
        }
        var total = filtered.Count();
        var items = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new ResultPaginationDto<ResJobDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items.Select(MapToResJobDto).ToList()
        };
        return result;
    }

    private ResJobDto MapToResJobDto(Job job)
    {
        return new ResJobDto
        {
            Id = job.Id,
            Name = job.Name,
            Location = job.Location,
            Salary = (decimal)job.Salary,
            Quantity = job.Quantity,
            Level = job.Level.ToString(),
            Description = job.Description ?? string.Empty,
            StartDate = job.StartDate ?? DateTime.MinValue,
            EndDate = job.EndDate ?? DateTime.MinValue,
            Active = job.Active,
            CreatedAt = job.CreatedAt,
            CreatedBy = job.CreatedBy,
            UpdatedAt = job.UpdatedAt,
            UpdatedBy = job.UpdatedBy,
            Skills = job.Skills?.Select(s => s.Name).ToList() ?? new List<string>()
        };
    }
}
