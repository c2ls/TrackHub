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

using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Application.PingOperator.Queries;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Interfaces.Registry;
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Models;
using Common.Domain.Enums;
using Application.UnitTests;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Application.UnitTests.PingOperator.Queries;

[TestFixture]
public class PingOperatorQueryTests : TestsContext
{
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<IOperatorReader> _operatorReaderMock = null!;
    private Mock<IConnectivityRegistry> _connectivityRegistryMock = null!;
    private Mock<IOperatorHealthCheckSystemWriter> _healthWriterMock = null!;

    [SetUp]
    public void SetUp()
    {
        _configurationMock = new Mock<IConfiguration>();
        _operatorReaderMock = new Mock<IOperatorReader>();
        _connectivityRegistryMock = new Mock<IConnectivityRegistry>();
        _healthWriterMock = new Mock<IOperatorHealthCheckSystemWriter>();

        _configurationMock.Setup(x => x["AppSettings:EncryptionKey"]).Returns("4F2C2E66-107F-452A-ACDE-402DFD47B84C");
    }

    private PingOperatorQueryHandler CreateHandler()
        => new(
            _configurationMock.Object,
            _operatorReaderMock.Object,
            _connectivityRegistryMock.Object,
            _healthWriterMock.Object,
            Mock.Of<ILogger<PingOperatorQueryHandler>>());

    [Test]
    public async Task Handle_WithCredential_PingsAndReturnsTrue()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), TestCredentialTokenVm);

        var testerMock = new Mock<IConnectivityTester>();
        testerMock.SetupGet(t => t.Protocol).Returns(ProtocolType.CommandTrack);
        testerMock.Setup(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);
        _connectivityRegistryMock.Setup(x => x.GetTester(It.IsAny<ProtocolType>())).Returns(testerMock.Object);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new PingOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.True);
        testerMock.Verify(t => t.Ping(It.IsAny<CredentialTokenDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _healthWriterMock.Verify(x => x.RecordAsync(
            It.Is<OperatorHealthCheckDto>(d => d.CheckType == "MANUAL" && d.Status == "HEALTHY" && d.OperatorId == operatorId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WithoutCredential_ReturnsFalse()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm(operatorId, (int)ProtocolType.CommandTrack, Guid.NewGuid(), null);

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new PingOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Handle_DisabledOperator_ReturnsFalseWithoutPingingProvider()
    {
        // Arrange
        var operatorId = Guid.NewGuid();
        var operatorVm = new OperatorVm
        {
            OperatorId = operatorId,
            ProtocolTypeId = (int)ProtocolType.CommandTrack,
            Credential = TestCredentialTokenVm,
            Enabled = false
        };

        _operatorReaderMock.Setup(x => x.GetOperatorAsync(operatorId, It.IsAny<CancellationToken>())).ReturnsAsync(operatorVm);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new PingOperatorQuery(operatorId), CancellationToken.None);

        // Assert
        Assert.That(result, Is.False);
        _connectivityRegistryMock.Verify(x => x.GetTester(It.IsAny<ProtocolType>()), Times.Never);
    }
}
