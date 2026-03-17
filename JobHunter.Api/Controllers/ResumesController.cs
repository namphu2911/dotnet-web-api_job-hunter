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
        // Implementation placeholder
        return Ok();
    }

    [HttpPut]
    [HasPermission("resume:update")]
    public async Task<IActionResult> Update([FromBody] ReqUpdateResumeDto dto, CancellationToken cancellationToken)
    {
        // Implementation placeholder
        return Ok();
    }

    [HttpDelete("{id}")]
    [HasPermission("resume:delete")]
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

    [HttpPost("by-user")]
    [Authorize]
    public async Task<IActionResult> GetByUser([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // Implementation placeholder
        return Ok();
    }
}
