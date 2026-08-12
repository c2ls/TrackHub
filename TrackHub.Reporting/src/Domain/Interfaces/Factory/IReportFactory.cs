namespace TrackHub.Reporting.Domain.Interfaces.Factory;

public interface IReportFactory
{
    IReport GetReport(string reportCode);
}

