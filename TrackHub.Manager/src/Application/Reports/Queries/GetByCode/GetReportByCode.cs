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

namespace TrackHub.Manager.Application.Reports.Queries.GetByCode;

// Metadata lookup consumed by the Reporting service at execution time. Returns
// the raw catalog row (including inactive rows) with no visibility filtering; Reporting enforces the
// RequiredFeatureKey/ManagerOnly gates itself. Null for an unknown code.
[Authorize(Resource = Resources.Reports, Action = Actions.Read)]
public readonly record struct GetReportByCodeQuery(string Code) : IRequest<ReportVm?>;

public class GetReportByCodeQueryHandler(IReportReader reader) : IRequestHandler<GetReportByCodeQuery, ReportVm?>
{
    public async Task<ReportVm?> Handle(GetReportByCodeQuery request, CancellationToken cancellationToken)
        => await reader.GetReportByCodeAsync(request.Code, cancellationToken);
}
