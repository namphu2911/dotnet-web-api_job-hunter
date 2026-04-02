using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Roles;
using JobHunter.Application.Utilities;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;

        public RoleService(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
        }

        public async Task<Role> CreateRoleAsync(ReqCreateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                Active = dto.Active
            };
            if (dto.Permissions != null && dto.Permissions.Count > 0)
            {
                var permissionIds = FlexibleIdParser.ParseList(dto.Permissions, "permissions");
                var permissions = await _permissionRepository.FindByIdsAsync(permissionIds, cancellationToken);
                foreach (var p in permissions)
                {
                    role.Permissions.Add(p);
                }
            }
            await _roleRepository.AddAsync(role, cancellationToken);
            return role;
        }

        public async Task<Role?> UpdateRoleAsync(ReqUpdateRoleDto dto, CancellationToken cancellationToken = default)
        {
            var role = await _roleRepository.GetByIdAsync(dto.Id, cancellationToken);
            if (role == null) return null;
            role.Name = dto.Name;
            role.Description = dto.Description;
            role.Active = dto.Active;
            if (dto.Permissions != null)
            {
                role.Permissions.Clear();
                if (dto.Permissions.Count > 0)
                {
                    var permissionIds = FlexibleIdParser.ParseList(dto.Permissions, "permissions");
                    var permissions = await _permissionRepository.FindByIdsAsync(permissionIds, cancellationToken);
                    foreach (var p in permissions)
                    {
                        role.Permissions.Add(p);
                    }
                }
            }
            await _roleRepository.UpdateAsync(role, cancellationToken);
            return role;
        }

        public async Task DeleteRoleAsync(long id, CancellationToken cancellationToken = default)
        {
            await _roleRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<Role?> GetRoleByIdAsync(long id, CancellationToken cancellationToken = default)
        {
            return await _roleRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<ResultPaginationDto<Role>> GetListAsync(string? filter, int page, int pageSize, string? sort, CancellationToken cancellationToken = default)
        {
            var (items, total) = await _roleRepository.GetPagedAsync(filter, page, pageSize, sort, cancellationToken);
            return ResultPaginationDto<Role>.Create(items, page, pageSize, total);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _roleRepository.ExistsByNameAsync(name, cancellationToken);
        }
    }
}
