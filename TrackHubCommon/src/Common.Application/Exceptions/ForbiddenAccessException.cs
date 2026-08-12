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

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : this("Insufficient permissions.")
    {
    }

    public ForbiddenAccessException(string message)
        : base(message)
    {
    }

    public ForbiddenAccessException(string resource, string action, string? reason = null)
        : base(CreateMessage(resource, action, reason))
    {
        Resource = resource;
        Action = action;
        Reason = reason;
    }

    public string? Resource { get; }
    public string? Action { get; }
    public string? Reason { get; }

    private static string CreateMessage(string resource, string action, string? reason)
    {
        var permission = string.IsNullOrWhiteSpace(resource) && string.IsNullOrWhiteSpace(action)
            ? "unspecified"
            : $"{resource}.{action}".Trim('.');

        return string.IsNullOrWhiteSpace(reason)
            ? $"Insufficient permissions. Required permission: {permission}."
            : $"Insufficient permissions. Required permission: {permission}. {reason}";
    }
}
