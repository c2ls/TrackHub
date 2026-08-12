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

using AppValidationException = Common.Application.Exceptions.ValidationException;

namespace TrackHub.TripManagement.Application.Common;

/// <summary>
/// Builds a <see cref="ValidationException"/> carrying one of the specific
/// <see cref="TripErrorCodes"/> literals rather than a generic failure (spec 11 §9), so spec 10's
/// offline error centre can explain a rejection instead of guessing. The code IS the message —
/// no localized string ever leaves the backend; the portal localizes it.
/// </summary>
public static class TripValidationFailure
{
    public static AppValidationException Create(string propertyName, string errorCode)
        => new(errorCode, [new FluentValidation.Results.ValidationFailure(propertyName, errorCode)]);
}
