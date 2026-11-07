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

using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Common.Infrastructure;

/// <summary>
/// The one place a TrackHub service turns a configured connection string into the one it opens.
/// Npgsql pools per distinct connection string per process and defaults to 100 connections, so a
/// deployment of twenty services silently claims twenty times the server's whole budget; and with
/// no auto-prepare every statement is parsed and planned again on every execution.
/// </summary>
public static class DatabaseConnectionStrings
{
    public const int DefaultMaxPoolSize = 20;
    public const int DefaultConnectTimeoutSeconds = 15;
    public const int DefaultCommandTimeoutSeconds = 30;
    public const int DefaultMaxAutoPrepare = 20;
    public const int DefaultAutoPrepareMinUsages = 2;
    public const string DefaultTimezone = "UTC";

    public const string SectionName = "Database";

    public static string GetRequiredConnectionString(this IConfiguration configuration, string name)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' not found.");

        return Normalize(connectionString, configuration);
    }

    /// <summary>
    /// Fills in the pool ceiling, timeouts and auto-prepare settings the connection string does not
    /// state. Anything it states explicitly is left exactly as configured.
    /// </summary>
    public static string Normalize(string connectionString, IConfiguration? configuration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var section = configuration?.GetSection(SectionName);
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        // NpgsqlConnectionStringBuilder answers ContainsKey for every keyword it knows, set or not,
        // so the plain base builder is what distinguishes a stated value from a staff default.
        var stated = new DbConnectionStringBuilder { ConnectionString = connectionString };

        if (!IsStated(stated, "Maximum Pool Size", "MaxPoolSize"))
        {
            builder.MaxPoolSize = Value(section, "MaxPoolSize", DefaultMaxPoolSize);
        }

        if (!IsStated(stated, "Timeout"))
        {
            builder.Timeout = Value(section, "ConnectTimeoutSeconds", DefaultConnectTimeoutSeconds);
        }

        if (!IsStated(stated, "Command Timeout", "CommandTimeout"))
        {
            builder.CommandTimeout = Value(section, "CommandTimeoutSeconds", DefaultCommandTimeoutSeconds);
        }

        if (!IsStated(stated, "Max Auto Prepare", "MaxAutoPrepare"))
        {
            builder.MaxAutoPrepare = Value(section, "MaxAutoPrepare", DefaultMaxAutoPrepare);
        }

        if (!IsStated(stated, "Auto Prepare Min Usages", "AutoPrepareMinUsages"))
        {
            builder.AutoPrepareMinUsages = Value(section, "AutoPrepareMinUsages", DefaultAutoPrepareMinUsages);
        }

        // A UTC session on every connection, so no SQL result depends on the server zone.
        if (!IsStated(stated, "Timezone"))
        {
            builder.Timezone = DefaultTimezone;
        }

        return builder.ConnectionString;
    }

    private static bool IsStated(DbConnectionStringBuilder stated, params string[] keywords)
        => keywords.Any(stated.ContainsKey);

    private static int Value(IConfigurationSection? section, string key, int fallback)
        => section?.GetValue<int?>(key) ?? fallback;
}
