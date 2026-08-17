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

/**
 * The geofence list sits in the narrow `lg:3` panel beside the map and is the only
 * table in the portal with explicit column widths. Its delete button must stay
 * narrower than that column — a default MUI Button carries `min-width: 64px`, which
 * overflows the action column and gets clipped by the scroll container.
 */

import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import Icon from '@mui/material/Icon';
import ArgonButton from 'components/ArgonButton';
import { TestWrapper } from '../components/testHelpers';

describe('geofence list action button', () => {
  test('iconOnly replaces the 64px button min-width with a square', () => {
    render(
      <TestWrapper>
        <ArgonButton variant="text" color="error" size="small" iconOnly title="Delete">
          <Icon>delete</Icon>
        </ArgonButton>
      </TestWrapper>
    );

    // 25.4px is ArgonButton's small iconOnly square (ArgonButtonRoot iconOnlyStyles).
    expect(screen.getByTitle('Delete')).toHaveStyle({
      minWidth: '1.5875rem',
      width: '1.5875rem',
    });
  });

  test('without iconOnly the button keeps the 64px min-width that overflowed', () => {
    render(
      <TestWrapper>
        <ArgonButton variant="text" color="error" title="Delete">
          <Icon>delete</Icon>
        </ArgonButton>
      </TestWrapper>
    );

    expect(screen.getByTitle('Delete')).toHaveStyle({ minWidth: '64px' });
  });
});
