namespace TrackHub.Manager.Domain.Records;

// Server-side narrowing for the synchronized-device list. Every member is a query argument, not a
// post-filter over a loaded page: the list is one server page, so anything applied after the window
// would narrow that page alone and read as the whole account.
//
// DetectedStatus is NOT a plain column comparison. Assigned/Available are derived from the active
// assignments in the projection, so the reader rebuilds the same branches rather than comparing the
// stored column, which only ever holds Ignored or Removed.
public readonly record struct SynchronizedDeviceFilter(
    DetectedStatus? DetectedStatus = null,
    Guid? OperatorId = null,
    bool UnassignedOnly = false,
    DateTimeOffset? FirstSeenSince = null);
