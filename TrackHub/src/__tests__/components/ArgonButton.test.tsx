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
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import ArgonButton from 'components/ArgonButton';
import { TestWrapper } from './testHelpers';

const renderButton = (props: Partial<ComponentProps<typeof ArgonButton>> = {}) =>
  render(
    <TestWrapper>
      <ArgonButton {...props}>Click me</ArgonButton>
    </TestWrapper>
  );

describe('ArgonButton', () => {
  test('renders children text', () => {
    renderButton();
    expect(screen.getByText('Click me')).toBeInTheDocument();
  });

  test('renders as a button element', () => {
    renderButton();
    expect(screen.getByRole('button')).toBeInTheDocument();
  });

  test('fires onClick handler', () => {
    const handleClick = vi.fn();
    renderButton({ onClick: handleClick });

    fireEvent.click(screen.getByText('Click me'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  test('does not fire onClick when disabled', () => {
    const handleClick = vi.fn();
    renderButton({ onClick: handleClick, disabled: true });

    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).not.toHaveBeenCalled();
  });

  test('renders with different variants without crashing', () => {
    const variants = ['text', 'contained', 'outlined', 'gradient'] as const;
    variants.forEach((variant) => {
      const { unmount } = renderButton({ variant });
      expect(screen.getByText('Click me')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders with different sizes', () => {
    const sizes = ['small', 'medium', 'large'] as const;
    sizes.forEach((size) => {
      const { unmount } = renderButton({ size });
      expect(screen.getByText('Click me')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders with different colors', () => {
    const colors = ['primary', 'secondary', 'info', 'success', 'warning', 'error', 'dark', 'white'] as const;
    colors.forEach((color) => {
      const { unmount } = renderButton({ color });
      expect(screen.getByText('Click me')).toBeInTheDocument();
      unmount();
    });
  });

  test('renders circular button', () => {
    renderButton({ circular: true });
    expect(screen.getByRole('button')).toBeInTheDocument();
  });

  test('applies custom className', () => {
    renderButton({ className: 'custom-class' });
    expect(screen.getByRole('button')).toHaveClass('custom-class');
  });
});
