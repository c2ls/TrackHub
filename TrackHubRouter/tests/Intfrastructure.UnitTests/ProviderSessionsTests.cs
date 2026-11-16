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

using TrackHub.Router.Domain.Helpers;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class ProviderSessionsTests
{
    private const string Scope = "test-session";
    private ProviderSessionStore _store = null!;
    private int _logins;

    [SetUp]
    public void SetUp()
    {
        _store = new ProviderSessionStore();
        _logins = 0;
    }

    private static CredentialTokenDto Credential(string? token = null, DateTimeOffset? expiration = null)
        => new(Guid.NewGuid(), "https://provider.example", "user", "secret", null, null, token, expiration, null, null);

    private Task<ProviderSessionGrant> LoginAsync(CancellationToken cancellationToken)
        => Task.FromResult(new ProviderSessionGrant($"session-{Interlocked.Increment(ref _logins)}", TimeSpan.FromHours(1)));

    [Test]
    public async Task AcquireAsync_ReusesTheCachedSession()
    {
        var credential = Credential();

        var first = await _store.AcquireAsync(Scope, credential, null, LoginAsync, CancellationToken.None);
        var second = await _store.AcquireAsync(Scope, credential, null, LoginAsync, CancellationToken.None);

        Assert.That((first, second, _logins), Is.EqualTo(("session-1", "session-1", 1)));
    }

    [Test]
    public async Task AcquireAsync_ReadersRejectedTogether_RenewOnce()
    {
        var credential = Credential();
        var rejected = await _store.AcquireAsync(Scope, credential, null, LoginAsync, CancellationToken.None);

        var renewed = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => _store.AcquireAsync(Scope, credential, rejected, LoginAsync, CancellationToken.None)));

        Assert.That(renewed, Is.All.EqualTo("session-2"));
        Assert.That(_logins, Is.EqualTo(2));
    }

    [Test]
    public async Task AcquireAsync_GrantWithoutLifetime_IsNotCached()
    {
        var credential = Credential();
        Task<ProviderSessionGrant> Uncacheable(CancellationToken _) => Task.FromResult(new ProviderSessionGrant($"session-{++_logins}", null));

        await _store.AcquireAsync(Scope, credential, null, Uncacheable, CancellationToken.None);
        var second = await _store.AcquireAsync(Scope, credential, null, Uncacheable, CancellationToken.None);

        Assert.That(second, Is.EqualTo("session-2"));
    }

    [Test]
    public async Task AcquireReusingStoredTokenAsync_LiveStoredToken_IsUsedOnlyUntilTheProviderRejectsIt()
    {
        var credential = Credential("stored", DateTimeOffset.UtcNow.AddHours(1));

        var first = await _store.AcquireReusingStoredTokenAsync(Scope, credential, null, LoginAsync, CancellationToken.None);
        var renewed = await _store.AcquireReusingStoredTokenAsync(Scope, credential, first, LoginAsync, CancellationToken.None);

        Assert.That((first, renewed, _logins), Is.EqualTo(("stored", "session-1", 1)));
    }

    [Test]
    public async Task AcquireReusingStoredTokenAsync_StoredTokenInsideTheMargin_Authenticates()
    {
        var credential = Credential("stored", DateTimeOffset.UtcNow.AddSeconds(30));

        var token = await _store.AcquireReusingStoredTokenAsync(Scope, credential, null, LoginAsync, CancellationToken.None);

        Assert.That(token, Is.EqualTo("session-1"));
    }

    [Test]
    public async Task AcquireAsync_ReadersRejectedTogether_RenewOnce_EvenWhenTheGrantCannotBeCached()
    {
        var credential = Credential();
        Task<ProviderSessionGrant> Uncacheable(CancellationToken _) => Task.FromResult(new ProviderSessionGrant($"session-{Interlocked.Increment(ref _logins)}", null));
        var rejected = await _store.AcquireAsync(Scope, credential, null, Uncacheable, CancellationToken.None);

        var renewed = await Task.WhenAll(Enumerable.Range(0, 5)
            .Select(_ => _store.AcquireAsync(Scope, credential, rejected, Uncacheable, CancellationToken.None)));

        Assert.That(renewed, Is.All.EqualTo("session-2"));
        Assert.That(_logins, Is.EqualTo(2));
    }

    [Test]
    public async Task AcquireAsync_FailedLogin_ReleasesTheGate()
    {
        var credential = Credential();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _store.AcquireAsync(Scope, credential, null, _ => throw new InvalidOperationException("down"), CancellationToken.None));
        var session = await _store.AcquireAsync(Scope, credential, null, LoginAsync, CancellationToken.None);

        Assert.That(session, Is.EqualTo("session-1"));
    }

    [Test]
    public async Task AcquireAsync_SlidingGrant_IsStoredAsSliding()
    {
        var store = new Mock<TrackHub.Router.Domain.Interfaces.IProviderSessionStore>();
        var credential = Credential();

        await store.Object.AcquireAsync(Scope, credential, null,
            _ => Task.FromResult(new ProviderSessionGrant("sid", TimeSpan.FromMinutes(4), Sliding: true)), CancellationToken.None);

        store.Verify(x => x.Set(credential, "sid", TimeSpan.FromMinutes(4), true), Times.Once);
    }

    [TestCase(null, 1)]
    [TestCase("", 1)]
    [TestCase("stored", null)]
    public void HasLiveStoredToken_NeedsATokenAndAnExpiry(string? token, int? expiresInHours)
        => Assert.That(ProviderSessions.HasLiveStoredToken(
            Credential(token, expiresInHours is { } hours ? DateTimeOffset.UtcNow.AddHours(hours) : null)), Is.False);

    [Test]
    public void HasLiveStoredToken_HonoursACustomMargin()
    {
        var credential = Credential("stored", DateTimeOffset.UtcNow.AddMinutes(3));

        Assert.That(ProviderSessions.HasLiveStoredToken(credential, TimeSpan.Zero), Is.True);
        Assert.That(ProviderSessions.HasLiveStoredToken(credential, TimeSpan.FromMinutes(5)), Is.False);
    }

    [Test]
    public void ExpiringAt_WithoutAnExpiry_HasNoLifetime()
        => Assert.That(ProviderSessions.ExpiringAt("token", null).TimeToLive, Is.Null);
}
