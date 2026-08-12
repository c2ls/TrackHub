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

using Common.Application.Attributes;
using Common.Domain.Constants;
using TrackHub.Manager.Application.PublicLinks.Commands;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.PublicLinks;

/// <summary>
/// Handler/validator/authorization contract for the shared public-link resolution command
/// (spec 11 §7.8/§18.10). The resolution semantics themselves — access counting and the single
/// <c>PublicLinkAccessed</c> audit row — are pinned in <c>Infrastructure.UnitTests</c>, where a real
/// DbContext can observe them.
/// </summary>
[TestFixture]
public class ResolvePublicLinkGrantTests
{
    private static ResolvePublicLinkGrantCommand Command(
        Guid? grantId = null, Guid? accountId = null, string resourceType = "Trip",
        string resourceId = "a-trip-id", string scope = "trip.track", string token = "a-token")
        => new(grantId ?? Guid.NewGuid(), accountId ?? Guid.NewGuid(), resourceType, resourceId, scope, token);

    private static PublicLinkGrantVm SomeGrant(Guid grantId, Guid accountId)
        => new(grantId, accountId, "Trip", "a-trip-id", "trip.track", "customer tracking",
            DateTimeOffset.UtcNow.AddDays(1), null, null, "creator", 1, DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, null);

    private static Mock<IPublicLinkGrantResolver> ResolverReturning(
        ResolvePublicLinkGrantCommand command, PublicLinkGrantResolutionResult result)
    {
        var resolver = new Mock<IPublicLinkGrantResolver>();
        resolver
            .Setup(x => x.ResolvePublicLinkGrantAsync(
                command.PublicLinkGrantId, command.AccountId, command.ResourceType,
                command.ResourceId, command.Scope, command.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return resolver;
    }

    [Test]
    public async Task Handle_Found_EchoesGrantAndResourceIdentifiers()
    {
        var command = Command();
        var resolver = ResolverReturning(command, new PublicLinkGrantResolutionResult(
            PublicLinkResolution.Found, SomeGrant(command.PublicLinkGrantId, command.AccountId)));

        var result = await new ResolvePublicLinkGrantCommandHandler(resolver.Object)
            .Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(PublicLinkResolution.Found));
            Assert.That(result.PublicLinkGrantId, Is.EqualTo(command.PublicLinkGrantId));
            Assert.That(result.ResourceId, Is.EqualTo("a-trip-id"));
        });
        resolver.VerifyAll();
    }

    // Rejections are normal answers, not exceptions — 404 and 410 must survive the handler, and must
    // disclose no identifiers.
    [TestCase(PublicLinkResolution.NotFound)]
    [TestCase(PublicLinkResolution.Expired)]
    public async Task Handle_Rejected_ReturnsOutcomeWithNoIdentifiers(PublicLinkResolution resolution)
    {
        var command = Command();
        var resolver = ResolverReturning(command, new PublicLinkGrantResolutionResult(resolution, null));

        var result = await new ResolvePublicLinkGrantCommandHandler(resolver.Object)
            .Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(resolution));
            Assert.That(result.PublicLinkGrantId, Is.Null);
            Assert.That(result.ResourceId, Is.Null);
        });
        resolver.VerifyAll();
    }

    // The consumer maps these literals by name and defaults anything unrecognized to NotFound.
    // A fourth literal is a coordinated contract change, not a local edit.
    [Test]
    public void Resolution_HasExactlyTheThreeAgreedLiterals()
        => Assert.That(
            Enum.GetNames<PublicLinkResolution>(),
            Is.EquivalentTo(new[] { "Found", "NotFound", "Expired" }));

    [Test]
    public void Command_IsServiceClientOnly()
    {
        var attribute = typeof(ResolvePublicLinkGrantCommand)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(attribute.PrincipalTypes, Is.EqualTo("ServiceClient"), "only downstream services may exchange a raw token here");
            Assert.That(attribute.Resource, Is.EqualTo(Resources.PublicLinks));
            Assert.That(attribute.Action, Is.EqualTo(Actions.Read));
        });
    }

    [Test]
    public void Validator_AcceptsAWellFormedCommand()
    {
        var result = new ResolvePublicLinkGrantCommandValidator().Validate(Command());
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validator_RejectsEmptyIdentifiersAndBlankStrings()
    {
        var validator = new ResolvePublicLinkGrantCommandValidator();
        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(Command(grantId: Guid.Empty)).IsValid, Is.False);
            Assert.That(validator.Validate(Command(accountId: Guid.Empty)).IsValid, Is.False);
            Assert.That(validator.Validate(Command(resourceType: " ")).IsValid, Is.False);
            Assert.That(validator.Validate(Command(resourceId: " ")).IsValid, Is.False);
            Assert.That(validator.Validate(Command(scope: " ")).IsValid, Is.False);
            Assert.That(validator.Validate(Command(token: " ")).IsValid, Is.False);
        });
    }
}
