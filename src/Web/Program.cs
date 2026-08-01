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
using Microsoft.AspNetCore.HttpOverrides;
using System.Reflection;
using TrackHub.TripManagement.Infrastructure.TripDB;
using TrackHub.TripManagement.Web.BackgroundServices;
using TrackHub.TripManagement.Web.Endpoints;
using TrackHub.TripManagement.Web.GraphQL.Mutation;
using TrackHub.TripManagement.Web.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

builder.AddTrackHubSerilog();

var allowedCORSOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string>();
Guard.Against.Null(allowedCORSOrigins, message: $"Allowed Origins configuration for CORS not loaded");

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddManagerApiContext();
builder.Services.AddTelemetryApiContext();
builder.Services.AddRoutingApiContext(builder.Configuration);
// Registers IGraphQLClientFactory, which every ManagerApi/TelemetryApi client above takes in its
// constructor. Without it the service starts and answers /health, but the first request touching
// AlertEmitter, ManagerValidationClient, PublicLinkGrantClient, DocumentClient or
// PositionHistoryClient fails DI activation and surfaces as "Unexpected Execution Error", and both
// hosted jobs throw every cycle.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();

// The module's two hosted jobs. Both are on-work-only recorders (SVD-11): an old BackgroundJobRun
// row for their keys is the healthy steady state, not a stuck job.
builder.Services.AddHostedService<TripEtaRefreshService>();
builder.Services.AddHostedService<TripScheduleReminderService>();

// Add HealthChecks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment());

// Anonymous public tracking endpoint: a named rate-limit policy applied per endpoint, so nothing
// else in the pipeline changes behavior. There is deliberately NO output cache here — see the
// comment on PublicTrips: a response cache cannot satisfy acceptance 24's "every successful
// resolution increments the access count and writes one PublicLinkAccessed audit event", and it
// kept revoked links alive for the cache window.
//
// Partitioned PER CLIENT IP, not one global bucket: a single shared limiter would start rejecting
// as soon as a few dozen customers had a tracking page open, which is exactly when the endpoint
// needs to work.
// The partitioning itself lives in Common.Web so this and Manager's platform-status feed cannot
// drift apart; it also requires UseForwardedHeaders (below) to see the real client rather than nginx.
builder.Services.AddAnonymousEndpointRateLimiter(
    PublicTrips.RateLimitPolicy, permitLimit: 60, window: TimeSpan.FromMinutes(1));

builder.Services.AddCors(options => options
    .AddPolicy("AllowFrontend",
        builder => builder
                    .WithOrigins(allowedCORSOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365 * 2);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

// Behind nginx every request otherwise appears to come from the proxy's container IP, which would
// collapse the per-IP rate-limit partition above into a single shared bucket.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UseHeaderPropagation();

// UseCors MUST precede UseHealthChecks (SVD-10 invariant). UseHealthChecks short-circuits the
// pipeline, so a health probe mapped ahead of CORS never gets its Access-Control-* headers — the
// portal's cross-origin fetch then fails and the /status tile renders a FALSE OUTAGE for a service
// that is perfectly healthy. Do not reorder these two lines.
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

app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGraphQL();

app.Run();
