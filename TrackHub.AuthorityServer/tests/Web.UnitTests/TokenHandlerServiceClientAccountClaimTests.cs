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

using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Web.Endpoints;

namespace TrackHub.AuthorityServer.Web.UnitTests;

// Client-credentials tokens carry the account their permission grants declare, so an
// account-bound partner credential cannot reach another tenant. Platform-internal identities
// (router/syncworker/security/geofence/trip clients) hold declared cross-account grants and must
// keep receiving an UNSCOPED token — Security's matcher rejects an account-bearing token against
// an unbound grant.
[TestFixture]
public class TokenHandlerServiceClientAccountClaimTests
{
    private static ServiceClientAccountScopeVm Scope(bool allowsCrossAccount, params Guid[] accountIds)
        => new(allowsCrossAccount, accountIds);

    [Test]
    public void ResolveServiceClientAccount_SingleAccountGrant_BindsTheToken()
    {
        var accountId = Guid.NewGuid();

        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(null, Scope(false, accountId));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.EqualTo(accountId));
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_CrossAccountGrant_LeavesTheTokenUnscoped()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(null, Scope(true));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_CrossAccountGrantAlongsideBoundGrants_LeavesTheTokenUnscoped()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(null, Scope(true, Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_NoGrants_LeavesTheTokenUnscoped()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(null, Scope(false));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_SeveralAccountsWithoutARequest_IsRejectedAsAmbiguous()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(null, Scope(false, Guid.NewGuid(), Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Not.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_SeveralAccountsWithARequestedOne_BindsTheRequestedAccount()
    {
        var requested = Guid.NewGuid();

        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(
            requested.ToString(),
            Scope(false, Guid.NewGuid(), requested));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.EqualTo(requested));
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_RequestedAccountWithoutAGrant_IsRejected()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(
            Guid.NewGuid().ToString(),
            Scope(false, Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Not.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_RequestedAccountOnACrossAccountClient_IsRejected()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount(Guid.NewGuid().ToString(), Scope(true));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Not.Null);
        });
    }

    [Test]
    public void ResolveServiceClientAccount_UnparseableRequestedAccount_IsRejected()
    {
        var (resolved, error) = TokenHandler.ResolveServiceClientAccount("not-a-guid", Scope(false, Guid.NewGuid()));

        Assert.Multiple(() =>
        {
            Assert.That(resolved, Is.Null);
            Assert.That(error, Is.Not.Null);
        });
    }
}
