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

using Common.Application.Exceptions;
using HotChocolate;
using HotChocolate.Execution;

namespace Common.Web.Infrastructure;

public sealed class TrackHubGraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is ForbiddenAccessException forbidden)
        {
            var builder = ErrorBuilder.FromError(error)
                .SetMessage(forbidden.Message)
                .SetCode("FORBIDDEN");

            if (!string.IsNullOrWhiteSpace(forbidden.Resource))
            {
                builder.SetExtension("requiredResource", forbidden.Resource);
            }

            if (!string.IsNullOrWhiteSpace(forbidden.Action))
            {
                builder.SetExtension("requiredAction", forbidden.Action);
            }

            return builder.Build();
        }

        if (error.Exception is FeatureDisabledException featureDisabled)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(featureDisabled.Message)
                .SetCode("FEATURE_DISABLED")
                .SetExtension("featureKey", featureDisabled.FeatureKey)
                .Build();
        }

        if (error.Exception is AccountSuspendedException accountSuspended)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(accountSuspended.Message)
                .SetCode("ACCOUNT_SUSPENDED")
                .SetExtension("accountStatus", accountSuspended.Status.ToString())
                .Build();
        }

        if (error.Exception is TooManyRequestsException tooManyRequests)
        {
            var builder = ErrorBuilder.FromError(error)
                .SetMessage(tooManyRequests.Message)
                .SetCode("TOO_MANY_REQUESTS");

            if (tooManyRequests.RetryAfterSeconds is { } retryAfter)
            {
                builder.SetExtension("retryAfterSeconds", retryAfter);
            }

            return builder.Build();
        }

        if (error.Exception is UnauthorizedAccessException)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage("Authentication is required.")
                .SetCode("UNAUTHORIZED")
                .Build();
        }

        // The REST pipeline has mapped NotFoundException to a 404 ProblemDetails since day one
        // (CustomExceptionHandler), but the GraphQL filter never had a branch for it — so every
        // "not found" on a GraphQL surface arrived as an unmapped "Unexpected Execution Error" with
        // no code. That covers the DELIBERATE non-disclosure path: a cross-account transporter,
        // driver, geofence or document id is answered NotFound precisely so it cannot be probed,
        // and the caller could not tell that apart from a server fault.
        if (error.Exception is NotFoundException notFound)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(notFound.Message)
                .SetCode("NOT_FOUND")
                .Build();
        }

        if (error.Exception is ConflictException conflict)
        {
            // conflict.Code, not a flat "CONFLICT": the specific literal is what a client branches
            // on and what the portal translates. A hardcoded code collapsed
            // STOP_ALREADY_DEPARTED, TRIP_HAS_HISTORY, TRIP_DUPLICATE_CODE and
            // TOLL_OVERLAPPING_TARIFF into one indistinguishable error.
            return ErrorBuilder.FromError(error)
                .SetMessage(conflict.Message)
                .SetCode(conflict.Code)
                .Build();
        }

        // Without this branch a ValidationException fell through to `return error` and reached the
        // client as an unmapped "Unexpected Execution Error" with no code — including the specific,
        // localizable rejections the domain raises deliberately (TRIP_NOT_ACTIVE,
        // POD_DOCUMENT_NOT_CLEAN). Field-level failures travel in an `errors` extension so a form
        // can highlight the offending input instead of showing one flat message.
        if (error.Exception is ValidationException validation)
        {
            return ErrorBuilder.FromError(error)
                .SetMessage(validation.Message)
                .SetCode(validation.Code)
                .SetExtension("errors", validation.Errors)
                .Build();
        }

        return error;
    }
}
