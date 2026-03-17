using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Subscribers;
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
            skills = await _skillRepository.FindByIdsAsync(dto.Skills, cancellationToken);
            if (skills.Count != dto.Skills.Count)
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
            skills = await _skillRepository.FindByIdsAsync(dto.Skills, cancellationToken);
            if (skills.Count != dto.Skills.Count)
                throw new InvalidOperationException("Some skills not found");
        }
        subscriber.Skills.Clear();
        foreach (var skill in skills)
            subscriber.Skills.Add(skill);

        await _subscriberRepository.UpdateAsync(subscriber, cancellationToken);
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
