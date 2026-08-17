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

import type { ComponentProps } from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import ArgonProgress from 'components/ArgonProgress';
import { TestWrapper } from './testHelpers';

const renderProgress = (props: Partial<ComponentProps<typeof ArgonProgress>> = {}) =>
  render(
    <TestWrapper>
      <ArgonProgress {...props} />
    </TestWrapper>
  );

describe('ArgonProgress', () => {
  test('renders progress bar', () => {
    const { container } = renderProgress({ value: 50 });
    expect(container.querySelector('[role="progressbar"]')).toBeInTheDocument();
  });

  test('renders with correct aria value', () => {
    const { container } = renderProgress({ value: 75 });
    const progressbar = container.querySelector('[role="progressbar"]');
    expect(progressbar).toHaveAttribute('aria-valuenow', '75');
  });

  test('renders label when label prop is true', () => {
    renderProgress({ value: 60, label: true });
    expect(screen.getByText('60%')).toBeInTheDocument();
  });

  test('does not render label by default', () => {
    renderProgress({ value: 60 });
    expect(screen.queryByText('60%')).not.toBeInTheDocument();
  });

  test('renders with zero value', () => {
    const { container } = renderProgress({ value: 0 });
    const progressbar = container.querySelector('[role="progressbar"]');
    expect(progressbar).toHaveAttribute('aria-valuenow', '0');
  });

  test('renders with 100% value', () => {
    const { container } = renderProgress({ value: 100 });
    const progressbar = container.querySelector('[role="progressbar"]');
    expect(progressbar).toHaveAttribute('aria-valuenow', '100');
  });

  test('renders with different colors', () => {
    const colors = ['primary', 'info', 'success', 'warning', 'error'] as const;
    colors.forEach((color) => {
      const { unmount, container } = renderProgress({ color, value: 50 });
      expect(container.querySelector('[role="progressbar"]')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders with gradient variant', () => {
    const { container } = renderProgress({ variant: 'gradient', value: 50 });
    expect(container.querySelector('[role="progressbar"]')).toBeInTheDocument();
  });
});
