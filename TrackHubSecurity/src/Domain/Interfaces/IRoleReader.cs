namespace TrackHub.Security.Domain.Interfaces;

public interface IRoleReader
{
    Task<RoleVm> GetRoleAsync(string name, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RoleVm>> GetRolesAsync(CancellationToken cancellationToken);
    Task<RoleResourceVm> GetResourcesAsync(int roleId, CancellationToken cancellationToken);
}
