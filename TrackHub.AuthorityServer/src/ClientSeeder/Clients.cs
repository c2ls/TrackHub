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
