using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Permissions;
using JobHunter.Api.Authorization;
using JobHunter.Api.Contracts;
using JobHunter.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Api.Controllers
{
    [ApiController]
    [Route("api/v1/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpPost]
        [HasPermission("/api/v1/permissions", "POST", "PERMISSIONS")]
        public async Task<ActionResult<Permission>> Create([FromBody] ReqCreatePermissionDto dto, CancellationToken cancellationToken)
        {
            if (await _permissionService.ExistsByModuleApiPathMethodAsync(dto.Module, dto.ApiPath, dto.Method, cancellationToken))
            {
                return Conflict($"Permission already exists for module={dto.Module}, apiPath={dto.ApiPath}, method={dto.Method}.");
            }
            var permission = await _permissionService.CreatePermissionAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
        }

        [HttpGet]
        [HasPermission("/api/v1/permissions", "GET", "PERMISSIONS")]
        public async Task<ActionResult<ResultPaginationDto<Permission>>> GetAll([FromQuery] ListQueryParameters query, CancellationToken cancellationToken)
        {
            var permissions = await _permissionService.GetListAsync(query.Filter, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
            return Ok(permissions);
        }

        [HttpPut]
        [HasPermission("/api/v1/permissions", "PUT", "PERMISSIONS")]
        public async Task<ActionResult<Permission>> Update([FromBody] ReqUpdatePermissionDto dto, CancellationToken cancellationToken)
        {
            var permission = await _permissionService.UpdatePermissionAsync(dto, cancellationToken);
            if (permission == null) return NotFound();
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        [HasPermission("/api/v1/permissions/{id}", "DELETE", "PERMISSIONS")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _permissionService.DeletePermissionAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        [HasPermission("/api/v1/permissions/{id}", "GET", "PERMISSIONS")]
        public async Task<ActionResult<Permission>> GetById(long id, CancellationToken cancellationToken)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (permission == null) return NotFound();
            return Ok(permission);
        }
    }
}
