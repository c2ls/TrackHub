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
 * CustomSelect had no way to show a validation message, so a required select that failed
 * the screen's validate() gate simply refused to save with no feedback. These pin the
 * message surfacing and the invalid state — the failure mode is silence, which no
 * higher-level test would catch.
 */

import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import CustomSelect from 'controls/Dialogs/CustomSelect';
import { TestWrapper } from '../components/testHelpers';

const OPTIONS = [
  { value: 'a', label: 'Option A' },
  { value: 'b', label: 'Option B' },
];

const renderSelect = (errorMsg?: string) =>
  render(
    <TestWrapper>
      <CustomSelect
        list={OPTIONS}
        name="qualificationType"
        id="qualificationType"
        label="Type"
        value=""
        numericValue={false}
        required
        errorMsg={errorMsg}
      />
    </TestWrapper>
  );

describe('CustomSelect validation message', () => {
  test('renders the message and marks the field invalid', () => {
    renderSelect('Type is required');

    expect(screen.getByText('Type is required')).toBeInTheDocument();
    // The select must be programmatically invalid, not merely coloured.
    expect(screen.getByLabelText('Type')).toHaveAttribute('aria-invalid', 'true');
  });

  test('links the message to the listbox trigger, not the hidden native input', () => {
    renderSelect('Type is required');

    const message = screen.getByText('Type is required');
    const trigger = screen.getByRole('combobox');
    expect(trigger).toHaveAttribute('aria-describedby', message.id);
    // A message wired to the hidden input would never be announced.
    expect(trigger).not.toHaveAttribute('aria-hidden');
  });

  test('renders nothing extra when valid', () => {
    renderSelect();

    expect(screen.queryByText('Type is required')).not.toBeInTheDocument();
    expect(screen.getByLabelText('Type')).not.toHaveAttribute('aria-invalid', 'true');
  });
});
