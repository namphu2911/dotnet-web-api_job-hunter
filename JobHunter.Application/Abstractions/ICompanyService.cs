using JobHunter.Application.Contracts.Companies;
using JobHunter.Application.Contracts;
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
        Task<ResultPaginationDto<Company>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
    }
}
