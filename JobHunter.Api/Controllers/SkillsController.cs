using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Skills;
using JobHunter.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JobHunter.Api.Controllers
{
    [ApiController]
    [Route("api/v1/skills")]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillsController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Skill>> Create([FromBody] ReqCreateSkillDto dto, CancellationToken cancellationToken)
        {
            if (await _skillService.ExistsByNameAsync(dto.Name, cancellationToken))
            {
                return Conflict($"Skill name '{dto.Name}' already exists.");
            }
            var skill = await _skillService.CreateSkillAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = skill.Id }, skill);
        }

        [HttpGet]
        public async Task<ActionResult<List<Skill>>> GetAll(CancellationToken cancellationToken)
        {
            var skills = await _skillService.GetSkillsAsync(cancellationToken);
            return Ok(skills);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Skill>> Update([FromBody] ReqUpdateSkillDto dto, CancellationToken cancellationToken)
        {
            var skill = await _skillService.UpdateSkillAsync(dto, cancellationToken);
            if (skill == null) return NotFound();
            return Ok(skill);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            await _skillService.DeleteSkillAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetById(long id, CancellationToken cancellationToken)
        {
            var skill = await _skillService.GetSkillByIdAsync(id, cancellationToken);
            if (skill == null) return NotFound();
            return Ok(skill);
        }
    }
}
