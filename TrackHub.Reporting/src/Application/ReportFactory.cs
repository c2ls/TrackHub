using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces.Factory;

namespace TrackHub.Reporting.Application;

public class ReportFactory(IServiceScopeFactory scopeFactory) : IReportFactory
{
    public IReport GetReport(string reportCode)
    {
        using var scope = scopeFactory.CreateScope();
        return scope.ServiceProvider.GetServices<IReport>()
            .FirstOrDefault(reader => reader.ReportCode.Equals(reportCode))
            ?? throw new ReportNotFoundException(reportCode);
    }
}
