// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.S3;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;
using TrackHub.Manager.Infrastructure.ManagerDB.Storage;
using TrackHub.Manager.Infrastructure.Readers;
using TrackHub.Manager.Infrastructure.Writers;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        // Configure NpgsqlDataSourceBuilder and enable dynamic JSON serialization
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.ConfigureJsonOptions(new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(dataSource, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        services.AddTrackHubHeaderPropagation();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAccountSettingsWriter, AccountSettingsWriter>();
        services.AddScoped<IAccountSettingsReader, AccountSettingsReader>();
        services.AddScoped<IAccountWriter, AccountWriter>();
        services.AddScoped<IAccountReader, AccountReader>();
        services.AddScoped<ICredentialWriter, CredentialWriter>();
        services.AddScoped<ICredentialReader, CredentialReader>();
        services.AddScoped<ITransporterWriter, TransporterWriter>();
        services.AddScoped<ITransporterPositionWriter, TransporterPositionWriter>();
        services.AddScoped<ITransporterGroupWriter, TransporterGroupWriter>();
        services.AddScoped<IDeviceWriter, DeviceWriter>();
        services.AddScoped<IDeviceReader, DeviceReader>();
        services.AddScoped<IDeviceTransporterReader, DeviceTransporterReader>();
        services.AddScoped<ITransporterReader, TransporterReader>();
        services.AddScoped<ITransporterTypeWriter, TransporterTypeWriter>();
        services.AddScoped<ITransporterTypeReader, TransporterTypeReader>();
        services.AddScoped<IGroupWriter, GroupWriter>();
        services.AddScoped<IGroupReader, GroupReader>();
        services.AddScoped<IOperatorWriter, OperatorWriter>();
        services.AddScoped<IOperatorReader, OperatorReader>();
        services.AddScoped<IReportWriter, ReportWriter>();
        services.AddScoped<IReportReader, ReportReader>();
        services.AddScoped<IDriverReader, DriverReader>();
        services.AddScoped<IDriverWriter, DriverWriter>();
        services.AddScoped<IDriverQualificationReader, DriverQualificationReader>();
        services.AddScoped<IDriverQualificationWriter, DriverQualificationWriter>();
        services.AddScoped<IDriverAssignmentReader, DriverAssignmentReader>();
        services.AddScoped<IDriverAssignmentWriter, DriverAssignmentWriter>();
        services.AddScoped<IGroupVisibilityReader, GroupVisibilityReader>();
        services.AddScoped<IVisibleTransporterReader, VisibleTransporterReader>();
        services.AddScoped<IAccountFeatureReader, AccountFeatureReader>();
        services.AddScoped<IAccountFeatureWriter, AccountFeatureWriter>();
        services.AddScoped<IAccountFeatureMasterReader, AccountFeatureMasterReader>();
        services.AddScoped<IAccountFeatureMasterWriter, AccountFeatureMasterWriter>();
        services.AddScoped<IAuditEventReader, AuditEventReader>();
        services.AddScoped<IAuditEventWriter, AuditEventWriter>();
        services.AddScoped<IDocumentReader, DocumentReader>();
        services.AddScoped<IDocumentWriter, DocumentWriter>();
        services.AddScoped<IDocumentAccessPolicy, DocumentAccessPolicy>();

        // Document storage provider selected by config: LocalFileSystem (dev), S3, or
        // AzureBlob. A real AV provider is a separate deployment blocker; dev uses the no-op scanner.
        AddDocumentStorage(services, configuration);
        services.AddSingleton<IDocumentScanner, NoOpDocumentScanner>();
        services.AddScoped<INotificationReader, NotificationReader>();
        services.AddScoped<INotificationWriter, NotificationWriter>();
        services.AddScoped<IAlertSubscriptionReader, AlertSubscriptionReader>();
        services.AddScoped<IAlertSubscriptionWriter, AlertSubscriptionWriter>();
        services.AddScoped<INotificationTemplateReader, NotificationTemplateReader>();
        services.AddScoped<INotificationTemplateWriter, NotificationTemplateWriter>();
        services.AddScoped<IAlertRuleEvaluator, TrackHub.Manager.Infrastructure.ManagerDB.Services.AlertRuleEvaluator>();

        // Notification channel providers. Push is contract-only for now.
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        services.AddHttpClient(WebhookNotificationProvider.HttpClientName, client => client.Timeout = TimeSpan.FromSeconds(10));
        services.AddHttpClient(WhatsAppNotificationProvider.HttpClientName, client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<INotificationChannelProvider, InAppNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, EmailNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, WebhookNotificationProvider>();
        services.AddScoped<INotificationChannelProvider, WhatsAppNotificationProvider>();
        services.AddScoped<IAlertEventReader, AlertEventReader>();
        services.AddScoped<IAlertEventWriter, AlertEventWriter>();
        services.AddScoped<IBackgroundJobRunReader, BackgroundJobRunReader>();
        services.AddScoped<IBackgroundJobStatusReader, BackgroundJobStatusReader>();
        services.AddScoped<IPlatformAnnouncementReader, PlatformAnnouncementReader>();
        services.AddScoped<IPlatformAnnouncementWriter, PlatformAnnouncementWriter>();
        services.AddScoped<IBackgroundJobRunWriter, BackgroundJobRunWriter>();
        services.AddScoped<IPublicLinkGrantReader, PublicLinkGrantReader>();
        services.AddScoped<IPublicLinkGrantWriter, PublicLinkGrantWriter>();
        services.AddScoped<IPublicLinkGrantResolver, PublicLinkGrantResolver>();
        services.AddScoped<IAccountSupportGrantReader, AccountSupportGrantReader>();
        services.AddScoped<IAccountSupportGrantWriter, AccountSupportGrantWriter>();
        services.AddScoped<IUserWriter, UserWriter>();
        services.AddScoped<IUserReader, UserReader>();
        services.AddScoped<IUserSettingsWriter, UserSettingsWriter>();
        services.AddScoped<IUserSettingsReader, UserSettingsReader>();
        services.AddScoped<IUserGroupWriter, UserGroupWriter>();
        services.AddScoped<ITransporterDeviceAssignmentReader, TransporterDeviceAssignmentReader>();
        services.AddScoped<ITransporterDeviceAssignmentWriter, TransporterDeviceAssignmentWriter>();
        services.AddScoped<IPointOfInterestReader, PointOfInterestReader>();
        services.AddScoped<IPointOfInterestWriter, PointOfInterestWriter>();
        services.AddScoped<IGeocodingProviderReader, GeocodingProviderReader>();
        services.AddScoped<IGeocodingProviderWriter, GeocodingProviderWriter>();
        services.AddScoped<IGpsIntegrationDashboardReader, GpsIntegrationDashboardReader>();
        services.AddMemoryCache();
        services.AddScoped<Common.Application.Interfaces.IFeatureFlagService, TrackHub.Manager.Infrastructure.ManagerDB.Services.FeatureFlagService>();

        // Account lifecycle + branding.
        services.AddScoped<IAccountStatusWriter, AccountStatusWriter>();
        services.AddScoped<IAccountBrandingReader, AccountBrandingReader>();
        services.AddScoped<IAccountBrandingWriter, AccountBrandingWriter>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusReader, AccountOperationalStatusReader>();
        services.AddScoped<Common.Application.Interfaces.IAccountOperationalStatusService, Common.Application.Services.CachedAccountOperationalStatusService>();

        return services;
    }

    // Registers the IDocumentStorage implementation chosen by DocumentStorage:Provider
    // (LocalFileSystem | S3 | AzureBlob). The client is a singleton; the local FS default is a writable
    // OS-temp path (the app directory is read-only under IIS).
    private static void AddDocumentStorage(IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration.GetValue<string>("DocumentStorage:Provider") ?? LocalFileSystemDocumentStorage.ProviderName).Trim();

        switch (provider)
        {
            case S3DocumentStorage.ProviderName:
                services.AddSingleton<IDocumentStorage>(BuildS3Storage(configuration));
                break;

            case AzureBlobDocumentStorage.ProviderName:
                services.AddSingleton<IDocumentStorage>(BuildAzureStorage(configuration));
                break;

            default:
                var configuredRoot = configuration.GetValue<string>("DocumentStorage:LocalRootPath");
                var localRoot = string.IsNullOrWhiteSpace(configuredRoot)
                    ? Path.Combine(Path.GetTempPath(), "trackhub-documents")
                    : configuredRoot;
                services.AddSingleton<IDocumentStorage>(sp =>
                    new LocalFileSystemDocumentStorage(localRoot, sp.GetRequiredService<ILogger<LocalFileSystemDocumentStorage>>()));
                break;
        }
    }

    private static Func<IServiceProvider, IDocumentStorage> BuildS3Storage(IConfiguration configuration)
    {
        // Blank is treated as absent: container orchestration supplies unset keys as empty strings.
        var bucket = configuration.GetValue<string>("DocumentStorage:S3:BucketName");
        if (string.IsNullOrWhiteSpace(bucket))
        {
            throw new InvalidOperationException("DocumentStorage:S3:BucketName is required when Provider = S3.");
        }
        var region = configuration.GetValue<string>("DocumentStorage:S3:Region");
        var serviceUrl = configuration.GetValue<string>("DocumentStorage:S3:ServiceUrl");
        var accessKey = configuration.GetValue<string>("DocumentStorage:S3:AccessKey");
        var secretKey = configuration.GetValue<string>("DocumentStorage:S3:SecretKey");
        var forcePathStyle = configuration.GetValue<bool?>("DocumentStorage:S3:ForcePathStyle") ?? !string.IsNullOrWhiteSpace(serviceUrl);
        var ttl = TimeSpan.FromMinutes(configuration.GetValue<int?>("DocumentStorage:S3:PresignedExpiryMinutes") ?? 5);

        var s3Config = new AmazonS3Config { ForcePathStyle = forcePathStyle };
        if (!string.IsNullOrWhiteSpace(region))
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
        }
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            s3Config.ServiceURL = serviceUrl;
        }

        // Static keys when supplied; otherwise the default credential chain (IAM role / env / profile).
        IAmazonS3 client = !string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey)
            ? new AmazonS3Client(accessKey, secretKey, s3Config)
            : new AmazonS3Client(s3Config);

        return sp => new S3DocumentStorage(client, bucket, ttl, sp.GetRequiredService<ILogger<S3DocumentStorage>>());
    }

    private static Func<IServiceProvider, IDocumentStorage> BuildAzureStorage(IConfiguration configuration)
    {
        // Blank is treated as absent: container orchestration supplies unset keys as empty strings.
        var containerName = configuration.GetValue<string>("DocumentStorage:AzureBlob:ContainerName");
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException("DocumentStorage:AzureBlob:ContainerName is required when Provider = AzureBlob.");
        }

        var connectionString = configuration.GetValue<string>("DocumentStorage:AzureBlob:ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DocumentStorage:AzureBlob:ConnectionString is required when Provider = AzureBlob.");
        }
        var ttl = TimeSpan.FromMinutes(configuration.GetValue<int?>("DocumentStorage:AzureBlob:SasExpiryMinutes") ?? 5);

        var container = new BlobContainerClient(connectionString, containerName);
        return sp => new AzureBlobDocumentStorage(container, ttl, sp.GetRequiredService<ILogger<AzureBlobDocumentStorage>>());
    }
}
