using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Jobs;
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
        // Implementation placeholder
        return Ok();
    }

    [HttpPut]
    [HasPermission("job:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateJobDto dto, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpDelete("{id}")]
    [HasPermission("job:delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetList([FromQuery] string? filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Ok();
    }
}
