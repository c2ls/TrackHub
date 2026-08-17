/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*/

import type { SxProps, Theme } from '@mui/material/styles';

// The global theme forces `width: max-content !important` on the inner <input>,
// which causes end adornments (e.g. the password show/hide icon) to render in the
// middle of the control. These overrides keep the input filling its container so
// adornments sit flush against the right edge.
export const textFieldSx: SxProps<Theme> = {
    '& .MuiInputBase-root': {
        width: '100%',
    },
    '& .MuiInputBase-input': {
        width: '100% !important',
        flex: '1 1 auto',
        minWidth: 0,
    },
};
