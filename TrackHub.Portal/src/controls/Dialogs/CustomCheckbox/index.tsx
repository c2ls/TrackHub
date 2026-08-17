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

import type { ReactNode } from 'react';
import { FormControlLabel, Checkbox } from '@mui/material';
import type { FormChangeHandler } from 'controls/Dialogs/useForm';

interface CustomCheckboxProps {
  handleChange?: FormChangeHandler;
  name: string;
  id: string;
  value?: boolean;
  label: ReactNode;
}

const CustomCheckbox = ({ handleChange, name, id, value, label }: CustomCheckboxProps) => (
  <FormControlLabel
    control={
      <Checkbox
        name={name}
        id={id}
        checked={value}
        onChange={handleChange}
      />
    }
    label={label}
  />
);

export default CustomCheckbox;
