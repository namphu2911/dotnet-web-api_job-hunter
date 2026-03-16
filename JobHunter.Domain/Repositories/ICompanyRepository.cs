using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Company company, CancellationToken cancellationToken = default);
        Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
        Task DeleteAsync(long id, CancellationToken cancellationToken = default);
        // Add more methods as needed (e.g., search/filter)
    }
}
