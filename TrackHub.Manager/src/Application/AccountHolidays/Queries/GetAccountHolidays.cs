namespace TrackHub.Manager.Application.AccountHolidays.Queries;

[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
public readonly record struct GetAccountHolidaysQuery(Guid AccountId, int Year) : IRequest<IReadOnlyCollection<AccountHolidayVm>>;

public class GetAccountHolidaysQueryHandler(IAccountHolidayReader reader) : IRequestHandler<GetAccountHolidaysQuery, IReadOnlyCollection<AccountHolidayVm>>
{
    public async Task<IReadOnlyCollection<AccountHolidayVm>> Handle(GetAccountHolidaysQuery request, CancellationToken cancellationToken)
        => await reader.GetAccountHolidaysAsync(request.AccountId, request.Year, cancellationToken);
}

public class GetAccountHolidaysQueryValidator : AbstractValidator<GetAccountHolidaysQuery>
{
    public GetAccountHolidaysQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(AccountHolidayRules.MinYear, AccountHolidayRules.MaxYear);
    }
}
