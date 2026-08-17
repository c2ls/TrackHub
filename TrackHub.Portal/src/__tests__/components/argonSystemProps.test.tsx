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
 * Material UI v9 dropped the system shorthands from Box and Typography; ArgonBox and
 * ArgonTypography keep them by folding them into `sx` themselves. Several hundred call
 * sites depend on that, and the failure mode is silent (a prop that no longer styles
 * anything, or an Argon token swallowed by the fold), so both halves are pinned here.
 */

import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';
import colors from 'assets/theme/base/colors';
import { TestWrapper } from './testHelpers';

describe('Argon system props', () => {
  test('ArgonBox applies spacing and layout shorthands', () => {
    render(
      <TestWrapper>
        <ArgonBox mt={2} px={3} display="flex" width="12rem">
          content
        </ArgonBox>
      </TestWrapper>
    );

    const box = screen.getByText('content');
    // The Argon theme uses the default 8px spacing unit.
    expect(box).toHaveStyle({
      marginTop: '16px',
      paddingLeft: '24px',
      paddingRight: '24px',
      display: 'flex',
      width: '12rem',
    });
  });

  test('ArgonBox keeps its own palette-token props out of the fold', () => {
    render(
      <TestWrapper>
        <ArgonBox mt={1} bgColor="info" color="white">
          tokens
        </ArgonBox>
      </TestWrapper>
    );

    // `color`/`bgColor` take Argon palette keys, not CSS colours: they must still reach
    // ArgonBoxRoot's ownerState rather than being folded into sx as literal values.
    expect(screen.getByText('tokens')).toHaveStyle({
      background: colors.info.main,
      color: colors.white.main,
    });
  });

  test('ArgonTypography applies shorthands while keeping its own token props', () => {
    render(
      <TestWrapper>
        <ArgonTypography mb={2} display="block" color="error">
          label
        </ArgonTypography>
      </TestWrapper>
    );

    expect(screen.getByText('label')).toHaveStyle({
      marginBottom: '16px',
      display: 'block',
      color: colors.error.main,
    });
  });
});
