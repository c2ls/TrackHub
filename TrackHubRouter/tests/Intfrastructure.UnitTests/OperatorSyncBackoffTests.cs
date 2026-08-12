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

using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.Tests;

// Guards the per-operator failure backoff (router-audit A-15).
[TestFixture]
public class OperatorSyncBackoffTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void NoFailures_IsNotInBackoff()
    {
        var sut = new OperatorSyncBackoff();
        Assert.That(sut.IsInBackoff(Guid.NewGuid(), Now), Is.False);
    }

    [Test]
    public void AfterFailure_IsInBackoffUntilWindowElapses()
    {
        var sut = new OperatorSyncBackoff();
        var id = Guid.NewGuid();

        sut.RecordFailure(id, Now);

        Assert.Multiple(() =>
        {
            // First failure → 1 minute window.
            Assert.That(sut.IsInBackoff(id, Now.AddSeconds(30)), Is.True);
            Assert.That(sut.IsInBackoff(id, Now.AddMinutes(1).AddSeconds(1)), Is.False);
        });
    }

    [Test]
    public void ConsecutiveFailures_GrowWindowExponentially()
    {
        var sut = new OperatorSyncBackoff();
        var id = Guid.NewGuid();

        sut.RecordFailure(id, Now); // 1 min
        sut.RecordFailure(id, Now); // 2 min
        sut.RecordFailure(id, Now); // 4 min

        Assert.Multiple(() =>
        {
            Assert.That(sut.IsInBackoff(id, Now.AddMinutes(3)), Is.True, "still within the 4-minute window");
            Assert.That(sut.IsInBackoff(id, Now.AddMinutes(4).AddSeconds(1)), Is.False);
        });
    }

    [Test]
    public void Success_ClearsBackoff()
    {
        var sut = new OperatorSyncBackoff();
        var id = Guid.NewGuid();

        sut.RecordFailure(id, Now);
        sut.RecordSuccess(id);

        Assert.That(sut.IsInBackoff(id, Now.AddSeconds(1)), Is.False);
    }

    [Test]
    public void BackoffWindow_IsCappedAtThirtyMinutes()
    {
        var sut = new OperatorSyncBackoff();
        var id = Guid.NewGuid();

        for (var i = 0; i < 20; i++)
        {
            sut.RecordFailure(id, Now);
        }

        Assert.Multiple(() =>
        {
            Assert.That(sut.IsInBackoff(id, Now.AddMinutes(29)), Is.True);
            Assert.That(sut.IsInBackoff(id, Now.AddMinutes(30).AddSeconds(1)), Is.False, "window is capped at 30 minutes");
        });
    }
}
