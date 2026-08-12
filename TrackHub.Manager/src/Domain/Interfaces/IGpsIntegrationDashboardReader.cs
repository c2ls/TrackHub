using TrackHub.Manager.Domain.Models;

namespace TrackHub.Manager.Domain.Interfaces;

public interface IGpsIntegrationDashboardReader
{
    Task<GpsIntegrationDashboardVm> GetAsync(Guid accountId, CancellationToken cancellationToken);
}
