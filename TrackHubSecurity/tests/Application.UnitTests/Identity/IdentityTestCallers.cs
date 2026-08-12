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

using Common.Application.Interfaces;

namespace Application.UnitTests.Identity;

// Identity queries pin their subject to the caller (IdentityCallerGuard); these helpers
// build the two legitimate caller shapes for handler tests.
internal static class IdentityTestCallers
{
    public static Mock<IUser> Service(string client = "my-client")
    {
        var mock = new Mock<IUser>();
        mock.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        mock.Setup(u => u.Role).Returns("service");
        mock.Setup(u => u.Client).Returns(client);
        return mock;
    }

    public static Mock<IUser> User(Guid userId)
    {
        var mock = new Mock<IUser>();
        mock.Setup(u => u.Id).Returns(userId.ToString());
        mock.Setup(u => u.Role).Returns("admin");
        return mock;
    }
}
