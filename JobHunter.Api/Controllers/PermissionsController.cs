using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Permissions;
using JobHunter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
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
        public async Task<ActionResult<List<Permission>>> GetAll(CancellationToken cancellationToken)
        {
            var permissions = await _permissionService.GetPermissionsAsync(cancellationToken);
            return Ok(permissions);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Permission>> Update([FromBody] ReqUpdatePermissionDto dto, CancellationToken cancellationToken)
        {
            var permission = await _permissionService.UpdatePermissionAsync(dto, cancellationToken);
            if (permission == null) return NotFound();
            return Ok(permission);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _permissionService.DeletePermissionAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetById(long id, CancellationToken cancellationToken)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id, cancellationToken);
            if (permission == null) return NotFound();
            return Ok(permission);
        }
    }
}
