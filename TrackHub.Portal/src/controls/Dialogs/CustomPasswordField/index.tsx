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

import { useState } from 'react';
import type { ReactNode } from 'react';
import TextField from '@mui/material/TextField';
import type { TextFieldProps } from '@mui/material/TextField';
import InputAdornment from '@mui/material/InputAdornment';
import IconButton from '@mui/material/IconButton';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import ArgonBox from 'components/ArgonBox';
import FieldLabel from 'controls/Dialogs/FieldLabel';
import { textFieldSx } from 'controls/Dialogs/fieldStyles';

// Forwards any extra MUI TextField prop; `label` is rendered through FieldLabel
// and `errorMsg` maps onto the field's error/helper text. A show/hide toggle is
// injected as an end adornment.
export type CustomPasswordFieldProps = Omit<TextFieldProps, 'label' | 'type'> & {
  errorMsg?: string;
  label?: ReactNode;
};

const marginMap: Record<'none' | 'dense' | 'normal', number> = { none: 0, dense: 1, normal: 2 };

const CustomPasswordField = ({
  errorMsg,
  label,
  id,
  name,
  required = false,
  fullWidth = true,
  margin = 'dense',
  ...props
}: CustomPasswordFieldProps) => {
  const [showPassword, setShowPassword] = useState(false);
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
        type={showPassword ? 'text' : 'password'}
        fullWidth={fullWidth}
        error={!!errorMsg}
        helperText={errorMsg}
        required={required}
        sx={textFieldSx}
        InputProps={{
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                aria-label="toggle password visibility"
                size="small"
                onClick={() => setShowPassword((s) => !s)}
                onMouseDown={(e) => e.preventDefault()}
              >
                {showPassword ? <Visibility fontSize="small" /> : <VisibilityOff fontSize="small" />}
              </IconButton>
            </InputAdornment>
          ),
        }}
        {...props}
      />
    </ArgonBox>
  );
};

export default CustomPasswordField;
