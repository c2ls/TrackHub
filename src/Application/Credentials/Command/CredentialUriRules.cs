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

using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace TrackHub.Manager.Application.Credentials.Command;

// A credential Uri is dialled by the Router with the platform's own network reach, so an
// unvalidated value is a server-side request forgery vector: file://, an internal host or a
// cloud metadata endpoint would all be fetched. Every credential write shares these rules.
public static class CredentialUriRules
{
    // Development environments legitimately point operators at loopback or a LAN address.
    // Absent (the deployed default) the non-routable ranges are rejected.
    public const string AllowPrivateHostsKey = "AppSettings:AllowPrivateCredentialHosts";

    private static readonly string[] MetadataHosts =
    [
        "metadata.google.internal",
        "metadata.goog",
        "instance-data"
    ];

    public static void Apply<T>(AbstractValidator<T> validator, Expression<Func<T, string>> selector, IConfiguration configuration)
    {
        var allowPrivateHosts = bool.TryParse(configuration[AllowPrivateHostsKey], out var configured) && configured;

        validator.RuleFor(selector)
            .NotEmpty()
            .Must(BeAnAbsoluteHttpUri)
            .WithMessage("Credential Uri must be an absolute http or https URL.")
            .Must(uri => allowPrivateHosts || !TargetsNonRoutableHost(uri))
            .WithMessage("Credential Uri must not target a loopback, link-local, private or metadata-service host.");
    }

    private static bool BeAnAbsoluteHttpUri(string uri)
        => string.IsNullOrWhiteSpace(uri)
            || (Uri.TryCreate(uri, UriKind.Absolute, out var parsed)
                && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps));

    private static bool TargetsNonRoutableHost(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        var host = parsed.DnsSafeHost;
        if (IPAddress.TryParse(host, out var address))
        {
            return IsNonRoutable(address);
        }

        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            || MetadataHosts.Contains(host, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsNonRoutable(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // fe80::/10 link-local and fc00::/7 unique-local.
            return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || (address.GetAddressBytes()[0] & 0xFE) == 0xFC;
        }

        var octets = address.GetAddressBytes();
        return octets[0] switch
        {
            10 => true,
            127 => true,
            // 100.64.0.0/10 carrier-grade NAT.
            100 => octets[1] >= 64 && octets[1] <= 127,
            // 169.254.0.0/16 link-local, which covers the 169.254.169.254 metadata service.
            169 => octets[1] == 254,
            172 => octets[1] >= 16 && octets[1] <= 31,
            192 => octets[1] == 168,
            _ => false
        };
    }
}
