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

// Values mirror Common.Domain.Enums.ProtocolType (TrackHubCommon) — keep in lockstep when a
// provider is added. Mettax (10) is reserved in the enum but has no Router provider assembly yet.
const protocolTypes = [
    { value: 1, label: 'COMMAND_TRACK' },
    { value: 2, label: 'TRACCAR' },
    { value: 3, label: 'FLESPI' },
    { value: 4, label: 'GEOTAB' },
    { value: 5, label: 'GPS_GATE' },
    { value: 6, label: 'NAVIXY' },
    { value: 7, label: 'SAMSARA' },
    { value: 8, label: 'WIALON' },
    { value: 9, label: 'PROTRACK' },
  ] as const;

  export type ProtocolType = (typeof protocolTypes)[number];
  export type ProtocolTypeValue = ProtocolType['value'];

  export default protocolTypes;