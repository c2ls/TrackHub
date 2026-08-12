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

namespace TrackHub.Router.Infrastructure.Wialon.Models;

/// <summary>
/// Wialon reports failures as HTTP 200 with an <c>{"error": N}</c> body — a payload that
/// deserializes into any response model with all-default fields. Every response model carries the
/// error code so the reader base can distinguish "empty result" from "failed call"
/// (error 1 = invalid session → re-login and retry once; anything else → throw).
/// </summary>
internal interface IWialonResponse
{
    long? Error { get; }
}
