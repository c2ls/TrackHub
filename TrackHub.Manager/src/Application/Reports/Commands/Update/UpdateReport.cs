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

namespace TrackHub.Manager.Application.Reports.Commands.Update;

[Authorize(Resource = Resources.Reports, Action = Actions.Edit)]
[PlatformScoped("Spec-06 governed report catalog: one platform-owned row set (code, category, feature and format metadata) serves every tenant; no tenant owns a row.")]
public readonly record struct UpdateReportCommand(UpdateReportDto Report) : IRequest;

public class UpdateReportCommandHandler(IReportWriter writer) : IRequestHandler<UpdateReportCommand>
{
    // Handles the update report command
    public async Task Handle(UpdateReportCommand request, CancellationToken cancellationToken)
        => await writer.UpdateReportAsync(request.Report, cancellationToken);
}
