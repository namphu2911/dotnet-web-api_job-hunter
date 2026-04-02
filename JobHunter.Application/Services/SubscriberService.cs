using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Subscribers;
using JobHunter.Application.Utilities;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public class SubscriberService : ISubscriberService
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly ISkillRepository _skillRepository;

    public SubscriberService(ISubscriberRepository subscriberRepository, ISkillRepository skillRepository)
    {
        _subscriberRepository = subscriberRepository;
        _skillRepository = skillRepository;
    }

    public async Task<List<SubscriberDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await _subscriberRepository.GetAllAsync(cancellationToken);
        return all.Select(MapToDto).ToList();
    }

    public async Task<ResultPaginationDto<SubscriberDto>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
    {
        var (items, total) = await _subscriberRepository.GetPagedAsync(filter, page, pageSize, sort, cancellationToken);
        var result = items.Select(MapToDto).ToList();
        return ResultPaginationDto<SubscriberDto>.Create(result, page, pageSize, total);
    }

    public async Task<long> CreateAsync(ReqCreateSubscriberDto dto, CancellationToken cancellationToken = default)
    {
        // Check if email exists
        var exists = await _subscriberRepository.ExistsByEmailAsync(dto.Email, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"Subscriber with email {dto.Email} already exists");

        // Validate skills
        var skills = new List<Skill>();
        if (dto.Skills != null && dto.Skills.Count > 0)
        {
            var skillIds = FlexibleIdParser.ParseList(dto.Skills, "skills");
            skills = await _skillRepository.FindByIdsAsync(skillIds, cancellationToken);
            if (skills.Count != skillIds.Count)
                throw new InvalidOperationException("Some skills not found");
        }

        var subscriber = new Subscriber
        {
            Email = dto.Email,
            Name = dto.Name
        };
        foreach (var skill in skills)
            subscriber.Skills.Add(skill);

        await _subscriberRepository.AddAsync(subscriber, cancellationToken);
        return subscriber.Id;
    }

    public async Task UpdateAsync(ReqUpdateSubscriberDto dto, CancellationToken cancellationToken = default)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (subscriber == null)
            throw new KeyNotFoundException($"Subscriber with id {dto.Id} not found");

        // Update skills
        var skills = new List<Skill>();
        if (dto.Skills != null && dto.Skills.Count > 0)
        {
            var skillIds = FlexibleIdParser.ParseList(dto.Skills, "skills");
            skills = await _skillRepository.FindByIdsAsync(skillIds, cancellationToken);
            if (skills.Count != skillIds.Count)
                throw new InvalidOperationException("Some skills not found");
        }
        subscriber.Skills.Clear();
        foreach (var skill in skills)
            subscriber.Skills.Add(skill);

        await _subscriberRepository.UpdateAsync(subscriber, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, cancellationToken);
        if (subscriber == null)
        {
            throw new KeyNotFoundException($"Subscriber with id {id} not found");
        }

        await _subscriberRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _subscriberRepository.ExistsByEmailAsync(email, cancellationToken);
    }

    public async Task<SubscriberDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var subscriber = await _subscriberRepository.FindByEmailAsync(email, cancellationToken);
        if (subscriber == null) return null;
        return MapToDto(subscriber);
    }

    public async Task<SubscriberDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var subscriber = await _subscriberRepository.GetByIdAsync(id, cancellationToken);
        if (subscriber == null) return null;
        return MapToDto(subscriber);
    }

    private SubscriberDto MapToDto(Subscriber subscriber)
    {
        return new SubscriberDto
        {
            Id = subscriber.Id,
            Email = subscriber.Email,
            Name = subscriber.Name,
            Skills = subscriber.Skills?.Select(s => s.Name).ToList() ?? new List<string>()
        };
    }
}
