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

using System.Reflection;
using Ardalis.GuardClauses;
using Common.Application;
using Common.Web.Transformers;
using Scalar.AspNetCore;
using TrackHub.Router.Web.GraphQL;
using TrackHub.Router.Web.GraphQL.Mutation;
using TrackHub.Router.Web.GraphQL.Query;

var builder = WebApplication.CreateBuilder(args);

builder.AddTrackHubSerilog();

var allowedCORSOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string>();
Guard.Against.Null(allowedCORSOrigins, message: $"Allowed Origins configuration for CORS not loaded");

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddAppManagerContext();
builder.Services.AddAppTelemetryContext();
builder.Services.AddGeofenceManagerContext();
builder.Services.AddAppTripManagementContext();
builder.Services.AddCommonContext(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddHealthChecks();

// Shared TrackHub GraphQL hardening + the Router-specific error filters.
builder.Services.AddTrackHubGraphQLServer<Query, Mutation>(builder.Environment.IsDevelopment())
    .AddErrorFilter<GeocodingErrorFilter>()
    .AddErrorFilter<OperatorSyncErrorFilter>()
    .AddErrorFilter<ProviderCapabilityErrorFilter>();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

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

//Add Scalar API
app.MapOpenApi();
app.MapScalarApiReference();

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
app.MapEndpoints(Assembly.GetExecutingAssembly());

app.MapGraphQL();

app.Run();
