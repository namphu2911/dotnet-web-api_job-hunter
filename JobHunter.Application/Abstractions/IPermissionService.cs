using JobHunter.Application.Contracts.Permissions;
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
        Task<List<Permission>> GetPermissionsAsync(CancellationToken cancellationToken = default);
        Task<bool> ExistsByModuleApiPathMethodAsync(string module, string apiPath, string method, CancellationToken cancellationToken = default);
    }
}