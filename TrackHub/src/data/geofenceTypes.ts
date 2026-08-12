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

const geofenceTypes = [
    { value: 1, label: 'ClientLocation' },
    { value: 2, label: 'ConstructionSite' },
    { value: 3, label: 'DangerZone' },
    { value: 4, label: 'FuelStation' },
    { value: 5, label: 'Garage' },
    { value: 6, label: 'Hospital' },
    { value: 7, label: 'Hotel' },
    { value: 8, label: 'Office' },
    { value: 9, label: 'Park' },
    { value: 10, label: 'ParkingLot' },
    { value: 11, label: 'RestrictedArea' },
    { value: 12, label: 'RetailStore' },
    { value: 13, label: 'School' },
    { value: 14, label: 'Warehouse' }
  ] as const;

export type GeofenceType = (typeof geofenceTypes)[number];
export type GeofenceTypeValue = GeofenceType['value'];

const getGeofenceType = (value: number): string => {
  const geofence = geofenceTypes.find(type => type.value === value);
  return geofence ? geofence.label : '';
};

export { 
  geofenceTypes, 
  getGeofenceType 
};