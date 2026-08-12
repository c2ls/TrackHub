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

global using TrackHub.Manager.Domain.Interfaces;
global using TrackHub.Manager.Domain.Records;
global using Ardalis.GuardClauses;
global using Microsoft.EntityFrameworkCore;
// NOTE: Common.Domain.Enums is deliberately NOT global here — it defines a TransporterType that
// collides with the Infrastructure.Entities table entity of the same name. Import it per file.
global using TrackHub.Manager.Domain.Enums;
global using TrackHub.Manager.Domain.Models;
global using TrackHub.Manager.Infrastructure.ManagerDB;
global using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
global using TrackHub.Manager.Infrastructure.ManagerDB.Writers;
