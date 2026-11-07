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
using Microsoft.Extensions.Logging.Abstractions;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Services;

namespace Infrastructure.UnitTests;

// An e-mail subscription delivers a transporter event only to a subscriber who can see that
// transporter: privileged role from the replica, group membership, or (staff) their own vehicle.
[TestFixture]
public class AlertSubscriptionVisibilityTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static AlertEventVm Event(Guid accountId, Guid transporterId)
        => new(Guid.NewGuid(), accountId, "TripStarted", "Info", "Trips", "Transporter", transporterId.ToString(),
            "Open", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, $"test:{Guid.NewGuid():N}", DateTimeOffset.UtcNow);

    private static async Task<(Guid AccountId, Transporter Transporter)> SeedAccountAsync(ApplicationDbContext context)
    {
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("ABC123", 1, accountId);
        await context.Transporters.AddAsync(transporter);
        await context.AccountFeatures.AddRangeAsync(
            new AccountFeature(accountId, FeatureKeys.Notifications, true, "standard", "manual", null, null, null),
            new AccountFeature(accountId, FeatureKeys.NotificationsEmail, true, "standard", "manual", null, null, null));
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "trip-started", "Notifications", true, "TripStarted",
            """{"subscribers":true}""", """["Email"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);
        return (accountId, transporter);
    }

    private static async Task SubscribeAsync(ApplicationDbContext context, Guid accountId, string principalType, Guid principalId, string email)
    {
        await context.AlertSubscriptions.AddAsync(new AlertSubscription(accountId, principalType, principalId, "TripStarted", NotificationChannels.Email, email, true));
        await context.SaveChangesAsync(CancellationToken.None);
    }

    private static IEnumerable<string> EmailedTo(ApplicationDbContext context)
        => context.NotificationDeliveries.Where(d => d.Channel == NotificationChannels.Email).Select(d => d.Recipient).ToList();

    [Test]
    public async Task UserSubscriber_WithoutVisibility_GetsNothing_WithReplicatedManagerRole_GetsIt()
    {
        await using var context = NewContext(nameof(UserSubscriber_WithoutVisibility_GetsNothing_WithReplicatedManagerRole_GetsIt));
        var (accountId, transporter) = await SeedAccountAsync(context);
        var customer = Guid.NewGuid();
        var manager = Guid.NewGuid();
        await context.Users.AddRangeAsync(
            new User(customer, "customer", true, accountId) { Role = Roles.User },
            new User(manager, "manager", true, accountId) { Role = Roles.Manager });
        await context.SaveChangesAsync(CancellationToken.None);
        await SubscribeAsync(context, accountId, RecipientPrincipalTypes.User, customer, "customer@example.com");
        await SubscribeAsync(context, accountId, RecipientPrincipalTypes.User, manager, "manager@example.com");

        await new AlertRuleEvaluator(context as IApplicationDbContext, NullLogger<AlertRuleEvaluator>.Instance)
            .EvaluateAsync(Event(accountId, transporter.TransporterId), CancellationToken.None);

        Assert.That(EmailedTo(context), Is.EquivalentTo(new[] { "manager@example.com" }));
    }

    [Test]
    public async Task UserSubscriber_InAnActiveGroupWithTheTransporter_GetsIt()
    {
        await using var context = NewContext(nameof(UserSubscriber_InAnActiveGroupWithTheTransporter_GetsIt));
        var (accountId, transporter) = await SeedAccountAsync(context);
        var userId = Guid.NewGuid();
        var group = new Group("ops", "", true, accountId);
        group.Transporters.Add(transporter);
        await context.Users.AddAsync(new User(userId, "dispatcher", true, accountId));
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync(CancellationToken.None);
        await context.UsersGroup.AddAsync(new UserGroup { UserId = userId, GroupId = group.GroupId });
        await context.SaveChangesAsync(CancellationToken.None);
        await SubscribeAsync(context, accountId, RecipientPrincipalTypes.User, userId, "dispatcher@example.com");

        await new AlertRuleEvaluator(context as IApplicationDbContext, NullLogger<AlertRuleEvaluator>.Instance)
            .EvaluateAsync(Event(accountId, transporter.TransporterId), CancellationToken.None);

        Assert.That(EmailedTo(context), Is.EquivalentTo(new[] { "dispatcher@example.com" }));
    }

    [Test]
    public async Task StaffSubscriber_SeesOnlyTheirOwnVehicle()
    {
        await using var context = NewContext(nameof(StaffSubscriber_SeesOnlyTheirOwnVehicle));
        var (accountId, transporter) = await SeedAccountAsync(context);
        var owner = new StaffMember(accountId, "Owner", null, null, null, true, null, null, null, transporter.TransporterId);
        var other = new StaffMember(accountId, "Other", null, null, null, true, null, null, null, null);
        await context.StaffMembers.AddRangeAsync(owner, other);
        await context.SaveChangesAsync(CancellationToken.None);
        await SubscribeAsync(context, accountId, RecipientPrincipalTypes.Staff, owner.StaffId, "owner@example.com");
        await SubscribeAsync(context, accountId, RecipientPrincipalTypes.Staff, other.StaffId, "other@example.com");

        await new AlertRuleEvaluator(context as IApplicationDbContext, NullLogger<AlertRuleEvaluator>.Instance)
            .EvaluateAsync(Event(accountId, transporter.TransporterId), CancellationToken.None);

        Assert.That(EmailedTo(context), Is.EquivalentTo(new[] { "owner@example.com" }));
    }
}
