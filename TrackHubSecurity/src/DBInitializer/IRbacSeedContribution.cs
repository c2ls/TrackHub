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

namespace DBInitializer;

/// <summary>
/// One module's slice of the RBAC seed data. The initializer discovers every implementation
/// in this assembly and seeds the aggregate, so a feature module contributes its resources,
/// role grants and service-client allowlists from its own file instead of growing a central
/// list. All seeding stays additive and idempotent; the Administrator role automatically
/// receives grant-all over every contributed resource/action pair.
/// Implementations must have a public parameterless constructor.
/// </summary>
internal interface IRbacSeedContribution
{
    /// <summary>Resource catalog entries; each is paired with every standard action.</summary>
    IReadOnlyList<string> Resources { get; }

    /// <summary>Resources additionally paired with <c>Actions.Custom</c> (making the pair grantable).</summary>
    IReadOnlyList<string> CustomActionResources { get; }

    /// <summary>Role name → the full set of (resource, actions) grants for that role.</summary>
    IReadOnlyDictionary<string, (string Resource, string[] Actions)[]> RoleGrants { get; }

    /// <summary>
    /// Service-client NAME registrations (the app-level allowlist consulted by
    /// IsValidServiceAsync; credentials live in the AuthorityServer's store).
    /// </summary>
    IReadOnlyList<string> ServiceClientNames { get; }

    /// <summary>
    /// Service-client permission allowlists: each entry grants a set of (resource, action)
    /// pairs to a set of client identities.
    /// </summary>
    IReadOnlyList<(string[] Clients, (string Resource, string Action)[] Grants)> ServiceClientGrants { get; }
}
