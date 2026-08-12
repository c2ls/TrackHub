// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using TrackHub.Security.Application.UserPolicy.Commands.Create;

namespace Application.UnitTests.UserPolicy;

[TestFixture]
public class CreateUserPolicyCommandHandlerTests
{
    private Mock<IUserPolicyWriter> _writerMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IUserPolicyWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        var dto = new UserPolicyDto(Guid.NewGuid(), 5);
        var expectedVm = new UserPolicyVm(dto.UserId, 5);
        _writerMock.Setup(w => w.CreateUserPolicyAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateUserPolicyCommandHandler(_writerMock.Object, new Mock<Common.Mediator.IPublisher>().Object, new Mock<Common.Application.Interfaces.ICurrentPrincipal>().Object);
        var result = await handler.Handle(new CreateUserPolicyCommand(dto), CancellationToken.None);

        Assert.That(result, Is.EqualTo(expectedVm));
        _writerMock.Verify(w => w.CreateUserPolicyAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }
}
