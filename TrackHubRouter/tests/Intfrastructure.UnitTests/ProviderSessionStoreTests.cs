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

using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Common;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class ProviderSessionStoreTests
{
    private ProviderSessionStore _store;

    private static CredentialTokenDto CreateCredential(
        Guid? credentialId = null,
        string password = "secret")
        => new(
            credentialId ?? Guid.NewGuid(),
            "https://provider.example.com",
            "user",
            password,
            null,
            null,
            "api-token",
            null,
            null,
            null);

    [SetUp]
    public void Setup() => _store = new ProviderSessionStore();

    [Test]
    public void TryGet_WithNoEntry_ReturnsFalse()
    {
        var found = _store.TryGet(CreateCredential(), out var session);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.False);
            Assert.That(session, Is.Empty);
        });
    }

    [Test]
    public void TryGet_AfterSet_ReturnsStoredSession()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromMinutes(5));

        var found = _store.TryGet(credential, out var session);

        Assert.Multiple(() =>
        {
            Assert.That(found, Is.True);
            Assert.That(session, Is.EqualTo("sid-1"));
        });
    }

    [Test]
    public void TryGet_WithRotatedCredential_ReturnsFalse()
    {
        var credentialId = Guid.NewGuid();
        var original = CreateCredential(credentialId, password: "old-password");
        _store.Set(original, "sid-1", TimeSpan.FromMinutes(5));

        var rotated = CreateCredential(credentialId, password: "new-password");
        var found = _store.TryGet(rotated, out _);

        Assert.That(found, Is.False, "a rotated credential must never resolve the old session");
    }

    [Test]
    public void TryGet_WithExpiredEntry_ReturnsFalse()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromMilliseconds(1));

        Thread.Sleep(30);

        Assert.That(_store.TryGet(credential, out _), Is.False);
    }

    [Test]
    public void Set_WithNonPositiveTtl_DoesNotStore()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromSeconds(-5), sliding: false);

        Assert.That(_store.TryGet(credential, out _), Is.False);
    }

    [Test]
    public void Invalidate_RemovesEntry()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromMinutes(5));

        _store.Invalidate(credential.CredentialId);

        Assert.That(_store.TryGet(credential, out _), Is.False);
    }

    [Test]
    public void Set_OverwritesPreviousSession()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromMinutes(5));
        _store.Set(credential, "sid-2", TimeSpan.FromMinutes(5));

        _store.TryGet(credential, out var session);

        Assert.That(session, Is.EqualTo("sid-2"));
    }

    [Test]
    public void TryGet_SlidingEntry_ExtendsExpiration()
    {
        var credential = CreateCredential();
        _store.Set(credential, "sid-1", TimeSpan.FromMilliseconds(200));

        // Keep touching the entry past its original absolute expiry; sliding must keep it alive.
        for (var i = 0; i < 4; i++)
        {
            Thread.Sleep(80);
            Assert.That(_store.TryGet(credential, out _), Is.True,
                $"sliding entry expired after touch {i}");
        }
    }

    [Test]
    public void TryGet_NonSlidingEntry_ExpiresDespiteUse()
    {
        var credential = CreateCredential();
        _store.Set(credential, "token-1", TimeSpan.FromMilliseconds(150), sliding: false);

        Assert.That(_store.TryGet(credential, out _), Is.True);
        Thread.Sleep(200);
        Assert.That(_store.TryGet(credential, out _), Is.False,
            "non-sliding entries must expire at their absolute TTL even when used");
    }

    [Test]
    public void Entries_AreIsolatedPerCredential()
    {
        var first = CreateCredential();
        var second = CreateCredential();
        _store.Set(first, "sid-first", TimeSpan.FromMinutes(5));
        _store.Set(second, "sid-second", TimeSpan.FromMinutes(5));

        _store.Invalidate(first.CredentialId);

        Assert.Multiple(() =>
        {
            Assert.That(_store.TryGet(first, out _), Is.False);
            Assert.That(_store.TryGet(second, out var session), Is.True);
            Assert.That(session, Is.EqualTo("sid-second"));
        });
    }
}
