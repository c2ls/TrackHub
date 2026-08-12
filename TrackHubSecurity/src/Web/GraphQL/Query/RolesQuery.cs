using TrackHub.Security.Application.Roles.GetAll;
using TrackHub.Security.Application.Roles.GetResources;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Query;

public partial class Query
{

    public async Task<IReadOnlyCollection<RoleVm>> GetRoles([Service] ISender sender)
        => await sender.Send(new GetRolesQuery());

    public async Task<RoleResourceVm> GetResourcesByRole([Service] ISender sender, [AsParameters] GetResourcesByRoleQuery query)
        => await sender.Send(query);

}
