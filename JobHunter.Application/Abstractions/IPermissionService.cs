using JobHunter.Application.Contracts.Permissions;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Abstractions
{
    public interface IPermissionService
    {
        Task<Permission> CreatePermissionAsync(ReqCreatePermissionDto dto, CancellationToken cancellationToken = default);
        Task<Permission?> UpdatePermissionAsync(ReqUpdatePermissionDto dto, CancellationToken cancellationToken = default);
        Task DeletePermissionAsync(long id, CancellationToken cancellationToken = default);
        Task<Permission?> GetPermissionByIdAsync(long id, CancellationToken cancellationToken = default);
        Task<ResultPaginationDto<Permission>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default);
        Task<bool> ExistsByModuleApiPathMethodAsync(string module, string apiPath, string method, CancellationToken cancellationToken = default);
    }
}