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

using System.Text.Json;

namespace TrackHub.AuthorityServer.ClientSeeder;

internal class Clients
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Scope[] Scopes { get; set; } = [];
    public PKCEClient[] PKCEClients { get; set; } = [];
    public ServiceClient[] ServiceClients { get; set; } = [];

    public static Clients LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Clients>(json, JsonSerializerOptions) ?? new Clients();
    }

    /// <summary>
    /// Loads the base catalog plus every <c>clients.*.json</c> overlay next to it (sorted by
    /// file name). Overlays are additive; an overlay entry with the same scope name/client id
    /// replaces the base entry. Overlays are a local-development convenience only — the
    /// project excludes them from publish output so a deployed image never carries one; a
    /// production deployment registers extra identities in its mounted <c>clients.json</c>.
    /// </summary>
    public static Clients LoadWithOverlays(string filePath)
    {
        var clients = LoadFromFile(filePath);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath)) ?? ".";
        var baseName = Path.GetFileNameWithoutExtension(filePath);
        var overlays = Directory.EnumerateFiles(directory, $"{baseName}.*.json")
            .Where(path => !string.Equals(Path.GetFileName(path), Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

        foreach (var overlayPath in overlays)
        {
            var overlay = LoadFromFile(overlayPath);
            clients = new Clients
            {
                Scopes = MergeBy(clients.Scopes, overlay.Scopes, s => s.Name),
                PKCEClients = MergeBy(clients.PKCEClients, overlay.PKCEClients, c => c.ClientId),
                ServiceClients = MergeBy(clients.ServiceClients, overlay.ServiceClients, c => c.ClientId),
            };
        }

        return clients;
    }

    private static T[] MergeBy<T>(T[] baseline, T[] overlay, Func<T, string> key)
    {
        var overlayKeys = overlay.Select(key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return [.. baseline.Where(item => !overlayKeys.Contains(key(item))), .. overlay];
    }
}

internal class Scope
{
    public string Name { get; set; } = "";
    public string Resource { get; set; } = "";
}

internal class PKCEClient
{
    public string ClientId { get; set; } = "";
    public string Uri { get; set; } = "";
    public string PostLogoutUri { get; set; } = "";
    public string Scope { get; set; } = "";
}

internal class ServiceClient
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Scope { get; set; } = "";
}
