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

using System.Net.Http.Headers;
using System.Text.Json;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Infrastructure.Webfleet.Helpers;

namespace TrackHub.Router.Infrastructure.Webfleet;

public class WebfleetReaderBase(
    ICredentialHttpClientFactory httpClientFactory,
    IHttpClientService httpClientService)
{
    // Error 8015: requests of one account/user pair must be serialised and may not overlap.
    private const string RequestGate = "webfleet-request";

    private Guid _credentialId;
    private string _requestPrefix = string.Empty;

    public ProtocolType Protocol => ProtocolType.Webfleet;

    public virtual Task Init(CredentialTokenDto credential, CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClientAsync(credential, cancellationToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // Credentials in the URL are rejected (error 1180); only Basic auth is accepted.
        httpClient.DefaultRequestHeaders.Authorization = BasicAuthentication.Header(credential.Username, credential.Password);
        httpClientService.Init(httpClient, $"{ProtocolType.Webfleet}");

        _credentialId = credential.CredentialId;
        var account = credential.Key2 ?? throw new InvalidOperationException("Webfleet credential has no account name (Key2).");
        var apiKey = credential.Key ?? throw new InvalidOperationException("Webfleet credential has no API key (Key).");
        // useUTF8: Uri.EscapeDataString percent-encodes UTF-8, Webfleet defaults to ISO-8859-1.
        _requestPrefix = $"{credential.Uri.TrimEnd('/')}?account={Uri.EscapeDataString(account)}&apikey={Uri.EscapeDataString(apiKey)}"
            + "&lang=en&outputformat=json&useISO8601=true&useUTF8=true";
        return Task.CompletedTask;
    }

    private protected async Task<IReadOnlyList<T>> GetListAsync<T>(
        string action, string parameters, CancellationToken cancellationToken)
        => WebfleetResponse.ReadList<T>(await GetAsync(action, parameters, cancellationToken));

    private protected Task<JsonElement> GetAsync(string action, string parameters, CancellationToken cancellationToken)
        => CredentialGate.RunAsync(RequestGate, _credentialId, async () =>
        {
            var body = await httpClientService.GetAsync<JsonElement>(
                $"{_requestPrefix}&action={action}{parameters}", cancellationToken: cancellationToken);
            WebfleetResponse.EnsureSuccess(body, action);
            return body;
        }, cancellationToken);
}
