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
import ArgonInput from 'components/ArgonInput';
import { TestWrapper } from './testHelpers';

const renderInput = (props: Partial<ComponentProps<typeof ArgonInput>> = {}) =>
  render(
    <TestWrapper>
      <ArgonInput {...props} />
    </TestWrapper>
  );

describe('ArgonInput', () => {
  test('renders an input element', () => {
    renderInput({ placeholder: 'Enter text' });
    expect(screen.getByPlaceholderText('Enter text')).toBeInTheDocument();
  });

  test('accepts user input', () => {
    renderInput({ placeholder: 'Type here' });
    const input = screen.getByPlaceholderText('Type here') as HTMLInputElement;

    fireEvent.change(input, { target: { value: 'Hello World' } });
    expect(input.value).toBe('Hello World');
  });

  test('renders with error state', () => {
    const { container } = renderInput({ error: true, placeholder: 'Error field' });
    expect(screen.getByPlaceholderText('Error field')).toBeInTheDocument();
    expect(container.firstChild).toBeInTheDocument();
  });

  test('renders with success state', () => {
    const { container } = renderInput({ success: true, placeholder: 'Success field' });
    expect(screen.getByPlaceholderText('Success field')).toBeInTheDocument();
    expect(container.firstChild).toBeInTheDocument();
  });

  test('renders with disabled styling prop', () => {
    const { container } = renderInput({ disabled: true, placeholder: 'Disabled' });
    // ArgonInput disabled is a styling prop, not HTML disabled — it changes visual appearance
    expect(screen.getByPlaceholderText('Disabled')).toBeInTheDocument();
    expect(container.firstChild).toBeInTheDocument();
  });

  test('renders with different sizes', () => {
    const sizes = ['small', 'medium', 'large'] as const;
    sizes.forEach((size) => {
      const { unmount } = renderInput({ size, placeholder: `${size} input` });
      expect(screen.getByPlaceholderText(`${size} input`)).toBeInTheDocument();
      unmount();
    });
  });

  test('calls onChange handler', () => {
    const handleChange = vi.fn();
    renderInput({ onChange: handleChange, placeholder: 'Input' });

    fireEvent.change(screen.getByPlaceholderText('Input'), { target: { value: 'test' } });
    expect(handleChange).toHaveBeenCalledTimes(1);
  });
});
