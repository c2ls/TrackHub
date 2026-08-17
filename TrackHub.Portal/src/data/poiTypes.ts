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

const poiTypes = [
    { value: 1, label: 'ClientSite' },
    { value: 2, label: 'Warehouse' },
    { value: 3, label: 'FuelStation' },
    { value: 4, label: 'TollBooth' },
    { value: 5, label: 'RestArea' },
    { value: 6, label: 'Workshop' },
    { value: 7, label: 'Port' },
    { value: 8, label: 'Other' }
  ] as const;

export type PoiType = (typeof poiTypes)[number];
export type PoiTypeValue = PoiType['value'];

const getPoiType = (value: number): string => {
  const poiType = poiTypes.find(type => type.value === value);
  return poiType ? poiType.label : '';
};

export {
  poiTypes,
  getPoiType
};
