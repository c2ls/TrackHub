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

using TrackHub.Security.Application.Users.Queries.GetAuthorizedActions;

namespace Application.UnitTests.Users;

[TestFixture]
internal class GetAuthorizedActionsQueryTests
{
    private readonly Mock<IUserRoleReader> _userRoleReaderMock = new();
    private readonly Mock<IUserPolicyReader> _userPolicyReaderMock = new();
    private readonly Mock<IResourceActionRoleReader> _resourceActionRoleReaderMock = new();
    private readonly Mock<IResourceActionPolicyReader> _resourceActionPolicyReaderMock = new();

    [Test]
    public async Task Handle_Should_Return_All_Authorized_Actions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<int> { 1, 2 };
        var userPolicies = new List<int> { 3, 4 };
        var roleAuthorizedActions = new List<ResourceActionVm> { new(1 ,"", 2, ""), new(3, "", 4, "") };
        var policyAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, ""), new(3, "", 4, "") };
        var allAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, ""), new(3, "", 4, "") };

        _userRoleReaderMock.Setup(x => x.GetUserRolesIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRoles);

        _userPolicyReaderMock.Setup(x => x.GetUserPolicyIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPolicies);

        _resourceActionRoleReaderMock.Setup(x => x.GetRoleAuthorizedActionsAsync(userRoles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleAuthorizedActions);

        _resourceActionPolicyReaderMock.Setup(x => x.GetPolicyAuthorizedActionsAsync(userPolicies, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyAuthorizedActions);

        var query = new GetAuthorizedActionsQuery(userId);
        var handler = new GetAuthorizedActionsQueryHandler(
            _userRoleReaderMock.Object,
            _userPolicyReaderMock.Object,
            _resourceActionRoleReaderMock.Object,
            _resourceActionPolicyReaderMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(allAuthorizedActions));
    }

    [Test]
    public async Task Handle_Should_Return_Empty_Actions_When_No_Roles_And_Policies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<int>();
        var userPolicies = new List<int>();
        var roleAuthorizedActions = new List<ResourceActionVm>();
        var policyAuthorizedActions = new List<ResourceActionVm>();
        var allAuthorizedActions = new List<ResourceActionVm>();

        _userRoleReaderMock.Setup(x => x.GetUserRolesIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRoles);

        _userPolicyReaderMock.Setup(x => x.GetUserPolicyIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPolicies);

        _resourceActionRoleReaderMock.Setup(x => x.GetRoleAuthorizedActionsAsync(userRoles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleAuthorizedActions);

        _resourceActionPolicyReaderMock.Setup(x => x.GetPolicyAuthorizedActionsAsync(userPolicies, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyAuthorizedActions);

        var query = new GetAuthorizedActionsQuery(userId);
        var handler = new GetAuthorizedActionsQueryHandler(
            _userRoleReaderMock.Object,
            _userPolicyReaderMock.Object,
            _resourceActionRoleReaderMock.Object,
            _resourceActionPolicyReaderMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(allAuthorizedActions));
    }

    [Test]
    public async Task Handle_Should_Return_Role_Authorized_Actions_When_No_Policies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<int> { 1, 2 };
        var userPolicies = new List<int>();
        var roleAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, "") };
        var allAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, "") };
        var policyAuthorizedActions = new List<ResourceActionVm>();

        _userRoleReaderMock.Setup(x => x.GetUserRolesIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRoles);

        _userPolicyReaderMock.Setup(x => x.GetUserPolicyIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPolicies);

        _resourceActionRoleReaderMock.Setup(x => x.GetRoleAuthorizedActionsAsync(userRoles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleAuthorizedActions);

        _resourceActionPolicyReaderMock.Setup(x => x.GetPolicyAuthorizedActionsAsync(userPolicies, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyAuthorizedActions);

        var query = new GetAuthorizedActionsQuery(userId);
        var handler = new GetAuthorizedActionsQueryHandler(
            _userRoleReaderMock.Object,
            _userPolicyReaderMock.Object,
            _resourceActionRoleReaderMock.Object,
            _resourceActionPolicyReaderMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(allAuthorizedActions));
    }

    [Test]
    public async Task Handle_Should_Return_Policy_Authorized_Actions_When_No_Roles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRoles = new List<int>();
        var userPolicies = new List<int> { 3, 4 };
        var roleAuthorizedActions = new List<ResourceActionVm>();
        var policyAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, "") };
        var allAuthorizedActions = new List<ResourceActionVm> { new(1, "", 2, "") };

        _userRoleReaderMock.Setup(x => x.GetUserRolesIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRoles);

        _userPolicyReaderMock.Setup(x => x.GetUserPolicyIdsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPolicies);

        _resourceActionRoleReaderMock.Setup(x => x.GetRoleAuthorizedActionsAsync(userRoles, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleAuthorizedActions);

        _resourceActionPolicyReaderMock.Setup(x => x.GetPolicyAuthorizedActionsAsync(userPolicies, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyAuthorizedActions);

        var query = new GetAuthorizedActionsQuery(userId);
        var handler = new GetAuthorizedActionsQueryHandler(
            _userRoleReaderMock.Object,
            _userPolicyReaderMock.Object,
            _resourceActionRoleReaderMock.Object,
            _resourceActionPolicyReaderMock.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(allAuthorizedActions));
    }
}
