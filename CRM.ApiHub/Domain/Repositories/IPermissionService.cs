namespace CRM.ApiHub.Domain.Repositories;

public interface IPermissionService
{
    Task<bool> CanUserActionAsync(int userId, string permissionKey, int statusId);
}