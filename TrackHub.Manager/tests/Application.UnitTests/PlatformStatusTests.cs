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

using TrackHub.Manager.Application.PlatformStatus.Commands;
using Common.Domain.Enums;
using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Application.UnitTests;

// Announcement command validation. The reader/writer visibility-window behaviour lives in
// Infrastructure.UnitTests/PlatformAnnouncementReaderTests (that layer owns the DbContext).
[TestFixture]
public class PlatformStatusTests
{
    private static CreatePlatformAnnouncementCommand Create(
        string messageEn = "m", string? messageEs = null,
        AnnouncementSeverity severity = AnnouncementSeverity.Info,
        DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null)
        => new(new PlatformAnnouncementDto(messageEn, messageEs, severity, startsAt, endsAt, true));

    [Test]
    public void Validator_RejectsEmptyEnglishMessage()
        => Assert.That(new CreatePlatformAnnouncementCommandValidator().Validate(Create(messageEn: string.Empty)).IsValid, Is.False);

    [Test]
    public void Validator_RejectsOverlongMessages()
    {
        var validator = new CreatePlatformAnnouncementCommandValidator();
        var tooLong = new string('x', PlatformAnnouncementContractRules.MessageMaxLength + 1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validator.Validate(Create(messageEn: tooLong)).IsValid, Is.False);
            Assert.That(validator.Validate(Create(messageEs: tooLong)).IsValid, Is.False);
        }
    }

    [Test]
    public void Validator_AllowsMessagesExactlyAtTheLimit()
    {
        var atLimit = new string('x', PlatformAnnouncementContractRules.MessageMaxLength);
        Assert.That(new CreatePlatformAnnouncementCommandValidator().Validate(Create(messageEn: atLimit)).IsValid, Is.True);
    }

    [Test]
    public void Validator_RejectsEndsAtBeforeOrEqualStartsAt()
    {
        var now = DateTimeOffset.UtcNow;
        var validator = new CreatePlatformAnnouncementCommandValidator();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validator.Validate(Create(startsAt: now, endsAt: now.AddHours(-1))).IsValid, Is.False);
            Assert.That(validator.Validate(Create(startsAt: now, endsAt: now)).IsValid, Is.False);
        }
    }

    [Test]
    public void Validator_AllowsOpenEndedWindows()
    {
        var validator = new CreatePlatformAnnouncementCommandValidator();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validator.Validate(Create()).IsValid, Is.True, "null/null = immediate until deactivated");
            Assert.That(validator.Validate(Create(startsAt: DateTimeOffset.UtcNow)).IsValid, Is.True, "scheduled, open-ended");
            Assert.That(validator.Validate(Create(endsAt: DateTimeOffset.UtcNow.AddHours(1))).IsValid, Is.True, "immediate, expiring");
        }
    }

    [Test]
    public void Validator_RejectsSeverityOutsideTheEnum()
        => Assert.That(new CreatePlatformAnnouncementCommandValidator().Validate(Create(severity: (AnnouncementSeverity)99)).IsValid, Is.False);

    [Test]
    public void UpdateValidator_AppliesTheSameContractRules()
    {
        var now = DateTimeOffset.UtcNow;
        var validator = new UpdatePlatformAnnouncementCommandValidator();
        var command = new UpdatePlatformAnnouncementCommand(Guid.NewGuid(),
            new PlatformAnnouncementDto(string.Empty, null, AnnouncementSeverity.Info, now, now.AddHours(-1), true));

        Assert.That(validator.Validate(command).IsValid, Is.False, "create and update must not drift");
    }
}
