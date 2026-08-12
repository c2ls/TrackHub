using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Web.Endpoints;

/// <summary>
/// The anonymous read surface behind the public platform status page (spec 28 ST-09).
///
/// This is REST rather than GraphQL on purpose: the mediator pipeline's logging, authorization and
/// account-status behaviors all assume a principal, and there is none here. The endpoint therefore
/// calls the reader directly and stays a thin, cacheable, rate-limited surface. It returns only
/// currently-visible announcements — drafts (Active = false) and out-of-window rows can never leak —
/// and it carries no secrets by construction.
/// </summary>
public sealed class PlatformStatus : Common.Web.Infrastructure.EndpointGroupBase
{
    /// <summary>Output-cache policy name; also referenced from Program.cs where the policy is defined.</summary>
    public const string CachePolicy = "platform-announcements";

    /// <summary>Rate-limit policy name; also referenced from Program.cs.</summary>
    public const string RateLimitPolicy = "platform-announcements";

    public override void Map(WebApplication app)
        => app.MapGet("~/api/PlatformStatus/announcements", GetAnnouncements)
            .AllowAnonymous()
            .CacheOutput(CachePolicy)
            .RequireRateLimiting(RateLimitPolicy);

    // GET ~/api/PlatformStatus/announcements — anonymous. Only Active rows inside their schedule window.
    public static async Task<IResult> GetAnnouncements(IPlatformAnnouncementReader reader, CancellationToken cancellationToken)
    {
        var announcements = await reader.GetVisiblePlatformAnnouncementsAsync(DateTimeOffset.UtcNow, cancellationToken);

        // Deliberately a narrow anonymous projection — Active/LastModified stay internal.
        return Results.Ok(announcements.Select(x => new
        {
            id = x.PlatformAnnouncementId,
            messageEn = x.MessageEn,
            messageEs = x.MessageEs,
            severity = x.Severity.ToString(),
            startsAt = x.StartsAt,
            endsAt = x.EndsAt
        }));
    }
}
