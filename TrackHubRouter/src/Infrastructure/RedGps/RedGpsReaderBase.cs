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

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Infrastructure.RedGps;

public class RedGpsReaderBase(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
{
    private const int OkStatus = 200;
    private const int InvalidTokenStatus = 30400;
    private const int NoDataStatus = 40100;
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(6);
    private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions DataOptions = new() { PropertyNameCaseInsensitive = true };

    // Every request costs a 30-second slot, so concurrent readers of one credential must share one gettoken.
    private const string TokenGate = "redgps-token";
    private const string RequestScope = "redgps-request";

    // RedGPS: "Entre cada petición debe existir una diferencia mínima de treinta segundos."
    internal static TimeSpan MinRequestInterval { get; set; } = TimeSpan.FromSeconds(30);

    private CredentialTokenDto _credential;
    private string _baseUrl = string.Empty;
    private string _token = string.Empty;

    public ProtocolType Protocol => ProtocolType.RedGps;

    public virtual async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClientService.Init(httpClient, $"{ProtocolType.RedGps}");
        _credential = credential;
        _baseUrl = credential.Uri.TrimEnd('/');
        _token = await AcquireTokenAsync(null, cancellationToken);
    }

    private protected async Task<List<T>> CallAsync<T>(
        string action,
        IEnumerable<KeyValuePair<string, string>> parameters,
        CancellationToken cancellationToken)
    {
        var response = await SendWithTokenAsync(action, parameters, cancellationToken);
        return response.Status switch
        {
            OkStatus => ReadList<T>(response.Data),
            NoDataStatus => [],
            _ => throw new InvalidOperationException($"RedGPS API error {response.Status} calling '{action}'.")
        };
    }

    private async Task<RedGpsResponse> SendWithTokenAsync(
        string action,
        IEnumerable<KeyValuePair<string, string>> parameters,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await SendAsync(action, WithToken(parameters), cancellationToken);
            if (response.Status != InvalidTokenStatus)
            {
                return response;
            }
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized)
        {
        }

        _token = await AcquireTokenAsync(_token, cancellationToken);
        return await SendAsync(action, WithToken(parameters), cancellationToken);
    }

    private IEnumerable<KeyValuePair<string, string>> WithToken(IEnumerable<KeyValuePair<string, string>> parameters)
        => parameters.Prepend(new("token", _token)).Prepend(new("apikey", _credential.Key ?? string.Empty));

    private Task<RedGpsResponse> SendAsync(
        string action,
        IEnumerable<KeyValuePair<string, string>> form,
        CancellationToken cancellationToken)
        => RequestPacing.RunAsync(RequestScope, _credential.CredentialId, MinRequestInterval, async () =>
        {
            using var content = new FormUrlEncodedContent(form);
            return await httpClientService.PostAsync<RedGpsResponse>($"{_baseUrl}/{action}", content, cancellationToken)
                ?? throw new InvalidOperationException($"RedGPS returned an empty response to '{action}'.");
        }, cancellationToken);

    private static List<T> ReadList<T>(JsonElement data)
        => data.ValueKind switch
        {
            JsonValueKind.Array => data.Deserialize<List<T>>(DataOptions) ?? [],
            JsonValueKind.Object => [data.Deserialize<T>(DataOptions)!],
            _ => []
        };

    private Task<string> AcquireTokenAsync(string? rejectedToken, CancellationToken cancellationToken)
        => sessionStore.AcquireReusingStoredTokenAsync(TokenGate, _credential, rejectedToken, RequestTokenAsync, cancellationToken, ExpiryMargin);

    private async Task<ProviderSessionGrant> RequestTokenAsync(CancellationToken cancellationToken)
    {
        var response = await SendAsync("gettoken",
        [
            new("apikey", _credential.Key ?? string.Empty),
            new("token", string.Empty),
            new("username", _credential.Username),
            new("password", _credential.Password)
        ], cancellationToken);

        var token = response is { Status: OkStatus, Data.ValueKind: JsonValueKind.String } ? response.Data.GetString() : null;
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException($"RedGPS authentication failed with status {response.Status}.");
        }

        var expiresAt = DateTimeOffset.UtcNow + TokenLifetime;
        await credentialWriter.UpdateTokenAsync(_credential.CredentialId,
            new UpdateTokenDto(_credential.CredentialId, token, expiresAt, null, null), cancellationToken);
        return ProviderSessions.ExpiringAt(token, expiresAt, ExpiryMargin);
    }
}
