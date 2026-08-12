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
import { render, screen, fireEvent, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import ArgonAlert from 'components/ArgonAlert';
import { TestWrapper } from './testHelpers';

const renderAlert = (props: Partial<ComponentProps<typeof ArgonAlert>> = {}) =>
  render(
    <TestWrapper>
      <ArgonAlert {...props}>Alert content</ArgonAlert>
    </TestWrapper>
  );

describe('ArgonAlert', () => {
  test('renders children text', () => {
    renderAlert();
    expect(screen.getByText('Alert content')).toBeInTheDocument();
  });

  test('renders with default color (info)', () => {
    const { container } = renderAlert();
    expect(container.firstChild).toBeInTheDocument();
  });

  test('renders with custom color', () => {
    const { container } = renderAlert({ color: 'error' });
    expect(container.firstChild).toBeInTheDocument();
  });

  test('does not show close button when not dismissible', () => {
    renderAlert({ dismissible: false });
    expect(screen.queryByText('×')).not.toBeInTheDocument();
  });

  test('shows close button when dismissible', () => {
    renderAlert({ dismissible: true });
    expect(screen.getByText('×')).toBeInTheDocument();
  });

  test('hides alert when close button is clicked', () => {
    vi.useFakeTimers();
    renderAlert({ dismissible: true });

    const closeButton = screen.getByText('×');
    fireEvent.click(closeButton);

    // After clicking, alert should start fading out
    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(screen.queryByText('Alert content')).not.toBeInTheDocument();
    vi.useRealTimers();
  });

  test('renders with all valid color props', () => {
    const colors = ['primary', 'secondary', 'info', 'success', 'warning', 'error', 'light', 'dark'] as const;
    colors.forEach((color) => {
      const { unmount } = renderAlert({ color });
      unmount();
    });
  });
});
