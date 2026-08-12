namespace TrackHub.Manager.Domain.Interfaces;

public interface ITransporterTypeReader
{
    Task<TransporterTypeVm> GetTransporterTypeAsync(short transporterTypeId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TransporterTypeVm>> GetTransporterTypesAsync(CancellationToken cancellationToken);
}
