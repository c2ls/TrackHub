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

using Common.Mediator;
using HotChocolate;
using HotChocolate.Execution;
using Moq;
using TrackHub.ServiceContracts.Harness;
using GeofenceMutation = TrackHub.Manager.Web.GraphQL.Mutation.Mutation;
using GeofenceQuery = TrackHub.Manager.Web.GraphQL.Query.Query;

namespace TrackHub.ServiceContracts.Geofence.Tests;

// The Geofence service's REAL root types. Note: the Geofencing repo was cloned from Manager
// and kept the TrackHub.Manager.Web.GraphQL.* namespaces — in THIS project those types come
// from the TrackHub.Manager.Geofencing assembly (Manager's own Web is not referenced here).
internal static class GeofenceProducerSchema
{
    public static Task<ISchemaDefinition> BuildSchemaAsync()
        => ProducerSchemaBuilder.BuildSchemaAsync<GeofenceQuery, GeofenceMutation>(Mock.Of<ISender>());

    public static Task<IRequestExecutor> BuildExecutorAsync(ISender sender)
        => ProducerSchemaBuilder.BuildExecutorAsync<GeofenceQuery, GeofenceMutation>(sender);
}
