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

using Common.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using Common.Application.Interfaces;
using Common.Infrastructure;
using Ardalis.GuardClauses;
using Common.Domain.Constants;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isGraphQLClient = true)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var validateSigningKey = configuration.GetValue<bool>("AuthorityServer:ValidateIssuerSigningKey");
                options.Authority = configuration.GetValue<string>("AuthorityServer:Authority");
                options.TokenValidationParameters.ValidateAudience = configuration.GetValue<bool>("AuthorityServer:ValidateAudience");
                options.TokenValidationParameters.ValidateIssuer = configuration.GetValue<bool>("AuthorityServer:ValidateIssuer");
                options.TokenValidationParameters.ValidateIssuerSigningKey = validateSigningKey;
                options.TokenValidationParameters.ValidIssuer = configuration.GetValue<string>("AuthorityServer:Authority");
                var validAudience = configuration.GetValue<string>("AuthorityServer:ValidAudience");
                if (!string.IsNullOrEmpty(validAudience))
                {
                    options.TokenValidationParameters.ValidAudiences = [validAudience];
                }
                if (validateSigningKey)
                {
                    X509Certificate2? certificate = null;
                    var loadCertFromFile = configuration.GetValue<bool>("OpenIddict:LoadCertFromFile");
                    if (loadCertFromFile)
                    {
                        var certificatePath = configuration.GetValue<string>("OpenIddict:Path");
                        var certificatePassword = configuration.GetValue<string>("OpenIddict:Password");

                        var bytes = File.ReadAllBytes(certificatePath ?? "");
                        certificate = X509CertificateLoader.LoadPkcs12(bytes, certificatePassword);
                    }
                    else
                    {
                        var thumbprint = configuration.GetValue<string>("OpenIddict:Thumbprint");
                        Guard.Against.Null(thumbprint, message: $"Thumbprint for OpenIddict not found");
                        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                        store.Open(OpenFlags.ReadOnly);
                        var certificates = store.Certificates.Find(
                            X509FindType.FindByThumbprint,
                            thumbprint,
                            false);
                        certificate = certificates.Count > 0 ? certificates[0] : null;
                    }
                    Guard.Against.Null(certificate, message: $"Certificate for OpenIddict not found");
                    var signingKey = new X509SecurityKey(certificate);
                    options.TokenValidationParameters.IssuerSigningKey = signingKey;
                }
            });

        services.AddSingleton(TimeProvider.System);

        if (isGraphQLClient)
        {
            // Identity client runs queries only — full resilience (incl. retry) is safe.
            services.AddGraphQLClient(Clients.Identity, resilience: GraphQLClientResilience.WithRetry);
            services.AddMemoryCache();
            services.AddSingleton<IGraphQLClientFactory, GraphQLClientFactory>();
            services.AddScoped<IIdentityService, IdentityService>();
        }

        return services;
    }
}
