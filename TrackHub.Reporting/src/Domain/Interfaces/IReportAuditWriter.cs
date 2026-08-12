namespace TrackHub.Reporting.Domain.Interfaces;

public interface IReportAuditWriter
{
    Task RecordReportExportAsync(
        Guid accountId,
        string actorType,
        string actorId,
        string reportCode,
        string filtersJson,
        int rowCount,
        string format,
        string? correlationId,
        CancellationToken cancellationToken);
}

