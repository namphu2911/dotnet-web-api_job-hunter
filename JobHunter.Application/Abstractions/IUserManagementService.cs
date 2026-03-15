using JobHunter.Application.Contracts.Users;

namespace JobHunter.Application.Abstractions;

public interface IUserManagementService
{
    Task<ResCreateUserDto> CreateUserAsync(ReqCreateUserDto request, CancellationToken cancellationToken = default);

    Task<ResUpdateUserDto?> UpdateUserAsync(ReqUpdateUserDto request, CancellationToken cancellationToken = default);

    Task<ResUserDto?> GetUserByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<ResultPaginationDto<ResUserDto>> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default);
}
