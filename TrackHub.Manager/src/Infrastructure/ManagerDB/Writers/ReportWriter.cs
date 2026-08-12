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

using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

public sealed class ReportWriter(IApplicationDbContext context) : IReportWriter
{
    /// <summary>
    /// Updates an existing report in the database with the provided data.
    /// </summary>
    /// <param name="reportDto">The data transfer object containing the updated report information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the report with the specified ID is not found.</exception>
    public async Task UpdateReportAsync(UpdateReportDto reportDto, CancellationToken cancellationToken)
    {
        var report = await context.Reports.FindAsync(reportDto.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), $"{reportDto.ReportId}");

        context.Reports.Attach(report);

        report.Description = reportDto.Description;
        report.Type = reportDto.TypeId;
        report.Active = reportDto.Active;
        report.Category = reportDto.Category;
        report.RequiredFeatureKey = reportDto.RequiredFeatureKey;
        report.ManagerOnly = reportDto.ManagerOnly;
        report.SupportsPdf = reportDto.SupportsPdf;
        report.SortOrder = reportDto.SortOrder;

        await context.SaveChangesAsync(cancellationToken);
    }
}
