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

using Common.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.SyncWorker;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class OperatorFanOutTests
{
    [TestCase(OperatorRunOutcome.Succeeded, 1, 0)]
    [TestCase(OperatorRunOutcome.Failed, 0, 1)]
    [TestCase(OperatorRunOutcome.Skipped, 0, 0)]
    public async Task RunAsync_RecordsBackoffOnlyForARealOutcome(OperatorRunOutcome outcome, int successes, int failures)
    {
        var accountId = Guid.NewGuid();
        var @operator = new OperatorVm(Guid.NewGuid(), (int)ProtocolType.Traccar, accountId, new CredentialTokenVm());
        var accounts = new Mock<IAccountReader>();
        accounts.Setup(x => x.GetAccountsToSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new AccountSettingsVm(accountId, 60, false, false, GpsIntegrationEnabled: true)]);
        var operators = new Mock<IOperatorReader>();
        operators.Setup(x => x.GetOperatorsByAccountsAsync(accountId, It.IsAny<CancellationToken>())).ReturnsAsync([@operator]);
        var backoff = new Mock<IOperatorSyncBackoff>();
        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<ISender>())
            .AddSingleton(backoff.Object)
            .BuildServiceProvider();
        var fanOut = new OperatorFanOut(services.GetRequiredService<IServiceScopeFactory>(), accounts.Object, operators.Object,
            new ConfigurationBuilder().Build(), NullLogger<OperatorFanOut>.Instance);

        await fanOut.RunAsync("test", DateTimeOffset.UtcNow, (_, _) => true, (_, _, _) => Task.FromResult(outcome), CancellationToken.None);

        backoff.Verify(x => x.RecordSuccessAsync(@operator, It.IsAny<CancellationToken>()), Times.Exactly(successes));
        backoff.Verify(x => x.RecordFailureAsync(@operator, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Exactly(failures));
    }
}
