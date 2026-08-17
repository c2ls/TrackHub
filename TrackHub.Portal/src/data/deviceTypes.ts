/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

const deviceTypes = [
  { value: 0, label: 'Unknown' },
  { value: 1, label: 'Aviation' },
  { value: 2, label: 'Camera' },
  { value: 3, label: 'Cycling' },
  { value: 4, label: 'Cellular' },
  { value: 5, label: 'Drones' },
  { value: 6, label: 'Emergency Locator' },
  { value: 7, label: 'Fitness' },
  { value: 8, label: 'Handheld' },
  { value: 9, label: 'Marine' },
  { value: 10, label: 'OBD Scanner' },
  { value: 11, label: 'Pet Tracking' },
  { value: 12, label: 'Phone' },
  { value: 13, label: 'Satellite' },
  { value: 14, label: 'Smartwatch' },
  { value: 15, label: 'Wearable' },
] as const;

export type DeviceType = (typeof deviceTypes)[number];
export type DeviceTypeValue = DeviceType['value'];

export default deviceTypes;