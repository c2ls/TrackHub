namespace TrackHub.Manager.Domain.Interfaces;

public interface IGroupVisibilityReader
{
    Task<bool> ValidateGroupVisibilityAsync(Guid accountId, Guid userId, string resourceType, string resourceId, CancellationToken cancellationToken);
}
