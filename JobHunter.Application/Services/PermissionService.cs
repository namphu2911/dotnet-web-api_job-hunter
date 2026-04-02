using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Permissions;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionService(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Permission> CreatePermissionAsync(ReqCreatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            var permission = new Permission
            {
                Name = dto.Name,
                ApiPath = dto.ApiPath,
                Method = dto.Method,
                Module = dto.Module
            };
            await _permissionRepository.AddAsync(permission, cancellationToken);
            return permission;
        }

        public async Task<Permission?> UpdatePermissionAsync(ReqUpdatePermissionDto dto, CancellationToken cancellationToken = default)
        {
            var permission = await _permissionRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (permission == null) return null;
            permission.Name = dto.Name;
            permission.ApiPath = dto.ApiPath;
            permission.Method = dto.Method;
            permission.Module = dto.Module;
            await _permissionRepository.UpdateAsync(permission, cancellationToken);
            return permission;
        }

        public async Task DeletePermissionAsync(long id, CancellationToken cancellationToken = default)
        {
            await _permissionRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<Permission?> GetPermissionByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _permissionRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ResultPaginationDto<Permission>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
        {
            var (items, total) = await _permissionRepository.GetPagedAsync(filter, page, pageSize, sort, cancellationToken);
            return ResultPaginationDto<Permission>.Create(items, page, pageSize, total);
        }

        public async Task<bool> ExistsByModuleApiPathMethodAsync(string module, string apiPath, string method, CancellationToken cancellationToken = default)
        {
            return await _permissionRepository.ExistsByModuleApiPathMethodAsync(module, apiPath, method, cancellationToken);
        }
    }
}
