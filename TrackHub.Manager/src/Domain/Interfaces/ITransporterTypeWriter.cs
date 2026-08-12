using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Domain.Interfaces;

public interface ITransporterTypeWriter
{
    Task UpdateTransporterTypeAsync(TransporterTypeDto transporterTypeDto, CancellationToken cancellationToken);
}
