using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface ISkillRepository
    {
        Task<Skill?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<(List<Skill> Items, int Total)> GetPagedAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
        Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
        Task UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<List<Skill>> FindByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    }
}
