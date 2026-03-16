using JobHunter.Application.Contracts.Companies;
using JobHunter.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Abstractions
{
    public interface ICompanyService
    {
        Task<Company> CreateCompanyAsync(ReqCreateCompanyDto dto, CancellationToken cancellationToken = default);
        Task<Company?> UpdateCompanyAsync(ReqUpdateCompanyDto dto, CancellationToken cancellationToken = default);
        Task DeleteCompanyAsync(long id, CancellationToken cancellationToken = default);
        Task<Company?> GetCompanyByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<List<Company>> GetCompaniesAsync(CancellationToken cancellationToken = default);
    }
}
