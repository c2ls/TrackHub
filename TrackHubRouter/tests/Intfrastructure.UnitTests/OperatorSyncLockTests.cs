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

// Guards router-audit A-25: the manual-sync and background-sync paths must not run concurrently
// for the same operator.
[TestFixture]
public class OperatorSyncLockTests
{
    [Test]
    public async Task AcquireAsync_SameOperator_SerializesAccess()
    {
        var sut = new OperatorSyncLock();
        var operatorId = Guid.NewGuid();

        var first = await sut.AcquireAsync(operatorId, CancellationToken.None);

        var second = sut.AcquireAsync(operatorId, CancellationToken.None);
        Assert.That(second.IsCompleted, Is.False, "second acquire must block while the first holds the gate");

        first.Dispose();
        var secondHandle = await second;
        Assert.That(secondHandle, Is.Not.Null);
        secondHandle.Dispose();
    }

    [Test]
    public async Task AcquireAsync_DifferentOperators_DoNotBlock()
    {
        var sut = new OperatorSyncLock();

        var a = await sut.AcquireAsync(Guid.NewGuid(), CancellationToken.None);
        var b = await sut.AcquireAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.Not.Null);
            Assert.That(b, Is.Not.Null);
        });

        a.Dispose();
        b.Dispose();
    }
}
