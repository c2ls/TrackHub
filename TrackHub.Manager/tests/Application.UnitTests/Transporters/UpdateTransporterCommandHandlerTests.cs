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

using TrackHub.Manager.Application.Transporters.Commands.Update;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Transporters;

[TestFixture]
public class UpdateTransporterCommandHandlerTests
{
    private Mock<ITransporterWriter> _writerMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<ITransporterWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        var dto = new UpdateTransporterDto(Guid.NewGuid(), "Updated", 2);
        var handler = new UpdateTransporterCommandHandler(_writerMock.Object);

        await handler.Handle(new UpdateTransporterCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateTransporterAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesExactTransporterId()
    {
        var transporterId = Guid.NewGuid();
        var dto = new UpdateTransporterDto(transporterId, "Name", 1);
        var handler = new UpdateTransporterCommandHandler(_writerMock.Object);

        await handler.Handle(new UpdateTransporterCommand(dto), CancellationToken.None);

        _writerMock.Verify(w => w.UpdateTransporterAsync(
            It.Is<UpdateTransporterDto>(d => d.TransporterId == transporterId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
