// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using System.Reflection;
using Common.Application.Attributes;
using Common.Domain.Constants;
using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Accounts.Commands.Update;
using TrackHub.Manager.Application.Accounts.Commands.UpdateMaster;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

// The platform/account split on the account write surface: the systemadmin console edits ANY
// account through the AccountsMaster command (Administrator-only, declared cross-account), while
// the account-scoped twin stays guarded so an account administrator can only edit its own account.
[TestFixture]
public class UpdateAccountMasterCommandTests
{
    private Mock<IAccountWriter> _accountWriterMock;
    private UpdateAccountMasterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _accountWriterMock = new Mock<IAccountWriter>();
        _handler = new UpdateAccountMasterCommandHandler(_accountWriterMock.Object);
    }

    [Test]
    public async Task Handle_WhenCalled_CallsUpdate()
    {
        var dto = new UpdateAccountDto(Guid.NewGuid(), "Name", "Desc", 1, true);

        await _handler.Handle(new UpdateAccountMasterCommand(dto), CancellationToken.None);

        _accountWriterMock.Verify(w => w.UpdateAccountAsync(dto, CancellationToken.None), Times.Once);
    }

    [Test]
    public void Command_IsGatedByTheAdministratorOnlyMasterResource()
    {
        var authorize = typeof(UpdateAccountMasterCommand).GetCustomAttribute<AuthorizeAttribute>();

        Assert.That(authorize, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(authorize!.Resource, Is.EqualTo(Resources.AccountsMaster));
            Assert.That(authorize.Action, Is.EqualTo(Actions.Edit));
        });
    }

    [Test]
    public void Command_DeclaresItsCrossAccountReach()
        => Assert.That(
            typeof(UpdateAccountMasterCommand).GetCustomAttribute<AllowCrossAccountAttribute>(),
            Is.Not.Null,
            "the platform-side account edit targets accounts other than the caller's own");

    [Test]
    public void AccountScopedTwin_StaysGuarded()
        => Assert.That(
            typeof(UpdateAccountCommand).GetCustomAttribute<AllowCrossAccountAttribute>(),
            Is.Null,
            "marking the account-scoped edit would hand every Manager-role account administrator an unbound cross-tenant write");

    [Test]
    public void Validator_RejectsAnIncompleteAccount()
    {
        var validator = new UpdateAccountMasterValidator();

        Assert.Multiple(() =>
        {
            Assert.That(
                validator.TestValidate(new UpdateAccountMasterCommand(new UpdateAccountDto(Guid.Empty, "Name", null, 1, true))).IsValid,
                Is.False);
            Assert.That(
                validator.TestValidate(new UpdateAccountMasterCommand(new UpdateAccountDto(Guid.NewGuid(), string.Empty, null, 1, true))).IsValid,
                Is.False);
            Assert.That(
                validator.TestValidate(new UpdateAccountMasterCommand(new UpdateAccountDto(Guid.NewGuid(), "Name", null, 0, true))).IsValid,
                Is.False);
            Assert.That(
                validator.TestValidate(new UpdateAccountMasterCommand(new UpdateAccountDto(Guid.NewGuid(), "Name", "d", 1, true))).IsValid,
                Is.True);
        });
    }
}
