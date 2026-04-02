using JobHunter.Application.Contracts.Roles;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Abstractions
{
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(ReqCreateRoleDto dto, CancellationToken cancellationToken = default);
        Task<Role?> UpdateRoleAsync(ReqUpdateRoleDto dto, CancellationToken cancellationToken = default);
        Task DeleteRoleAsync(long id, CancellationToken cancellationToken = default);
        Task<Role?> GetRoleByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResultPaginationDto<Role>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}