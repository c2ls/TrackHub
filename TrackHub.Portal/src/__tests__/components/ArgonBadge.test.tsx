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
import ArgonBadge from 'components/ArgonBadge';
import { TestWrapper } from './testHelpers';

const renderBadge = (props: Partial<ComponentProps<typeof ArgonBadge>> = {}) =>
  render(
    <TestWrapper>
      <ArgonBadge {...props}>Badge Text</ArgonBadge>
    </TestWrapper>
  );

describe('ArgonBadge', () => {
  test('renders children text', () => {
    renderBadge();
    expect(screen.getByText('Badge Text')).toBeInTheDocument();
  });

  test('renders with different colors', () => {
    const colors = ['primary', 'secondary', 'info', 'success', 'warning', 'error', 'light', 'dark'] as const;
    colors.forEach((color) => {
      const { unmount } = renderBadge({ color });
      expect(screen.getByText('Badge Text')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders with gradient variant', () => {
    renderBadge({ variant: 'gradient' });
    expect(screen.getByText('Badge Text')).toBeInTheDocument();
  });

  test('renders with contained variant', () => {
    renderBadge({ variant: 'contained' });
    expect(screen.getByText('Badge Text')).toBeInTheDocument();
  });

  test('renders with different sizes', () => {
    const sizes = ['xs', 'sm', 'md', 'lg'] as const;
    sizes.forEach((size) => {
      const { unmount } = renderBadge({ size });
      expect(screen.getByText('Badge Text')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders circular badge', () => {
    renderBadge({ circular: true });
    expect(screen.getByText('Badge Text')).toBeInTheDocument();
  });

  test('renders indicator badge', () => {
    const { container } = renderBadge({ indicator: true });
    expect(container.firstChild).toBeInTheDocument();
  });
});
