using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Role role, CancellationToken cancellationToken = default);
        Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
