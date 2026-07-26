namespace TrackHub.Manager.Domain.Interfaces;

public interface IDriverReader
{
    Task<DriverVm> GetDriverAsync(Guid driverId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DriverVm>> GetDriversByAccountAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DriverAssignmentVm>> GetDriverAssignmentsAsync(Guid driverId, CancellationToken cancellationToken);
    Task<bool> ValidateDriverAssignmentAsync(Guid driverId, string resourceType, string resourceId, CancellationToken cancellationToken);

    /// <summary>Driver self-view; the caller pins <paramref name="driverId"/> from the principal.</summary>
    Task<MyDriverProfileVm> GetMyDriverProfileAsync(Guid driverId, CancellationToken cancellationToken);
}
