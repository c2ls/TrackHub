namespace TrackHub.Manager.Application.AccountHolidays.Commands;

[Authorize(Resource = Resources.Accounts, Action = Actions.Edit)]
// Enforcement: AccountHolidayWriter.SetAccountHolidaysAsync calls RequireAccountWriteAccess on the account before touching rows.
[AccountScopeEnforcedInHandler]
public readonly record struct SetAccountHolidaysCommand(Guid AccountId, int Year, IReadOnlyCollection<AccountHolidayDto> Dates) : IRequest<IReadOnlyCollection<AccountHolidayVm>>;

public class SetAccountHolidaysCommandHandler(IAccountHolidayWriter writer) : IRequestHandler<SetAccountHolidaysCommand, IReadOnlyCollection<AccountHolidayVm>>
{
    public async Task<IReadOnlyCollection<AccountHolidayVm>> Handle(SetAccountHolidaysCommand request, CancellationToken cancellationToken)
        => await writer.SetAccountHolidaysAsync(request.AccountId, request.Year, request.Dates ?? [], cancellationToken);
}

public class SetAccountHolidaysCommandValidator : AbstractValidator<SetAccountHolidaysCommand>
{
    public SetAccountHolidaysCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(AccountHolidayRules.MinYear, AccountHolidayRules.MaxYear);
        RuleFor(x => x.Dates).NotNull().Must(d => d.Count <= AccountHolidayRules.MaxPerYear).WithMessage("Too many holidays for one year.");
        RuleForEach(x => x.Dates).ChildRules(holiday =>
        {
            holiday.RuleFor(h => h.Name).NotEmpty().MaximumLength(ColumnMetadata.DefaultNameLength);
        });
        RuleFor(x => x).Must(x => x.Dates == null || x.Dates.All(h => h.Date.Year == x.Year))
            .WithMessage("Every holiday must fall inside the selected year.");
    }
}
