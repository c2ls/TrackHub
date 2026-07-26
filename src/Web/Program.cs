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

using Ardalis.GuardClauses;
using Common.Application;
using Common.Web.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Reflection;
using System.Threading.RateLimiting;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Web.BackgroundServices;
using TrackHub.Manager.Web.Endpoints;
using TrackHub.Manager.Web.GraphQL.Mutation;
using TrackHub.Manager.Web.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

builder.AddTrackHubSerilog();

var allowedCORSOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string>();
Guard.Against.Null(allowedCORSOrigins, message: $"Allowed Origins configuration for CORS not loaded");

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAppSecurityContext();
builder.Services.AddAppRouterContext(builder.Configuration);
builder.Services.AddWebServices();

// Trial-expiration enforcement job.
builder.Services.AddHostedService<TrialExpirationService>();

// Document jobs: scan-result processing (quarantine → clean/infected) and the
// 30/15/7-day expiration scan.
builder.Services.AddHostedService<DocumentScanService>();
builder.Services.AddHostedService<DocumentExpirationService>();
builder.Services.AddHostedService<DocumentRetentionCleanupService>();

// Workforce job: daily 30/15/7/0-day driver-qualification expiration scan (workforce-gated accounts).
builder.Services.AddHostedService<WorkforceExpirationService>();

// Alerts/notifications jobs: 30 s delivery dispatch, 5 min alert evaluation
// (communication loss + escalation + daily credential-expiry emission), hourly digest fold, and
// daily delivery retention.
builder.Services.AddHostedService<NotificationDispatchService>();
builder.Services.AddHostedService<AlertEvaluationService>();
builder.Services.AddHostedService<NotificationDigestService>();
builder.Services.AddHostedService<DeliveryRetentionService>();

// Platform-table retention: daily purge of aged background job runs (latest row per JobKey preserved
// for the status page) and resolved alert events.
builder.Services.AddHostedService<PlatformRetentionService>();

// Add HealthChecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());

// Anonymous platform-status announcements endpoint (spec 28 ST-09): named policies, applied per
// endpoint so nothing else in the pipeline changes behavior.
builder.Services.AddOutputCache(options =>
    options.AddPolicy(PlatformStatus.CachePolicy, policy => policy.Expire(TimeSpan.FromSeconds(60))));

// Partitioned PER CLIENT IP, not a single global bucket: AddFixedWindowLimiter would share one
// 60/minute budget across every caller, so the endpoint would start rejecting once a few dozen
// portal sessions polled it — i.e. it would fail hardest during the incident it exists to report.
builder.Services.AddRateLimiter(options =>
    options.AddPolicy(PlatformStatus.RateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            // Requires UseForwardedHeaders (below) to see the real client rather than nginx.
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            })));

builder.Services.AddCors(options => options
    .AddPolicy("AllowFrontend",
        builder => builder
                    .WithOrigins(allowedCORSOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

// Configure HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365 * 2);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

// Behind nginx every request otherwise appears to come from the proxy's container IP, which would
// collapse the per-IP rate-limit partition above into a single shared bucket. Mirrors the
// AuthorityServer configuration.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseHeaderPropagation();

// Enable CORS
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

// Explicit: WebApplication would auto-insert these, but authentication must not depend on
// pipeline inference.
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(options => { });

app.UseRateLimiter();
app.UseOutputCache();

app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGraphQL();

app.Run();
