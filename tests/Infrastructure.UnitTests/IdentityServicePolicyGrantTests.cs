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

using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Infrastructure.Identity;

namespace Infrastructure.UnitTests;

// Pins POLICIES AS ADDITIVE GRANTS (role OR policy).
//
// The prior reading was the inverse — holding every attached policy was REQUIRED — which meant
// attaching a policy to a resource/action revoked it from everyone who did not hold that policy,
// Administrator's grant-all included. These tests fail if that inversion ever returns.
[TestFixture]
public class IdentityServicePolicyGrantTests
{
    private const string Resource = "Trips";
    private const string Action = "Delete";
    private static readonly Guid UserId = Guid.NewGuid();

    private static IdentityService Build(
        string[] rolesGrantingResourceAction,
        string[] usersRoles,
        string[] policiesOnResourceAction,
        string[] usersPolicies)
    {
        var resourceActionRoles = new Mock<IResourceActionRoleReader>();
        resourceActionRoles
            .Setup(x => x.GetResourceActionRolesAsync(Resource, Action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolesGrantingResourceAction);

        var userRoles = new Mock<IUserRoleReader>();
        userRoles
            .Setup(x => x.GetUserRoleNamesAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersRoles);

        var resourceActionPolicies = new Mock<IResourceActionPolicyReader>();
        resourceActionPolicies
            .Setup(x => x.GetResourceActionPoliciesAsync(Resource, Action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policiesOnResourceAction);

        var userPolicies = new Mock<IUserPolicyReader>();
        userPolicies
            .Setup(x => x.GetUserPolicyNamesAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersPolicies);

        return new IdentityService(
            new Mock<IUserReader>().Object,
            resourceActionRoles.Object,
            resourceActionPolicies.Object,
            userRoles.Object,
            userPolicies.Object,
            new Mock<IClientReader>().Object,
            new Mock<IServiceClientPermissionReader>().Object);
    }

    private static Task<bool> Authorize(IdentityService service)
        => service.AuthorizeUserAsync(UserId, Resource, Action, CancellationToken.None);

    // The everyday case: the role grants it, nothing else is attached.
    [Test]
    public async Task RoleGrant_WithNoPolicyAttached_Authorizes()
    {
        var service = Build(["Manager"], ["Manager"], [], []);

        Assert.That(await Authorize(service), Is.True);
    }

    // The point of the change: one user raised above their role WITHOUT widening the role.
    [Test]
    public async Task PolicyGrant_AloneAuthorizes_WhenTheRoleDoesNot()
    {
        var service = Build(["Manager"], ["User"], ["TripDeleters"], ["TripDeleters"]);

        Assert.That(await Authorize(service), Is.True);
    }

    // A policy the user does NOT hold grants them nothing.
    [Test]
    public async Task UnheldPolicy_DoesNotAuthorize_WhenTheRoleDoesNot()
    {
        var service = Build(["Manager"], ["User"], ["TripDeleters"], []);

        Assert.That(await Authorize(service), Is.False);
    }

    // The regression that matters: attaching a policy must not REVOKE an existing role grant.
    [Test]
    public async Task AttachingAPolicy_DoesNotRevokeAnExistingRoleGrant()
    {
        var service = Build(["Manager"], ["Manager"], ["TripDeleters"], []);

        Assert.That(await Authorize(service), Is.True);
    }

    // Holding one of several attached policies is enough — they are alternatives, not a checklist.
    [Test]
    public async Task HoldingOneOfSeveralAttachedPolicies_Authorizes()
    {
        var service = Build(["Manager"], ["User"], ["TripDeleters", "FleetAdmins"], ["FleetAdmins"]);

        Assert.That(await Authorize(service), Is.True);
    }

    // Neither path grants it.
    [Test]
    public async Task NeitherRoleNorPolicy_Denies()
    {
        var service = Build(["Manager"], ["User"], [], []);

        Assert.That(await Authorize(service), Is.False);
    }
}
