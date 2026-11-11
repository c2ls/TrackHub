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

import { useMemo, useState } from 'react';
import Autocomplete from '@mui/material/Autocomplete';
import TextField from '@mui/material/TextField';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import FieldLabel from 'controls/Dialogs/FieldLabel';
import { textFieldSx } from 'controls/Dialogs/fieldStyles';
import { useDebouncedValue } from 'utils/useDebouncedValue';

export interface SearchOption {
  value: string;
  label: string;
}

export type SearchOptionsHook = (search: string, take?: number) => { options: SearchOption[]; loading: boolean };

interface SearchSelectProps {
  id: string;
  label: string;
  value: string | null;
  valueLabel?: string | null;
  onChange: (option: SearchOption | null) => void;
  useOptions: SearchOptionsHook;
  placeholder?: string;
  disabled?: boolean;
  errorMsg?: string;
}

/** A single-select picker that searches the server as the user types, so no list is ever capped. */
function SearchSelect({ id, label, value, valueLabel, onChange, useOptions, placeholder, disabled = false, errorMsg }: SearchSelectProps) {
  const [input, setInput] = useState('');
  const search = useDebouncedValue(input);
  const { options, loading } = useOptions(search);

  // Autocomplete resets the typed text whenever the value object changes identity.
  const selectedLabel = valueLabel ?? options.find((option) => option.value === value)?.label ?? '';
  const selected = useMemo(() => (value ? { value, label: selectedLabel } : null), [value, selectedLabel]);

  return (
    <ArgonBox mt={1} mb={1}>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <Autocomplete
        id={id}
        disabled={disabled}
        options={options}
        value={selected}
        loading={loading}
        filterOptions={(items) => items}
        onInputChange={(_, next, reason) => {
          if (reason === 'input' || reason === 'clear') setInput(next);
        }}
        onChange={(_, option) => onChange(option)}
        getOptionLabel={(option) => option.label}
        isOptionEqualToValue={(option, current) => option.value === current.value}
        renderInput={(params) => <TextField {...params} placeholder={placeholder} sx={textFieldSx} error={Boolean(errorMsg)} />}
      />
      {errorMsg ? (
        <ArgonTypography variant="caption" color="error">
          {errorMsg}
        </ArgonTypography>
      ) : null}
    </ArgonBox>
  );
}

export default SearchSelect;
