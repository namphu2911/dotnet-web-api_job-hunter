using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts.Users;
using JobHunter.Application.Services.Security;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IUserRepository _userRepository;

    public UserManagementService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ResCreateUserDto> CreateUserAsync(ReqCreateUserDto request, CancellationToken cancellationToken = default)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException($"Email {request.Email} already exists.");
        }

        var user = new User
        {
            Name = request.Name ?? string.Empty,
            Email = request.Email,
            Password = PasswordSecurity.HashPassword(request.Password),
            Age = request.Age,
            Gender = request.Gender,
            Address = request.Address,
            Avatar = "default-avatar.png",
            CompanyId = request.Company?.Id,
            RoleId = request.Role?.Id,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.AddAsync(user, cancellationToken);
        return MapCreateUser(created);
    }

    public async Task<ResUpdateUserDto?> UpdateUserAsync(ReqUpdateUserDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = request.Name ?? existing.Name;
        existing.Age = request.Age;
        existing.Gender = request.Gender;
        existing.Address = request.Address;
        existing.Avatar = request.Avatar;
        existing.CompanyId = request.Company?.Id;
        existing.RoleId = request.Role?.Id;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _userRepository.UpdateAsync(existing, cancellationToken);
        return updated is null ? null : MapUpdateUser(updated);
    }

    public async Task<ResUserDto?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : MapUser(user);
    }

    public async Task<ResultPaginationDto<ResUserDto>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var users = await _userRepository.GetUsersAsync(page, pageSize, cancellationToken);
        var total = await _userRepository.CountUsersAsync(cancellationToken);
        var pages = (int)Math.Ceiling(total / (double)pageSize);

        return new ResultPaginationDto<ResUserDto>
        {
            Meta = new ResultPaginationDto<ResUserDto>.MetaDto
            {
                Page = page,
                PageSize = pageSize,
                Pages = pages,
                Total = total
            },
            Result = users.Select(MapUser).ToList()
        };
    }

    public Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return _userRepository.DeleteAsync(id, cancellationToken);
    }

    internal static ResUserDto MapUser(User user)
    {
        return new ResUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Age = user.Age,
            Gender = user.Gender,
            Address = user.Address,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Company = user.Company is null ? null : new ResObjectIdNameDto { Id = user.Company.Id, Name = user.Company.Name },
            Role = user.Role is null ? null : new ResObjectIdNameDto { Id = user.Role.Id, Name = user.Role.Name }
        };
    }

    private static ResCreateUserDto MapCreateUser(User user)
    {
        return new ResCreateUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Age = user.Age,
            Gender = user.Gender,
            Address = user.Address,
            CreatedAt = user.CreatedAt,
            Company = user.Company is null ? null : new ResObjectIdNameDto { Id = user.Company.Id, Name = user.Company.Name }
        };
    }

    private static ResUpdateUserDto MapUpdateUser(User user)
    {
        return new ResUpdateUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Age = user.Age,
            Gender = user.Gender,
            Address = user.Address,
            Avatar = user.Avatar,
            UpdatedAt = user.UpdatedAt,
            Company = user.Company is null ? null : new ResObjectIdNameDto { Id = user.Company.Id, Name = user.Company.Name }
        };
    }
}
