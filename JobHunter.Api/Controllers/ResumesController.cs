using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Resumes;
using JobHunter.Api.Contracts;
using System.Security.Claims;
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
    [HasPermission("/api/v1/resumes", "POST", "RESUMES")]
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
    [HasPermission("/api/v1/resumes", "PUT", "RESUMES")]
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
    [HasPermission("/api/v1/resumes/{id}", "DELETE", "RESUMES")]
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
    public async Task<IActionResult> GetList([FromQuery] ListQueryParameters query, CancellationToken cancellationToken = default)
    {
        var result = await _resumeService.GetListAsync(query.Filter, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
        return Ok(result);
    }

    [HttpPost("by-user")]
    [Authorize]
    public async Task<IActionResult> GetByUser([FromQuery] ListQueryParameters query, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User ID not found in token" });

        var result = await _resumeService.GetByUserAsync(userId, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
        return Ok(result);
    }
}
