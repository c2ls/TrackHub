// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using System.Globalization;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using OpenIddict.Server.AspNetCore;
using TrackHub.AuthorityServer.Infrastructure;
using TrackHub.AuthorityServer.Web;
using TrackHub.AuthorityServer.Web.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddTrackHubSerilog();

var allowedCORSOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string>();
Guard.Against.Null(allowedCORSOrigins, message: $"Allowed Origins configuration for CORS not loaded");

builder.Services.AddCors(options => options
    .AddPolicy("AllowFrontend",
        builder => builder
                    .WithOrigins(allowedCORSOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Configure supported cultures and localization options
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("es")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    FallBackToParentCultures = true,
    FallBackToParentUICultures = true
};

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                    options => options.LoginPath = "/login");

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddOpenIdDictDbContext(builder.Configuration);
builder.Services.AddOpenIdDictServices(builder.Configuration);

builder.Services.AddRazorPages();

// Add HealthChecks
builder.Services.AddHealthChecks()
            .AddDbContextCheck<SecurityDbContext>();

//Register Handlers
builder.Services.AddScoped<AuthorizationHandler>();
builder.Services.AddScoped<TokenHandler>();

// Configure HSTS
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365 * 2);
    options.IncludeSubDomains = true;
    options.Preload = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Add localization middleware
app.UseRequestLocalization(localizationOptions);

// Trust reverse proxy headers (nginx terminates SSL)
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UsePathBase("/Identity");

// CORS MUST precede UseHealthChecks: the latter is terminal middleware (it writes the response and
// never calls next), so registering it first would emit /Identity/health with no
// Access-Control-Allow-Origin and the portal's public status page could never read it — the Sign-in
// tile would report an outage while sign-in was healthy. Every other service already orders it this
// way; AuthorityServer was the outlier.
app.UseCors("AllowFrontend");

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(options => { });

app.MapGet("~/authorize", async (HttpContext context) =>
{
    var tokenHandler = context.RequestServices.GetRequiredService<AuthorizationHandler>();
    await tokenHandler.Authorize(context);
});

app.MapPost("~/token", async (HttpContext context) =>
{
    var tokenHandler = context.RequestServices.GetRequiredService<TokenHandler>();
    return await tokenHandler.Exchange(context);
});

app.MapPost("~/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.MapGet("~/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

    var postLogoutRedirectUri = context.Request.Query["post_logout_redirect_uri"].ToString();
    return Results.Redirect(postLogoutRedirectUri);
});

// POST /revoke is handled by OpenIddict's revocation endpoint (SetRevocationEndpointUris):
// it validates the body token + client_id per RFC 7009 and revokes it. The former custom
// handler shadowed that endpoint and never worked (it read a claim no anonymous call carries).

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
