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
using System.Text;

namespace TrackHub.Router.Infrastructure.Tests;

internal sealed partial class RecordingHttpHandler : HttpMessageHandler
{
    private readonly Lock _gate = new();
    private readonly Queue<HttpResponseMessage> _responses = new();
    private readonly List<(HttpRequestMessage Request, string Body)> _requests = [];
    private Func<HttpRequestMessage, string, string?>? _route;

    public IReadOnlyList<(HttpRequestMessage Request, string Body)> Requests
    {
        get
        {
            lock (_gate)
            {
                return [.. _requests];
            }
        }
    }

    public void EnqueueJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
        => Enqueue(Response(json, "application/json", statusCode));

    public void Route(Func<HttpRequestMessage, string, string?> route) => _route = route;

    public HttpClient CreateClient(string uri) => new(this) { BaseAddress = new Uri(uri) };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        lock (_gate)
        {
            _requests.Add((request, body));

            if (_responses.Count > 0)
            {
                return _responses.Dequeue();
            }

            if (_route is null)
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };
            }

            return _route(request, body) is { } payload
                ? Response(payload, payload.StartsWith('<') ? "text/xml" : "application/json", HttpStatusCode.OK)
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }

    private void Enqueue(HttpResponseMessage response)
    {
        lock (_gate)
        {
            _responses.Enqueue(response);
        }
    }

    private static HttpResponseMessage Response(string body, string mediaType, HttpStatusCode statusCode)
        => new(statusCode) { Content = new StringContent(body, Encoding.UTF8, mediaType) };
}
