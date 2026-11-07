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

using Common.Domain.Constants;

namespace DBInitializer;

// Deactivating a staff in Manager revokes the staff's Security credentials under manager_client
// (revokeStaffCredentials is a ServiceClient-only Staff/Custom command).
internal sealed class StaffOffboardingRbacSeedContribution : IRbacSeedContribution
{
    public IReadOnlyList<string> Resources { get; } = [];

    public IReadOnlyList<string> CustomActionResources { get; } = [Common.Domain.Constants.Resources.Staff];

    public IReadOnlyDictionary<string, (string Resource, string[] Actions)[]> RoleGrants { get; } =
        new Dictionary<string, (string Resource, string[] Actions)[]>();

    public IReadOnlyList<string> ServiceClientNames { get; } = ["manager_client"];

    public IReadOnlyList<(string[] Clients, (string Resource, string Action)[] Grants)> ServiceClientGrants { get; } =
    [
        (["manager_client"],
        [
            (Common.Domain.Constants.Resources.Staff, Actions.Custom),
        ]),
    ];
}
