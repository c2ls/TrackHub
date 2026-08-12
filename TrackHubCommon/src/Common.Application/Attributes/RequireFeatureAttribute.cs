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

namespace Common.Application.Attributes;

/// <summary>
/// Declares that the decorated request requires the specified account feature flag
/// to be enabled for the resolved account. The <see cref="Common.Application.Behaviors.FeatureFlagBehavior{TRequest, TResponse}"/>
/// reads this attribute, resolves the target account (preferring the request's
/// <c>AccountId</c> property when present, falling back to the principal's account),
/// and throws <see cref="Common.Application.Exceptions.FeatureDisabledException"/>
/// when the flag is missing, disabled, or outside its effective window.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class RequireFeatureAttribute(string featureKey) : Attribute
{
    public string FeatureKey { get; } = featureKey;

    /// <summary>
    /// When true (default), service-client principals with no specific account scope
    /// bypass the flag check. Set to false for system-level commands that should still
    /// fail-closed against per-account feature configuration.
    /// </summary>
    public bool AllowGlobalServiceClient { get; set; } = true;
}
