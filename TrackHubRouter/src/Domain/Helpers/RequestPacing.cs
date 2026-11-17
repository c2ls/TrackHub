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

using System.Collections.Concurrent;

namespace TrackHub.Router.Domain.Helpers;

public static class RequestPacing
{
    private static readonly ConcurrentDictionary<(string Scope, Guid CredentialId), DateTimeOffset> LastRequestEnd = new();

    public static Task<T> RunAsync<T>(string scope, Guid credentialId, TimeSpan minInterval, Func<Task<T>> request, CancellationToken cancellationToken)
        => CredentialGate.RunAsync(scope, credentialId, async () =>
        {
            var wait = LastRequestEnd.GetValueOrDefault((scope, credentialId)) + minInterval - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, cancellationToken);
            }

            try
            {
                return await request();
            }
            finally
            {
                LastRequestEnd[(scope, credentialId)] = DateTimeOffset.UtcNow;
            }
        }, cancellationToken);
}
