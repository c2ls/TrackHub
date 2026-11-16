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

using System.Globalization;
using System.Net.Http.Headers;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Infrastructure.Tracksolid.Helpers;

namespace TrackHub.Router.Infrastructure.Tracksolid;

public class TracksolidReaderBase(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService,
    ICredentialWriter credentialWriter,
    IProviderSessionStore sessionStore)
{
    internal const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    private const int InvalidTokenCode = 1004;
    private const string TokenLifetimeSeconds = "7200";

    // jimi.oauth.token.get is limited to one call per minute, so token acquisition is serialised per credential.
    private const string TokenGate = "tracksolid-token";

    private CredentialTokenDto _credential;
    private string _endpoint = string.Empty;
    private string _accessToken = string.Empty;

    public ProtocolType Protocol => ProtocolType.Tracksolid;

    protected string Account => _credential.Username;

    public virtual async Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClientService.Init(httpClient, $"{ProtocolType.Tracksolid}");
        _credential = credential;
        _endpoint = $"{credential.Uri.TrimEnd('/')}/route/rest";
        _accessToken = await AcquireTokenAsync(null, cancellationToken);
    }

    private protected async Task<T?> CallAsync<T>(
        string method,
        IEnumerable<KeyValuePair<string, string>> parameters,
        CancellationToken cancellationToken)
    {
        var response = await SendAsync<T>(method, WithToken(parameters), cancellationToken);
        if (response.Code == InvalidTokenCode)
        {
            _accessToken = await AcquireTokenAsync(_accessToken, cancellationToken);
            response = await SendAsync<T>(method, WithToken(parameters), cancellationToken);
        }
        return EnsureSuccess(response, method);
    }

    private IEnumerable<KeyValuePair<string, string>> WithToken(IEnumerable<KeyValuePair<string, string>> parameters)
        => parameters.Append(new("access_token", _accessToken));

    private async Task<TracksolidResponse<T>> SendAsync<T>(
        string method,
        IEnumerable<KeyValuePair<string, string>> parameters,
        CancellationToken cancellationToken)
    {
        var form = new Dictionary<string, string>(parameters)
        {
            ["method"] = method,
            ["timestamp"] = DateTimeOffset.UtcNow.ToString(TimestampFormat, CultureInfo.InvariantCulture),
            ["app_key"] = _credential.Key ?? string.Empty,
            ["sign_method"] = "md5",
            ["v"] = "1.0",
            ["format"] = "json"
        };
        form["sign"] = RequestSigner.Sign(form, _credential.Key2 ?? string.Empty);

        using var content = new FormUrlEncodedContent(form);
        return await httpClientService.PostAsync<TracksolidResponse<T>>(_endpoint, content, cancellationToken)
            ?? throw new InvalidOperationException($"Tracksolid returned an empty response to '{method}'.");
    }

    private static T? EnsureSuccess<T>(TracksolidResponse<T> response, string method)
        => response.Code == 0
            ? response.Result
            : throw new InvalidOperationException($"Tracksolid API error {response.Code} calling '{method}': {response.Message}");

    private Task<string> AcquireTokenAsync(string? rejectedToken, CancellationToken cancellationToken)
        => sessionStore.AcquireAsync(TokenGate, _credential, rejectedToken, async (renewing, token) =>
        {
            if (!renewing && ProviderSessions.HasLiveStoredToken(_credential))
            {
                return ProviderSessions.ExpiringAt(_credential.Token!, _credential.TokenExpiration);
            }

            if (!renewing && _credential is { Token.Length: > 0, RefreshToken.Length: > 0 })
            {
                var refreshed = await SendAsync<TokenResult>("jimi.oauth.token.refresh",
                [
                    new("access_token", _credential.Token),
                    new("refresh_token", _credential.RefreshToken),
                    new("expires_in", TokenLifetimeSeconds)
                ], token);
                if (refreshed is { Code: 0, Result: { } refreshedToken })
                {
                    return await StoreTokenAsync(refreshedToken, "jimi.oauth.token.refresh", token);
                }
            }

            return await RequestTokenAsync(token);
        }, cancellationToken);

    private async Task<ProviderSessionGrant> RequestTokenAsync(CancellationToken cancellationToken)
    {
        const string method = "jimi.oauth.token.get";
        var response = await SendAsync<TokenResult>(method,
        [
            new("user_id", _credential.Username),
            new("user_pwd_md5", RequestSigner.Md5Lower(_credential.Password)),
            new("expires_in", TokenLifetimeSeconds)
        ], cancellationToken);
        return await StoreTokenAsync(
            EnsureSuccess(response, method) ?? throw new InvalidOperationException($"Tracksolid returned no token from '{method}'."),
            method,
            cancellationToken);
    }

    private async Task<ProviderSessionGrant> StoreTokenAsync(TokenResult token, string method, CancellationToken cancellationToken)
    {
        if (token is not { AccessToken.Length: > 0, ExpiresIn: > 0 })
        {
            throw new InvalidOperationException($"Tracksolid returned no access token from '{method}'.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        await credentialWriter.UpdateTokenAsync(_credential.CredentialId, new UpdateTokenDto(
            _credential.CredentialId, token.AccessToken, expiresAt, token.RefreshToken, null), cancellationToken);

        return ProviderSessions.ExpiringAt(token.AccessToken, expiresAt);
    }
}
