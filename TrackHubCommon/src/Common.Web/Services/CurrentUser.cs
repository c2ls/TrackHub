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

using System.Security.Claims;
using Common.Application.Interfaces;

namespace Common.Web.Services;

// Represents the current user in the web application.
public class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? Id => UserId?.ToString() ?? SubjectId;
    public string? SubjectId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
    public PrincipalType PrincipalType => ResolvePrincipalType();
    public Guid? UserId => TryGetGuidClaim("user_id") ?? (ResolvePrincipalType() == PrincipalType.User ? TryParseGuid(SubjectId) : null);
    public Guid? DriverId => TryGetGuidClaim("driver_id") ?? (ResolvePrincipalType() == PrincipalType.Driver ? TryParseGuid(SubjectId) : null);
    public string? Client => ClientId;
    public string? ClientId => Principal?.FindFirstValue("client_id");
    public Guid? PublicLinkGrantId => TryGetGuidClaim("public_link_grant_id");
    public string? Role => Principal?.FindFirstValue(ClaimTypes.Role);
    public Guid? AccountId => TryGetGuidClaim("account_id");
    public IReadOnlyCollection<string> Scopes => Principal?
        .FindAll("scope")
        .Concat(Principal.FindAll("scp"))
        .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];
    public IReadOnlyCollection<string> Audiences => Principal?
        .FindAll("aud")
        .Concat(Principal.FindAll("audience"))
        .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];
    public string? CorrelationId => httpContextAccessor.HttpContext?.TraceIdentifier;

    private PrincipalType ResolvePrincipalType()
    {
        var type = Principal?.FindFirstValue("principal_type");
        if (Enum.TryParse<PrincipalType>(type, ignoreCase: true, out var principalType))
        {
            return principalType;
        }

        if (string.Equals(Role, "service", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(ClientId))
        {
            return PrincipalType.ServiceClient;
        }

        if (Principal?.HasClaim(c => c.Type == "driver_id") == true)
        {
            return PrincipalType.Driver;
        }

        if (Principal?.HasClaim(c => c.Type == "public_link_grant_id") == true)
        {
            return PrincipalType.PublicLink;
        }

        return SubjectId == null ? PrincipalType.Unknown : PrincipalType.User;
    }

    private Guid? TryGetGuidClaim(string claimType) => TryParseGuid(Principal?.FindFirstValue(claimType));

    private static Guid? TryParseGuid(string? value)
        => Guid.TryParse(value, out var id) ? id : null;
}
