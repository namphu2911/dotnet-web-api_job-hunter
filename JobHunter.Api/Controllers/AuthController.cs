using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Auth;
using JobHunter.Application.Contracts.Users;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ResLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResLoginDto>> Login([FromBody] ReqLoginDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (result is null)
        {
            return BadRequest("Invalid username or password.");
        }

        SetRefreshTokenCookie(result.RefreshTokenInternal);
        return Ok(result);
    }

    [HttpGet("account")]
    [ProducesResponseType(typeof(ResLoginDto.UserGetAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResLoginDto.UserGetAccountDto>> GetAccount(CancellationToken cancellationToken)
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        var result = await _authService.GetAccountAsync(authorizationHeader, cancellationToken);

        return result is null ? Unauthorized() : Ok(result);
    }

    [HttpGet("refresh")]
    [ProducesResponseType(typeof(ResLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResLoginDto>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        var result = await _authService.RefreshAsync(refreshToken, cancellationToken);
        if (result is null)
        {
            return BadRequest("Refresh token is invalid or expired.");
        }

        SetRefreshTokenCookie(result.RefreshTokenInternal);
        return Ok(result);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        await _authService.LogoutAsync(authorizationHeader, cancellationToken);

        Response.Cookies.Delete("refresh_token");
        return Ok();
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ResCreateUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ResCreateUserDto>> Register([FromBody] ReqCreateUserDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (InvalidOperationException)
        {
            return Conflict($"Email {request.Email} already exists.");
        }
    }

    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ResLoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ResLoginDto>> ChangePassword([FromBody] ReqChangePasswordDto request, CancellationToken cancellationToken)
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(authorizationHeader, request, cancellationToken);
        if (result is null)
        {
            return BadRequest("Invalid old password or session.");
        }

        SetRefreshTokenCookie(result.RefreshTokenInternal);
        return Ok(result);
    }

    private void SetRefreshTokenCookie(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = TimeSpan.FromDays(1)
        });
    }
}
