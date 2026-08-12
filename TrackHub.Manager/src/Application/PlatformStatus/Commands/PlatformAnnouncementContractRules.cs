using System.Linq.Expressions;

namespace TrackHub.Manager.Application.PlatformStatus.Commands;

// Shared create/update rules so the two commands cannot drift.
public static class PlatformAnnouncementContractRules
{
    public const int MessageMaxLength = 500;

    public static void Apply<T>(AbstractValidator<T> validator, Expression<Func<T, PlatformAnnouncementDto>> selector)
    {
        validator.RuleFor(selector).ChildRules(announcement =>
        {
            announcement.RuleFor(x => x.MessageEn)
                .NotEmpty()
                .MaximumLength(MessageMaxLength);

            announcement.RuleFor(x => x.MessageEs)
                .MaximumLength(MessageMaxLength);

            announcement.RuleFor(x => x.Severity)
                .IsInEnum();

            // A closed window must be a real window. Open-ended (null) on either side is valid.
            announcement.RuleFor(x => x.EndsAt)
                .GreaterThan(x => x.StartsAt!.Value)
                .When(x => x.StartsAt.HasValue && x.EndsAt.HasValue)
                .WithMessage("EndsAt must be later than StartsAt.");
        });
    }
}
