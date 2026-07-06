using System.Threading;
using System.Threading.Tasks;
using CRM.ApiHub.Domain.Entities;
using CRM.ApiHub.Domain.Repositories;

namespace CRM.ApiHub.Application.UseCases.Auth;

public class MeUseCase
{
    private readonly IUserRepository _userRepository;

    public MeUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDetail?> ExecuteAsync(long userId, CancellationToken ct = default)
    {
        return await _userRepository.GetUserDetailByIdAsync(userId, ct);
    }
}
