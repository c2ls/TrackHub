// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Accounts.Queries.GetMaster;
using TrackHub.Manager.Domain.Interfaces;
using Common.Application.GraphQL.Inputs;
using Common.Domain.Helpers;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountSettingsMasterQueryTests
{
    private Mock<IAccountSettingsReader> _readerMock;
    private GetAccountSettingsMasterQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _readerMock = new Mock<IAccountSettingsReader>();
        _handler = new GetAccountSettingsMasterQueryHandler(_readerMock.Object);
    }

    [Test]
    public async Task Handle_CallsReader_WithFilters()
    {
        var filtersInput = new FiltersInput();
        var list = new List<AccountSettingsVm>();
        _readerMock.Setup(r => r.GetAccountSettingsAsync(It.IsAny<Filters>(), CancellationToken.None)).ReturnsAsync(list);

        var result = await _handler.Handle(new GetAccountSettingsMasterQuery(filtersInput), CancellationToken.None);

        Assert.That(result, Is.EqualTo(list));
    }

    [Test]
    public void Validator_Should_Have_Error_When_Filter_Is_Null()
    {
        var validator = new GetAccountSettingsMasterValidator();
        var query = new GetAccountSettingsMasterQuery(null!);
        var result = validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Filter);
    }
}
