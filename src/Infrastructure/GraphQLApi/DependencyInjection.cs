using TrackHub.Reporting.Infrastructure.GraphQLApi;
using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Interfaces.Geofence;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsContext(this IServiceCollection services, IConfiguration configuration)
    {
        // Router/Geofence/Telemetry/TripManagement carry queries only — full resilience incl. retry.
        // Manager also carries the report-audit mutation — retries stay off there.
        services.AddGraphQLClient(Clients.Router, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLClient(Clients.Geofence, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLClient(Clients.Manager);
        services.AddGraphQLClient(Clients.Telemetry, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLClient(Clients.TripManagement, resilience: GraphQLClientResilience.WithRetry);

        services.AddScoped<IRouterReader, RouterReader>();
        services.AddScoped<IGeofenceReader, GeofenceReader>();
        services.AddScoped<IGpsManagerReader, GpsManagerReader>();
        services.AddScoped<IGpsTelemetryReader, GpsTelemetryReader>();
        services.AddScoped<IAccountFeatureReader, AccountFeatureReader>();
        services.AddScoped<IReportAuditWriter, ReportAuditWriter>();
        services.AddScoped<IAdminReportReader, AdminReportReader>();
        services.AddScoped<IDocumentReportReader, DocumentReportReader>();
        services.AddScoped<IWorkforceReportReader, WorkforceReportReader>();
        services.AddScoped<ITripReportReader, TripReportReader>();
        services.AddScoped<IReportCatalogReader, ReportCatalogReader>();
        services.AddScoped<IReportBrandingReader, ReportBrandingReader>();

        // Cross-service account-status enforcement.
        services.AddMemoryCache();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, AccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        return services;
    }
}

