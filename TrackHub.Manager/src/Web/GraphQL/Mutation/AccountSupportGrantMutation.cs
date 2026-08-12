using TrackHub.Manager.Application.AccountSupportGrants.Commands;

namespace TrackHub.Manager.Web.GraphQL.Mutation;

public partial class Mutation
{
    public async Task<AccountSupportGrantVm> CreateAccountSupportGrant([Service] ISender sender, CreateAccountSupportGrantCommand command, CancellationToken cancellationToken) => await sender.Send(command, cancellationToken);
    public async Task<bool> ApproveAccountSupportGrant([Service] ISender sender, ApproveAccountSupportGrantCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
    public async Task<bool> RevokeAccountSupportGrant([Service] ISender sender, RevokeAccountSupportGrantCommand command, CancellationToken cancellationToken) { await sender.Send(command, cancellationToken); return true; }
}
