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

using System.Security.Cryptography;
using System.Text;

namespace TrackHub.Router.Infrastructure.Tracksolid.Helpers;

internal static class RequestSigner
{
    // Documented algorithm: UPPER(md5(secret + key1value1key2value2... + secret)), keys sorted ordinally,
    // `sign` itself and empty values excluded.
    public static string Sign(IEnumerable<KeyValuePair<string, string>> parameters, string appSecret)
    {
        var builder = new StringBuilder(appSecret);
        foreach (var (key, value) in parameters
            .Where(p => p.Key != "sign" && !string.IsNullOrEmpty(p.Value))
            .OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            builder.Append(key).Append(value);
        }
        builder.Append(appSecret);
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    public static string Md5Lower(string input)
        => Convert.ToHexStringLower(MD5.HashData(Encoding.UTF8.GetBytes(input)));
}
