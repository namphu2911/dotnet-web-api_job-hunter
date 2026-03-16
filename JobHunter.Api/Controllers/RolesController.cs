using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Roles;
using JobHunter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
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
        public async Task<ActionResult<List<Role>>> GetAll(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetRolesAsync(cancellationToken);
            return Ok(roles);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Role>> Update([FromBody] ReqUpdateRoleDto dto, CancellationToken cancellationToken)
        {
            var role = await _roleService.UpdateRoleAsync(dto, cancellationToken);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _roleService.DeleteRoleAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetById(long id, CancellationToken cancellationToken)
        {
            var role = await _roleService.GetRoleByIdAsync(id, cancellationToken);
            if (role == null) return NotFound();
            return Ok(role);
        }
    }
}
