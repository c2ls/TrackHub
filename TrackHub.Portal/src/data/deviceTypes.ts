/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

// Mirrors Common.Domain.Enums.DeviceType (server-side ids 1..15). The label is
// the lookup segment for the `deviceTypes.*` i18n keys; translate at render time.
const deviceTypes = [
    { value: 1, label: 'aviation' },
    { value: 2, label: 'camera' },
    { value: 3, label: 'cycling' },
    { value: 4, label: 'cellular' },
    { value: 5, label: 'drones' },
    { value: 6, label: 'emergency_locator' },
    { value: 7, label: 'fitness' },
    { value: 8, label: 'handheld' },
    { value: 9, label: 'marine' },
    { value: 10, label: 'obd_scanner' },
    { value: 11, label: 'pet_tracking' },
    { value: 12, label: 'phone' },
    { value: 13, label: 'satellite' },
    { value: 14, label: 'smartwatch' },
    { value: 15, label: 'wearable' },
  ] as const;

  export type DeviceTypeOption = (typeof deviceTypes)[number];
  export type DeviceTypeValue = DeviceTypeOption['value'];

  export default deviceTypes;
