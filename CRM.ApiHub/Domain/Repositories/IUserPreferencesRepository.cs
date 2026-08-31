using System.Threading;
using System.Threading.Tasks;

namespace CRM.ApiHub.Domain.Repositories;

public interface IUserPreferencesRepository
{
    Task<string?> GetThemeAsync(long userId, CancellationToken ct = default);
    Task SaveThemeAsync(long userId, string themeName, CancellationToken ct = default);
}
