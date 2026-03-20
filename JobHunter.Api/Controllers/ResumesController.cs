using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Resumes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobHunter.Api.Authorization;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ResumesController : ControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumesController(IResumeService resumeService)
    {
        _resumeService = resumeService;
    }

    [HttpPost]
    [HasPermission("resume:create")]
    public async Task<IActionResult> Create([FromBody] ReqCreateResumeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _resumeService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [HasPermission("resume:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateResumeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _resumeService.UpdateAsync(dto, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [HasPermission("resume:delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _resumeService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await _resumeService.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = $"Resume with id {id} not found" });
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetList([FromQuery] string? filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _resumeService.GetListAsync(filter, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("by-user")]
    [Authorize]
    public async Task<IActionResult> GetByUser([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // Extract userId from claims (assume sub claim is userId)
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type.EndsWith("/nameidentifier"));
        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "User ID not found in token" });
        var result = await _resumeService.GetByUserAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
