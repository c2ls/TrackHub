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
using Common.Mediator;
using HotChocolate.Execution;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure.SecurityApi;
using TrackHub.Security.Application.Users.Commands.CreateManager;
using TrackHub.ServiceContracts.Harness;
using TrackHub.ServiceContracts.Tests.Harness;
using SecurityUserVm = TrackHub.Security.Domain.Models.UserVm;

namespace TrackHub.ServiceContracts.Tests.RoundTripTests;

// Manager's REAL SecurityWriter provisions a manager account against
// Security's REAL resolvers — the identity-provisioning path on account/user creation.
[TestFixture]
public class ManagerToSecurityRoundTripTests
{
    private Mock<ISender> _sender = null!;
    private InProcessGraphQLClientFactory _factory = null!;

    [OneTimeSetUp]
    public async Task BuildSecurityExecutor()
    {
        _sender = new Mock<ISender>();
        var executor = await ProducerSchema.BuildSecurityExecutorAsync(_sender.Object);
        _factory = new InProcessGraphQLClientFactory(
            new Dictionary<string, IRequestExecutor> { [Clients.Security] = executor });
    }

    [SetUp]
    public void ResetSender() => _sender.Reset();

    [Test]
    public async Task CreateManager_RoundTripsUserProvisioningIntoManagerUserVm()
    {
        CreateManagerCommand? received = null;
        _sender
            .Setup(s => s.Send(It.IsAny<CreateManagerCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<SecurityUserVm>, CancellationToken>((cmd, _) => received = (CreateManagerCommand)cmd)
            .ReturnsAsync(default(SecurityUserVm) with
            {
                UserId = FakeData.OperatorId,
                Username = "new-manager",
                AccountId = FakeData.AccountId,
                Active = true,
            });

        var writer = new SecurityWriter(_factory);
        var created = await writer.CreateUserAsync(new CreateUserDto(
            AccountId: FakeData.AccountId,
            Username: "new-manager",
            Password: "S3cret!pass",
            EmailAddress: "manager@example.com",
            FirstName: "Ada",
            LastName: "Lovelace",
            Active: true), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.UserId, Is.EqualTo(FakeData.OperatorId));
            Assert.That(created.Username, Is.EqualTo("new-manager"));
            Assert.That(created.AccountId, Is.EqualTo(FakeData.AccountId));
            Assert.That(created.Active, Is.True);
        }

        Assert.That(received, Is.Not.Null, "the real CreateManagerCommand must reach the Security handler");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(received!.Value.AccountId, Is.EqualTo(FakeData.AccountId));
            Assert.That(received!.Value.User.Username, Is.EqualTo("new-manager"));
            Assert.That(received!.Value.User.Password, Is.EqualTo("S3cret!pass"));
            Assert.That(received!.Value.User.EmailAddress, Is.EqualTo("manager@example.com"));
            Assert.That(received!.Value.User.FirstName, Is.EqualTo("Ada"));
            Assert.That(received!.Value.User.LastName, Is.EqualTo("Lovelace"));
        }
    }
}
