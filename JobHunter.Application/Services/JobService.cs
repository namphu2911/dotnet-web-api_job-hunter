using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Jobs;
using JobHunter.Application.Contracts;
using JobHunter.Application.Utilities;
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
        var companyId = FlexibleIdParser.ParseRequired(dto.Company, "company");
        var skillIds = FlexibleIdParser.ParseList(dto.Skills, "skills");

        // Validate company
        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company == null)
            throw new InvalidOperationException($"Company with id {companyId} not found");

        // Validate skills
        var skills = new List<Skill>();
        if (skillIds.Count > 0)
        {
            skills = await _skillRepository.FindByIdsAsync(skillIds, cancellationToken);
            if (skills.Count != skillIds.Count)
                throw new InvalidOperationException("Some skills not found");
        }

        if (!Enum.TryParse<Domain.Enums.Level>(dto.Level, true, out var lvl))
            throw new InvalidOperationException($"Invalid level value: {dto.Level}");
        var job = new Job
        {
            Name = dto.Name,
            Location = dto.Location,
            Salary = (double)dto.Salary,
            Quantity = dto.Quantity,
            Level = lvl,
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
        var companyId = FlexibleIdParser.ParseRequired(dto.Company, "company");
        var skillIds = FlexibleIdParser.ParseList(dto.Skills, "skills");

        var job = await _jobRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (job == null)
            throw new KeyNotFoundException($"Job with id {dto.Id} not found");

        // Update fields
        job.Name = dto.Name;
        job.Location = dto.Location;
        job.Salary = (double)dto.Salary;
        job.Quantity = dto.Quantity;
        if (!Enum.TryParse<Domain.Enums.Level>(dto.Level, true, out var lvl))
            throw new InvalidOperationException($"Invalid level value: {dto.Level}");
        job.Level = lvl;
        job.Description = dto.Description;
        job.StartDate = dto.StartDate;
        job.EndDate = dto.EndDate;
        job.Active = dto.Active;

        // Update company
        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);
        if (company == null)
            throw new InvalidOperationException($"Company with id {companyId} not found");
        job.Company = company;

        // Update skills
        var skills = new List<Skill>();
        if (skillIds.Count > 0)
        {
            skills = await _skillRepository.FindByIdsAsync(skillIds, cancellationToken);
            if (skills.Count != skillIds.Count)
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


    public async Task<ResultPaginationDto<ResJobDto>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _jobRepository.GetPagedAsync(filter, page, pageSize, sort, cancellationToken);
        var result = items.Select(MapToResJobDto).ToList();
        return ResultPaginationDto<ResJobDto>.Create(result, page, pageSize, total);
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
            StartDate = job.StartDate,
            EndDate = job.EndDate,
            Active = job.Active,
            CreatedAt = job.CreatedAt,
            CreatedBy = job.CreatedBy,
            UpdatedAt = job.UpdatedAt,
            UpdatedBy = job.UpdatedBy,
            Company = job.Company is null
                ? null
                : new ResJobDto.ResJobCompanyDto
                {
                    Id = job.Company.Id,
                    Name = job.Company.Name,
                    Logo = job.Company.Logo
                },
            Skills = job.Skills?.Select(s => new ResJobDto.ResJobSkillDto
            {
                Id = s.Id,
                Name = s.Name
            }).ToList() ?? new List<ResJobDto.ResJobSkillDto>()
        };
    }
}
