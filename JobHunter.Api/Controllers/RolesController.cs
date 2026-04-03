using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Application.Contracts.Roles;
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
    [Route("api/v1/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        [HasPermission("/api/v1/roles", "POST", "ROLES")]
        public async Task<ActionResult<Role>> Create([FromBody] ReqCreateRoleDto dto, CancellationToken cancellationToken)
        {
            if (await _roleService.ExistsByNameAsync(dto.Name, cancellationToken))
            {
                return Conflict($"Role name '{dto.Name}' already exists.");
            }
            var role = await _roleService.CreateRoleAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }

        [HttpGet]
        [HasPermission("/api/v1/roles", "GET", "ROLES")]
        public async Task<ActionResult<ResultPaginationDto<Role>>> GetAll([FromQuery] ListQueryParameters query, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetListAsync(query.Filter, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
            return Ok(roles);
        }

        [HttpPut]
        [HasPermission("/api/v1/roles", "PUT", "ROLES")]
        public async Task<ActionResult<Role>> Update([FromBody] ReqUpdateRoleDto dto, CancellationToken cancellationToken)
        {
            var role = await _roleService.UpdateRoleAsync(dto, cancellationToken);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [HasPermission("/api/v1/roles/{id}", "DELETE", "ROLES")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _roleService.DeleteRoleAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        [HasPermission("/api/v1/roles/{id}", "GET", "ROLES")]
        public async Task<ActionResult<Role>> GetById(long id, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null) return NotFound();
            return Ok(role);
        }
    }
}
