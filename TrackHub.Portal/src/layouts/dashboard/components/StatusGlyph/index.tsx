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

import type { Theme } from '@mui/material/styles';
import ArgonBox from 'components/ArgonBox';
import type { UnitStatus } from 'layouts/dashboard/utils/dashboard';

interface StatusGlyphProps {
  status: UnitStatus;
  title?: string;
}

const GLYPH_COLOR = { moving: 'success', stopped: 'warning', offline: 'secondary' } as const;

// Same shapes as the map markers: an arrow when moving, a square when stopped, a ring when offline.
function StatusGlyph({ status, title }: StatusGlyphProps) {
  return (
    <ArgonBox
      component="span"
      title={title}
      sx={({ palette }: Theme) => {
        const color = palette[GLYPH_COLOR[status]].main;
        return {
          display: 'inline-block',
          flex: 'none',
          width: '0.625rem',
          height: '0.625rem',
          boxSizing: 'border-box',
          ...(status === 'moving' && {
            backgroundColor: color,
            clipPath: 'polygon(50% 0, 100% 100%, 50% 76%, 0 100%)',
          }),
          ...(status === 'stopped' && { backgroundColor: color, borderRadius: '2px' }),
          ...(status === 'offline' && { border: `2px solid ${color}`, borderRadius: '50%' }),
        };
      }}
    />
  );
}

export default StatusGlyph;
