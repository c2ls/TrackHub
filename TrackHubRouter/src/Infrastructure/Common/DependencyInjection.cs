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

using Common.Domain.Enums;
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Infrastructure.Common;
using TrackHub.Router.Infrastructure.Common.Geocoding;
using TrackHub.Router.Infrastructure.Common.Helpers;
using TrackHub.Router.Domain.Interfaces.Geocoding;
using TrackHub.Router.Application.Devices.Registry;
using TrackHub.Router.Application.PingOperator;
using TrackHub.Router.Application.DevicePositions.Registry;
using TrackHub.Router.Domain.Interfaces.Registry;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonContext(this IServiceCollection services, IConfiguration configuration)
    {
        var protocols = configuration.GetSection("AppSettings:Protocols").Get<IEnumerable<string>>();
        Guard.Against.Null(protocols, message: $"Client configuration for Protocols not loaded");

        // Uniform registration for every provider (no adapter special-casing — router-audit A-18);
        // each provider's readers are registered keyed by ProtocolType (router-audit A-07).
        foreach (var protocol in protocols)
        {
            services.RegisterProtocol(protocol);
        }

        services.AddHttpClient(NominatimReverseGeocoder.HttpClientName);

        // Provider credential clients: auto-redirect disabled so an operator-configured base URL
        // cannot be used to 302-redirect the Router to an internal endpoint (router-audit A-20).
        services.AddHttpClient(CredentialHttpClientFactory.ProviderHttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services.AddScoped<IReverseGeocoder, NominatimReverseGeocoder>();
        services.AddScoped<IReverseGeocodingService, ReverseGeocodingService>();

        services.AddSingleton<IExecutionIntervalManager, ExecutionIntervalManager>();
        services.AddSingleton<IOperatorSyncLock, OperatorSyncLock>();
        services.AddSingleton<IOperatorSyncBackoff, OperatorSyncBackoff>();
        services.AddSingleton<IDeviceCatalogCache, DeviceCatalogCache>();
        // Singleton: provider sessions (Wialon sid, Navixy hash, Geotab session id, bearer tokens)
        // outlive the transient readers so a login survives across sync/ping cycles.
        services.AddSingleton<IProviderSessionStore, ProviderSessionStore>();
        services.AddScoped<ICredentialHttpClientFactory, CredentialHttpClientFactory>();
        // Transient: each provider reader gets its OWN HttpClientService, so two concurrent
        // same-protocol operators in one sync scope never share the mutable Init state
        // (router-audit A-07/ARCH-12). Paired with keyed-transient readers below.
        services.AddTransient<IHttpClientService, HttpClientService>();
        // Scoped: the registries resolve keyed readers from the caller's live request scope.
        services.AddScoped<IConnectivityRegistry, ConnectivityRegistry>();
        services.AddScoped<IDeviceRegistry, DeviceRegistry>();
        services.AddScoped<IPositionRegistry, PositionRegistry>();
        return services;
    }
}
