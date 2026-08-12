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

using TrackHub.Manager.Application.Accounts.Queries.Get;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests;

[TestFixture]
public class GetAccountQueryHandlerTests
{
    private Mock<IAccountReader> _mockReader;
    private GetAccountQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockReader = new Mock<IAccountReader>();
        _handler = new GetAccountQueryHandler(_mockReader.Object);
    }

    [Test]
    public async Task Handle_WhenAccountExists_ReturnsAccountVm()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var accountVm = new AccountVm { AccountId = accountId, Name = "Customer" };
        _mockReader.Setup(reader => reader.GetAccountAsync(accountId, CancellationToken.None))
            .ReturnsAsync(accountVm);

        var query = new GetAccountQuery(accountId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(accountVm));
    }
}
