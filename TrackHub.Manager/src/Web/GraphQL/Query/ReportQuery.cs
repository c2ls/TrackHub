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

using TrackHub.Manager.Application.Reports.Queries.GetAll;
using TrackHub.Manager.Application.Reports.Queries.GetByCode;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{

    public async Task<IReadOnlyCollection<ReportVm>> GetReports([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetReportsQuery(), cancellationToken);

    public async Task<ReportVm?> GetReportByCode([Service] ISender sender, string code, CancellationToken cancellationToken)
        => await sender.Send(new GetReportByCodeQuery(code), cancellationToken);

}
