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

[TestFixture]
public class ExecutionIntervalManagerTests
{
    [Test]
    public void TryBeginOperatorRun_InsideTheProviderInterval_IsRefused()
    {
        var manager = new ExecutionIntervalManager();
        var operatorId = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.That(manager.TryBeginOperatorRun(operatorId, TimeSpan.FromMinutes(2)), Is.True);
            Assert.That(manager.TryBeginOperatorRun(operatorId, TimeSpan.FromMinutes(2)), Is.False);
            Assert.That(manager.TryBeginOperatorRun(Guid.NewGuid(), TimeSpan.FromMinutes(2)), Is.True);
        });
    }

    [Test]
    public void TryBeginOperatorRun_WithoutAProviderInterval_AlwaysRuns()
    {
        var manager = new ExecutionIntervalManager();
        var operatorId = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.That(manager.TryBeginOperatorRun(operatorId, TimeSpan.Zero), Is.True);
            Assert.That(manager.TryBeginOperatorRun(operatorId, TimeSpan.Zero), Is.True);
        });
    }
}
