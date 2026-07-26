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

namespace DBInitializer;

/// <summary>
/// One module's slice of the governed report catalog. The initializer discovers every
/// implementation in this assembly and upserts the aggregate (per-code, idempotent), so a
/// feature module ships its report rows from its own file instead of growing a central
/// array. Implementations must have a public parameterless constructor.
/// </summary>
internal interface IReportCatalogContribution
{
    /// <summary>
    /// Report rows with their governance metadata: category grouping, RequiredFeatureKey
    /// gating (null = global), ManagerOnly role gating and SupportsPdf format support.
    /// </summary>
    IReadOnlyList<(string Code, string Description, string Category, string? RequiredFeatureKey, bool ManagerOnly, bool SupportsPdf, int SortOrder)> Reports { get; }
}
