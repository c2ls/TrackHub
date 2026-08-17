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

import FormControl from '@mui/material/FormControl';
import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import type { SelectChangeEvent } from '@mui/material/Select';
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';

/** An option for a {@link CompactSelect}. */
export interface CompactSelectOption {
  value: string | number;
  label: string;
}

// Make the chevron visible (the theme hides it globally) and keep the
// control compact enough for a single-row filter bar.
const selectSx = {
  minWidth: 130,
  fontSize: '0.75rem',
  cursor: 'pointer',
  '& .MuiSelect-icon': {
    display: 'inline-block !important',
    right: 8,
    color: 'inherit',
    pointerEvents: 'none',
  },
  '& .MuiSelect-select': {
    width: '100% !important',
    paddingRight: '28px !important',
    paddingTop: '4px !important',
    paddingBottom: '4px !important',
    cursor: 'pointer',
  },
};

export interface CompactSelectProps {
  name: string;
  value: string | number;
  options: CompactSelectOption[];
  label: string;
  onChange: (name: string, value: string | number) => void;
}

// Compact select shared by the dashboard filter bar and the geofence toolbar:
// the neutral ("all") option doubles as the field label, so no stacked label is
// rendered and the bar stays a single light row.
function CompactSelect({ name, value, options, label, onChange }: CompactSelectProps) {
  const labelFor = (selected: string | number) => {
    const option = options.find((item) => item.value === selected);
    return option ? option.label : label;
  };
  return (
    <FormControl size="small" variant="outlined">
      <Select<string | number>
        id={name}
        name={name}
        value={value}
        onChange={(event: SelectChangeEvent<string | number>) => onChange(name, event.target.value)}
        displayEmpty
        size="small"
        IconComponent={KeyboardArrowDownIcon}
        inputProps={{ 'aria-label': label }}
        renderValue={(selected) => (selected === 'all' ? labelFor(selected) : `${label}: ${labelFor(selected)}`)}
        sx={selectSx}
      >
        {options.map((item) => (
          <MenuItem key={item.value} value={item.value}>
            {item.label}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}

export default CompactSelect;
