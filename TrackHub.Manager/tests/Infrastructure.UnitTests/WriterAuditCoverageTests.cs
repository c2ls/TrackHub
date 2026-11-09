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

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TrackHub.Manager.Infrastructure.UnitTests;

/// <summary>
/// Every writer method that persists an administrative mutation must leave an audit row: the audit
/// trail is the compliance record for the tenant, and a writer that saves silently is invisible to it.
/// </summary>
public class WriterAuditCoverageTests
{
    // Files whose whole purpose is to record an event or a metric. The row they write IS the record,
    // so an audit row on top would duplicate it.
    private static readonly HashSet<string> ExemptWriters =
    [
        "AlertEventWriter.cs",
        "ApiUsageUpsert.cs",
        "ApiUsageWriter.cs",
        "AuditEventWriter.cs",
        "BackgroundJobRunWriter.cs",
        "DirectApiUsageSink.cs",
        "PublicLinkGrantResolver.cs",
        // Per-user UI preferences (language, theme, navbar): no security or compliance meaning.
        "UserSettingsWriter.cs",
    ];

    // Machine bookkeeping written on every sync cycle or every outbound send. Auditing them would
    // bury the administrative trail under one row per operator per minute.
    private static readonly HashSet<string> ExemptMethods =
    [
        "OperatorWriter.MarkManualSyncTriggeredAsync",
        "OperatorWriter.UpdateSyncSummaryAsync",
        "OperatorWriter.SetSyncBackoffAsync",
        "OperatorWriter.ClearSyncBackoffAsync",
        "NotificationWriter.CreateNotificationDeliveryAsync",
        "NotificationWriter.MarkNotificationReadAsync",
    ];

    [Test]
    public void EveryWriterMethodThatSaves_AlsoRecordsAnAuditEvent()
    {
        var offenders = new List<string>();

        foreach (var file in Directory.EnumerateFiles(WritersDirectory(), "*.cs"))
        {
            var fileName = Path.GetFileName(file);
            if (ExemptWriters.Contains(fileName))
            {
                continue;
            }

            var source = File.ReadAllText(file);
            foreach (var member in Regex.Split(source, @"\r?\n(?=    (?:public|private|internal|protected))"))
            {
                if (!member.Contains("SaveChangesAsync(", StringComparison.Ordinal)
                    || member.Contains("AddAuditEvent(", StringComparison.Ordinal)
                    || member.Contains("AuditTrail.Create(", StringComparison.Ordinal))
                {
                    continue;
                }

                var name = $"{Path.GetFileNameWithoutExtension(fileName)}.{MethodName(member)}";
                if (!ExemptMethods.Contains(name))
                {
                    offenders.Add(name);
                }
            }
        }

        Assert.That(offenders, Is.Empty,
            "These writer methods persist a mutation without recording an audit event:\n"
            + string.Join("\n", offenders));
    }

    private static string MethodName(string member)
    {
        var match = Regex.Match(member, @"\s(\w+)\s*\(");
        return match.Success ? match.Groups[1].Value : "<unknown>";
    }

    private static string WritersDirectory([CallerFilePath] string testFilePath = "")
        => Path.GetFullPath(Path.Combine(
            Path.GetDirectoryName(testFilePath)!, "..", "..", "src", "Infrastructure", "ManagerDB", "Writers"));
}
