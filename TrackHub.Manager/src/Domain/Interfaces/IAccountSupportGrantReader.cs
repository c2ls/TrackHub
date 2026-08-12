namespace TrackHub.Manager.Domain.Interfaces;

public interface IAccountSupportGrantReader
{
    Task<AccountSupportGrantVm> GetSupportGrantStatusAsync(Guid accountSupportGrantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccountSupportGrantVm>> GetAccountSupportGrantsAsync(Guid? accountId, int skip, int take, CancellationToken cancellationToken);
}
