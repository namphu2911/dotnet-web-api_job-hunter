using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Jobs;
using JobHunter.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JobHunter.Api.Authorization;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    [HasPermission("job:create")]
    public async Task<IActionResult> Create([FromBody] ReqCreateJobDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _jobService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut]
    [HasPermission("job:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateJobDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _jobService.UpdateAsync(dto, cancellationToken);
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
    [HasPermission("job:delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _jobService.DeleteAsync(id, cancellationToken);
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
        var result = await _jobService.GetByIdAsync(id, cancellationToken);
        if (result == null)
            return NotFound(new { message = $"Job with id {id} not found" });
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetList([FromQuery] ListQueryParameters query, CancellationToken cancellationToken = default)
    {
        var result = await _jobService.GetListAsync(query.Filter, query.ResolvePage(), query.ResolvePageSize(), query.Sort, cancellationToken);
        return Ok(result);
    }
}
