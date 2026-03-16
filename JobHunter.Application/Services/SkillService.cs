using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Skills;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;

        public SkillService(ISkillRepository skillRepository)
        {
            _skillRepository = skillRepository;
        }

        public async Task<Skill> CreateSkillAsync(ReqCreateSkillDto dto, CancellationToken cancellationToken = default)
        {
            var skill = new Skill
            {
                Name = dto.Name
            };
            await _skillRepository.AddAsync(skill, cancellationToken);
            return skill;
        }

        public async Task<Skill?> UpdateSkillAsync(ReqUpdateSkillDto dto, CancellationToken cancellationToken = default)
        {
            var skill = await _skillRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (skill == null) return null;
            skill.Name = dto.Name;
            await _skillRepository.UpdateAsync(skill, cancellationToken);
            return skill;
        }

        public async Task DeleteSkillAsync(long id, CancellationToken cancellationToken = default)
        {
            await _skillRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<Skill?> GetSkillByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _skillRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<List<Skill>> GetSkillsAsync(CancellationToken cancellationToken = default)
        {
            return await _skillRepository.GetAllAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _skillRepository.ExistsByNameAsync(name, cancellationToken);
        }
    }
}
