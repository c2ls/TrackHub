using TrackHub.Reporting.Infrastructure.GraphQLApi;
using TrackHub.Reporting.Domain.Interfaces.Router;
using TrackHub.Reporting.Domain.Interfaces.Geofence;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Interfaces.Telemetry;
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Common.Application.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsContext(this IServiceCollection services, IConfiguration configuration)
    {
        // Geofence/Telemetry/TripManagement carry queries only — full resilience incl. retry. No retry
        // for Manager (the report-audit mutation) or Router (provider reads are rate-limited per user,
        // and each retry would spend that budget).
        services.AddGraphQLClient(Clients.Router);
        services.AddGraphQLClient(Clients.Geofence, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLClient(Clients.Manager);
        services.AddGraphQLClient(Clients.Telemetry, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLClient(Clients.TripManagement, resilience: GraphQLClientResilience.WithRetry);
        services.AddGraphQLServiceClient(Clients.Manager);

        services.AddScoped<IRouterReader, RouterReader>();
        services.AddScoped<IGeofenceReader, GeofenceReader>();
        services.AddScoped<IGpsManagerReader, GpsManagerReader>();
        services.AddScoped<IReportNameReader, ReportNameReader>();
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
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, Common.Infrastructure.ManagerAccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        // Module discovery seam: registers any IServiceModule implementations shipped in
        // this assembly (none in this repository).
        services.AddDiscoveredModules(typeof(RouterReader).Assembly, configuration);

        return services;
    }
}

