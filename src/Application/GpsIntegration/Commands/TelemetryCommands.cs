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

namespace TrackHub.Telemetry.Application.GpsIntegration.Commands;

// Operator health is core (spec 07 section 3): no feature gate. Per the Slice B decision the handler
// no longer writes the denormalized operator health-summary columns (Telemetry has read-only access
// to the operator master row); the summary is derived from the telemetry tables at read time.
[Authorize(Resource = Resources.OperatorHealth, Action = Actions.Write, PrincipalTypes = "ServiceClient")]
public readonly record struct RecordOperatorHealthCommand(OperatorHealthCheckDto Check) : IRequest<OperatorHealthCheckVm>;

public class RecordOperatorHealthCommandHandler(IOperatorHealthCheckWriter writer)
    : IRequestHandler<RecordOperatorHealthCommand, OperatorHealthCheckVm>
{
    public async Task<OperatorHealthCheckVm> Handle(RecordOperatorHealthCommand request, CancellationToken cancellationToken)
        => await writer.RecordAsync(request.Check, cancellationToken);
}
