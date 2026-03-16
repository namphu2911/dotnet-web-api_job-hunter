using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface IPermissionRepository
    {
        Task<Permission?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Permission>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Permission permission, CancellationToken cancellationToken = default);
        Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByModuleApiPathMethodAsync(string module, string apiPath, string method, CancellationToken cancellationToken = default);
        Task<List<Permission>> FindByIdsAsync(IEnumerable<long> ids, CancellationToken cancellationToken = default);
    }
}
