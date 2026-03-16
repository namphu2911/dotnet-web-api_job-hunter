using JobHunter.Application.Contracts.Skills;
using JobHunter.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Abstractions
{
    public interface ISkillService
    {
        Task<Skill> CreateSkillAsync(ReqCreateSkillDto dto, CancellationToken cancellationToken = default);
        Task<Skill?> UpdateSkillAsync(ReqUpdateSkillDto dto, CancellationToken cancellationToken = default);
        Task DeleteSkillAsync(long id, CancellationToken cancellationToken = default);
        Task<Skill?> GetSkillByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Skill>> GetSkillsAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
