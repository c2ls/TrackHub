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
import TextField from '@mui/material/TextField';
import type { TextFieldProps } from '@mui/material/TextField';
import ArgonBox from 'components/ArgonBox';
import FieldLabel from 'controls/Dialogs/FieldLabel';
import { textFieldSx } from 'controls/Dialogs/fieldStyles';

// The rendered field forwards any extra MUI TextField prop (value, onChange,
// type, placeholder, …); `label` is rendered through FieldLabel instead, and
// `errorMsg` maps onto the field's error/helper text.
export type CustomTextFieldProps = Omit<TextFieldProps, 'label'> & {
  errorMsg?: string;
  label?: ReactNode;
};

const marginMap: Record<'none' | 'dense' | 'normal', number> = { none: 0, dense: 1, normal: 2 };

const CustomTextField = ({
  errorMsg,
  label,
  id,
  name,
  required = false,
  fullWidth = true,
  margin = 'dense',
  ...props
}: CustomTextFieldProps) => {
  const top = marginMap[margin] ?? 1;
  return (
    <ArgonBox mt={top} mb={1} width={fullWidth ? '100%' : 'auto'}>
      {label && (
        <FieldLabel htmlFor={id || name} required={required}>
          {label}
        </FieldLabel>
      )}
      <TextField
        id={id}
        name={name}
        fullWidth={fullWidth}
        error={!!errorMsg}
        helperText={errorMsg}
        required={required}
        sx={textFieldSx}
        {...props}
      />
    </ArgonBox>
  );
};

export default CustomTextField;
