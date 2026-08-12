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

const transporterTypes = [
    { value: 0, label: 'UNKNOWN', icon: 'unknown_5' },
    { value: 1, label: 'AIRCRAFT', icon: 'flight' },
    { value: 2, label: 'ASSET', icon: 'box' },
    { value: 3, label: 'BICYCLE', icon: 'directions_bike' },
    { value: 4, label: 'BOAT', icon: 'directions_boat' },
    { value: 5, label: 'CAR', icon: 'directions_car' },
    { value: 6, label: 'CARGOCONTAINER', icon: 'rv_hookup' },
    { value: 7, label: 'CONSTRUCTIONVEHICLE', icon: 'front_loader' },
    { value: 8, label: 'CHILD', icon: 'child_care' },
    { value: 9, label: 'DELIVERYVAN', icon: 'airport_shuttle' },
    { value: 10, label: 'DRONE', icon: 'helicopter' },
    { value: 11, label: 'ELDERLYPERSON', icon: 'elderly' },
    { value: 12, label: 'FLEETVEHICLE', icon: 'car_rental' },
    { value: 13, label: 'HEAVYEQUIPMENT', icon: 'forklift' },
    { value: 14, label: 'LIVESTOCK', icon: 'corporate_fare' },
    { value: 15, label: 'MOTORCYCLE', icon: 'motorcycle' },
    { value: 16, label: 'PACKAGE', icon: 'package' },
    { value: 17, label: 'PERSON', icon: 'person' },
    { value: 18, label: 'PET', icon: 'pets' },
    { value: 19, label: 'SCHOOLBUS', icon: 'directions_bus' },
    { value: 20, label: 'SCOOTER', icon: 'scooter' },
    { value: 21, label: 'TAXI', icon: 'local_taxi' },
    { value: 22, label: 'TOOL', icon: 'tools_power_drill' },
    { value: 23, label: 'TRUCK', icon: 'local_shipping' },
    { value: 24, label: 'TRACTOR', icon: 'agriculture' },
  ] as const;

  export type TransporterType = (typeof transporterTypes)[number];
  export type TransporterTypeValue = TransporterType['value'];

  export default transporterTypes;