namespace TrackHub.Manager.Domain.Interfaces;

public interface IDriverWriter
{
    Task<DriverVm> CreateDriverAsync(DriverDto driver, CancellationToken cancellationToken);
    Task UpdateDriverAsync(Guid driverId, DriverDto driver, CancellationToken cancellationToken);
    Task DeactivateDriverAsync(Guid driverId, CancellationToken cancellationToken);
}
