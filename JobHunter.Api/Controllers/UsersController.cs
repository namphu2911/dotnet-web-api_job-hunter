using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Users;
using JobHunter.Api.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpPost]
    [ApiMessage("Create user successfully")]
    [ProducesResponseType(typeof(ResCreateUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResCreateUserDto>> CreateUser([FromBody] ReqCreateUserDto request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _userManagementService.CreateUserAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (InvalidOperationException)
        {
            return Conflict($"Email {request.Email} already exists.");
        }
    }

    [HttpDelete("{id:long}")]
    [ApiMessage("Delete user successfully")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser([FromRoute] long id, CancellationToken cancellationToken)
    {
        var deleted = await _userManagementService.DeleteUserAsync(id, cancellationToken);
        return deleted ? Ok() : NotFound($"User with id {id} does not exist.");
    }

    [HttpGet("{id:long}")]
    [ApiMessage("Fetch user successfully")]
    [ProducesResponseType(typeof(ResUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResUserDto>> GetUserById([FromRoute] long id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        return user is null ? NotFound($"User with id {id} does not exist.") : Ok(user);
    }

    [HttpGet]
    [ApiMessage("Fetch users successfully")]
    [ProducesResponseType(typeof(ResultPaginationDto<ResUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultPaginationDto<ResUserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var users = await _userManagementService.GetUsersAsync(page, pageSize, cancellationToken);
        return Ok(users);
    }

    [HttpPut]
    [ApiMessage("Update user successfully")]
    [ProducesResponseType(typeof(ResUpdateUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResUpdateUserDto>> UpdateUser([FromBody] ReqUpdateUserDto request, CancellationToken cancellationToken)
    {
        var updated = await _userManagementService.UpdateUserAsync(request, cancellationToken);
        return updated is null ? NotFound($"User with id {request.Id} does not exist.") : Ok(updated);
    }
}