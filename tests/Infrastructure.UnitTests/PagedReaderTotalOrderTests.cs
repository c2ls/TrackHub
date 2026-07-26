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

using System.Text.RegularExpressions;

namespace Infrastructure.UnitTests;

/// <summary>
/// Guards the tiebreaker on every paged reader.
/// <para>
/// This is a SOURCE check on purpose. The behavioural tests in
/// <see cref="PagedReaderOrderingTests"/> run on the in-memory provider, whose <c>OrderBy</c> is
/// stable — so they keep passing with the tiebreaker deleted and cannot see this defect at all.
/// PostgreSQL makes no such promise: rows tied on the sort key come back in whatever order the plan
/// produces, which differs between the two queries a page boundary spans, so one row repeats on the
/// next page and another is dropped. The drop is silent — the caller receives a full page.
/// </para>
/// <para>
/// Asserting the source therefore encodes what the runtime cannot: a <c>Skip</c>/<c>Take</c> window
/// must never sit on an <c>OrderBy</c> that lacks a <c>ThenBy</c>.
/// </para>
/// </summary>
[TestFixture]
public partial class PagedReaderTotalOrderTests
{
    [GeneratedRegex(@"\.OrderBy(Descending)?\([^)]*\)\s*(?<next>\.\w+)", RegexOptions.Singleline)]
    private static partial Regex OrderByCall();

    /// <summary>
    /// A sort key already unique under the query's own filter totally orders the result on its own,
    /// and there is no tiebreaker left to add. Those sites carry a <c>// total-order:</c> comment
    /// saying why — the justification lives at the call site rather than in a list here that would
    /// silently drift away from the code it exempts.
    /// </summary>
    private const string ExemptionMarker = "// total-order:";

    private static DirectoryInfo ReadersDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "Infrastructure", "ManagerDB", "Readers");
            if (Directory.Exists(candidate))
            {
                return new DirectoryInfo(candidate);
            }

            dir = dir.Parent;
        }

        // Never Assert.Ignore here: a guard that quietly skips when it cannot find its subject is
        // indistinguishable from one that passes, which is how this class of defect returns.
        throw new DirectoryNotFoundException(
            "src/Infrastructure/ManagerDB/Readers not found above the test binary; the total-order guard cannot run.");
    }

    /// <summary>True when the ordering call is immediately preceded by a justification comment.</summary>
    private static bool IsExempt(string source, int orderByIndex)
        => source.LastIndexOf(ExemptionMarker, orderByIndex, StringComparison.Ordinal) is var marker
            && marker >= 0
            && source.AsSpan(marker, orderByIndex - marker).Count('\n') <= 4;

    [Test]
    public void Every_windowed_read_orders_by_more_than_one_key()
    {
        var offenders = new List<string>();

        foreach (var file in ReadersDirectory().GetFiles("*Reader.cs"))
        {
            var source = File.ReadAllText(file.FullName);
            if (!source.Contains(".Skip(", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in OrderByCall().Matches(source))
            {
                // A page window must sit on a composite sort. Anything continuing into Skip/Take
                // straight off a single OrderBy is a ragged page waiting to happen.
                if (match.Groups["next"].Value is not (".Skip" or ".Take"))
                {
                    continue;
                }

                if (IsExempt(source, match.Index))
                {
                    continue;
                }

                offenders.Add($"{file.Name}: '{match.Value.Trim()}' windows on a single sort key");
            }
        }

        Assert.That(offenders, Is.Empty,
            "every paged reader must break ties with a primary-key ThenBy:\n" + string.Join("\n", offenders));
    }

    [Test]
    public void Guard_locates_the_reader_sources()
    {
        var files = ReadersDirectory().GetFiles("*Reader.cs");
        Assert.That(files.Any(f => File.ReadAllText(f.FullName).Contains(".Skip(", StringComparison.Ordinal)),
            Is.True,
            "the total-order guard found no paged reader to inspect, so it would pass vacuously");
    }
}
