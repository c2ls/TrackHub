namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountSupportGrantWriter
{
    Task<AccountSupportGrantVm> CreateAccountSupportGrantAsync(AccountSupportGrantDto accountSupportGrant, CancellationToken cancellationToken);
    Task ApproveAccountSupportGrantAsync(Guid accountSupportGrantId, string approvedBy, CancellationToken cancellationToken);
    Task RevokeAccountSupportGrantAsync(Guid accountSupportGrantId, string revokedBy, CancellationToken cancellationToken);
}
