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
using Microsoft.EntityFrameworkCore;
using Moq;
using TrackHub.Telemetry.Infrastructure.TelemetryDB;

namespace TrackHub.Telemetry.Application.UnitTests;

internal static class TestDb
{
    public static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"telemetry-{Guid.NewGuid()}")
            .Options);

    // A service-client principal scoped to a single account: passes RequireAccountAccess for that
    // account (Principal.AccountId == accountId) without granting global access.
    public static ICurrentPrincipal PrincipalFor(Guid accountId, PrincipalType type = PrincipalType.ServiceClient, Guid? userId = null, string? role = null)
    {
        var m = new Mock<ICurrentPrincipal>();
        m.SetupGet(p => p.PrincipalType).Returns(type);
        m.SetupGet(p => p.AccountId).Returns(accountId);
        m.SetupGet(p => p.UserId).Returns(userId);
        m.SetupGet(p => p.Role).Returns(role);
        return m.Object;
    }
}
