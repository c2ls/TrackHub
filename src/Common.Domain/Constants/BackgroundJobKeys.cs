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

namespace Common.Domain.Constants;

/// <summary>
/// The canonical <c>BackgroundJobRun.JobKey</c> catalog. Manager owns the table, but Geofencing also
/// records runs through Manager's <c>createBackgroundJobRun</c> mutation, so these literals are
/// shared contract rather than one service's private constants.
/// <para>
/// Audited 2026-07-19 (spec 28 §4.2). Each producer was read and classified by whether it writes a
/// row on EVERY cycle or ONLY when work was actually performed. Consumers may assert staleness
/// ("this job looks stuck") ONLY for keys in <see cref="RecordsEveryCycle"/>; for every other key an
/// absent or old row is the normal, healthy state and must be presented neutrally.
/// </para>
/// <list type="table">
///   <item><term>alert-evaluation</term><description>Manager AlertEvaluationService — once per DAY unconditionally (credential-expiry scan), plus on-work rows for communication loss.</description></item>
///   <item><term>notification-dispatch</term><description>Manager NotificationDispatchService — only when it processed something.</description></item>
///   <item><term>notification-digest</term><description>Manager NotificationDigestService — only when it folded something.</description></item>
///   <item><term>delivery-retention</term><description>Manager DeliveryRetentionService — only when it deleted something.</description></item>
///   <item><term>document-scan</term><description>Manager DocumentScanService — only per scanned document (plus a Failed row on error).</description></item>
///   <item><term>document-expiration</term><description>Manager DocumentExpirationService — only per expiring document.</description></item>
///   <item><term>document-retention-cleanup</term><description>Manager DocumentRetentionCleanupService — only per purged version.</description></item>
///   <item><term>trial-expiration</term><description>Manager TrialExpirationService — only per transitioned account (plus a Failed row on error).</description></item>
///   <item><term>platform-retention</term><description>Manager PlatformRetentionService — only when it purged at least one job run or alert event.</description></item>
///   <item><term>geofence-dwell-evaluation</term><description>Geofencing DwellEvaluationService — only when it raised a dwell alert.</description></item>
///   <item><term>workforce-expiration-scan</term><description>Manager WorkforceExpirationService — only per raised qualification-expiration alert.</description></item>
///   <item><term>trip-eta-refresh</term><description>TripManagement TripEtaRefreshService — only when it refreshed an ETA for at least one in-progress trip.</description></item>
///   <item><term>trip-schedule-reminder</term><description>TripManagement TripScheduleReminderService — only when it raised a start-due reminder.</description></item>
/// </list>
/// <para>
/// Net result: <c>alert-evaluation</c> is the ONLY key with a guaranteed recording floor, and that
/// floor is daily — not per-cycle.
/// </para>
/// </summary>
public static partial class BackgroundJobKeys
{
    public const string AlertEvaluation = "alert-evaluation";
    public const string NotificationDispatch = "notification-dispatch";
    public const string NotificationDigest = "notification-digest";
    public const string DeliveryRetention = "delivery-retention";
    public const string DocumentScan = "document-scan";
    public const string DocumentExpiration = "document-expiration";
    public const string DocumentRetentionCleanup = "document-retention-cleanup";
    public const string TrialExpiration = "trial-expiration";
    public const string PlatformRetention = "platform-retention";
    public const string GeofenceDwellEvaluation = "geofence-dwell-evaluation";
    public const string WorkforceExpirationScan = "workforce-expiration-scan";
    public const string TripEtaRefresh = "trip-eta-refresh";
    public const string TripScheduleReminder = "trip-schedule-reminder";

    /// <summary>
    /// Keys whose producer writes a row on a guaranteed schedule, so a missing recent row is a real
    /// signal. <c>alert-evaluation</c> records unconditionally once per day; its staleness threshold
    /// is therefore measured in days, never in minutes.
    /// </summary>
    public static readonly IReadOnlySet<string> RecordsEveryCycle = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        AlertEvaluation,
    };

    /// <summary>Hours after which a <see cref="RecordsEveryCycle"/> job counts as stale.</summary>
    public const int StalenessThresholdHours = 26;

    public static bool IsCycleRecorded(string jobKey) => RecordsEveryCycle.Contains(jobKey);
}
