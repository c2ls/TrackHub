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

namespace TrackHub.Router.Domain.Models;

// Device-sync counts returned by Manager's synchronizeOperatorDevices mutation.
// Manager no longer records the sync run; it returns these counts so the Router can be the single
// writer of sync-run telemetry, recording exactly one run per attempt.
public readonly record struct DeviceSyncCountsVm(
    int DevicesSeen,
    int DevicesAdded,
    int DevicesUpdated,
    int DevicesRemoved,
    int DevicesIgnored);
