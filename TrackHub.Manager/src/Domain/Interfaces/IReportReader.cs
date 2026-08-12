namespace TrackHub.Manager.Domain.Interfaces;

public interface IReportReader
{
    Task<IReadOnlyCollection<ReportVm>> GetReportsAsync(CancellationToken cancellationToken);

    Task<ReportVm?> GetReportByCodeAsync(string code, CancellationToken cancellationToken);
}
