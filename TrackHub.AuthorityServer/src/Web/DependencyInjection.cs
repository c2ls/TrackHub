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

using System.Security.Cryptography.X509Certificates;
using Ardalis.GuardClauses;
using Quartz;
using TrackHub.AuthorityServer.Infrastructure;

namespace TrackHub.AuthorityServer.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenIdDictServices(this IServiceCollection services, IConfiguration configuration)
    {
        var scopes = configuration["OpenIddict:Scopes"];
        Guard.Against.Null(scopes, message: $"Scopes for OpenIddict not loaded");

        services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        services.AddOpenIddict()
        .AddCore(options => {
            options.UseEntityFrameworkCore()
                .UseDbContext<AuthorityDbContext>()
                .ReplaceDefaultEntities<long>();
            options.UseQuartz();
        })
        .AddServer(
            _ =>
            {
                _.AllowClientCredentialsFlow();
                _.AllowRefreshTokenFlow();
                _.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

                _.SetAuthorizationEndpointUris("authorize");
                _.SetTokenEndpointUris("token");
                _.SetRevocationEndpointUris("revoke");
                _.SetEndSessionEndpointUris("logout");
                _.RegisterScopes(scopes.Split(','));

                // Make token lifetimes explicit (OpenIddict defaults, pinned as configuration intent).
                _.SetAccessTokenLifetime(TimeSpan.FromHours(1));
                _.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

#if DEBUG
                    _.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
#else
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

                _.AddSigningCertificate(certificate)
                        .AddEncryptionCertificate(certificate);
                    //TODO: Separate certificates for signing and encryption
#endif

                //disable access token payload encryption
                _.DisableAccessTokenEncryption();
                _.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .DisableTransportSecurityRequirement();
            }
        );

        return services;
    }
}
