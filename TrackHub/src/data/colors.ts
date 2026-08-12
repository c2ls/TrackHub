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

const colors = [
    { value: 1, label: 'Red' },
    { value: 2, label: 'Blue' },
    { value: 3, label: 'Green' },
    { value: 4, label: 'Yellow' },
    { value: 5, label: 'Orange' },
    { value: 6, label: 'Purple' },
    { value: 7, label: 'Pink' },
    { value: 8, label: 'Brown' },
    { value: 9, label: 'Black' },
    { value: 10, label: 'White' }
  ] as const;

  export type Color = (typeof colors)[number];
  export type ColorValue = Color['value'];
  export type ColorLabel = Color['label'];

  const getColor = (value: number): ColorLabel => {
    const color = colors.find(type => type.value === value);
    return color ? color.label : 'Black';
  };
  
  export { 
    colors, 
    getColor 
  };