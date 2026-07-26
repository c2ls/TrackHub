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

namespace Common.Application.Exceptions;

/// <summary>
/// Thrown when a create/update would violate a uniqueness constraint (duplicate resource).
/// Maps to HTTP 409 (REST) / the <c>CONFLICT</c> GraphQL error code.
/// </summary>
public class ConflictException : Exception
{
    /// <summary>The generic code used when a caller does not supply a specific one.</summary>
    public const string DefaultCode = "CONFLICT";

    public ConflictException()
        : this("The resource already exists.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
        Code = DefaultCode;
    }

    /// <summary>
    /// A conflict carrying a SPECIFIC machine-readable code (<c>STOP_ALREADY_DEPARTED</c>,
    /// <c>TRIP_DUPLICATE_CODE</c>, <c>TOLL_OVERLAPPING_TARIFF</c>, …).
    /// <para>
    /// The GraphQL error filter surfaces <see cref="Code"/> as the error code. Without this the
    /// filter emitted a flat <c>CONFLICT</c> for every conflict and left the specific literal
    /// buried in the message, so a client could not tell a duplicate trip code from a departed stop
    /// and every per-code translation in the portal was unreachable.
    /// </para>
    /// </summary>
    public ConflictException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Machine-readable code; <see cref="DefaultCode"/> unless one was supplied.</summary>
    public string Code { get; }

    /// <summary>
    /// A conflict whose code is also its message — the shape used where the backend emits a bare
    /// code literal and the client owns the wording. Preferred over
    /// <c>new ConflictException(code)</c>, which silently binds to the message-only constructor and
    /// leaves <see cref="Code"/> at <see cref="DefaultCode"/>.
    /// </summary>
    public static ConflictException WithCode(string code) => new(code, code);
}
