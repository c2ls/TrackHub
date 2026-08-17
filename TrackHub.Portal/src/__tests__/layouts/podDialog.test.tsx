/**
 * Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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

import { render, screen, fireEvent } from '@testing-library/react';
import { TestWrapper } from '../components/testHelpers';
import PodDialog from 'layouts/tripmanager/components/PodDialog';
import type { PodAttachment } from 'layouts/tripmanager/tripWriteForms';

// i18next is not initialised in the test environment; echo the key so the
// assertions read against a stable identifier rather than a translation.
vi.mock('react-i18next', () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

const DOC_ID = '11111111-1111-1111-1111-111111111111';
const STOP_ID = '22222222-2222-2222-2222-222222222222';

const attachment = (scanStatus: string): PodAttachment => ({
  documentId: DOC_ID,
  fileName: 'signature.png',
  scanStatus,
});

function renderDialog(overrides: Partial<React.ComponentProps<typeof PodDialog>> = {}) {
  const props: React.ComponentProps<typeof PodDialog> = {
    open: true,
    setOpen: vi.fn(),
    handleSubmit: vi.fn(),
    values: { tripStopId: STOP_ID, receiverName: 'Ana' },
    handleChange: vi.fn(),
    errors: {},
    stopLabel: '1. Warehouse',
    deliveries: [],
    attachments: [],
    onUploadFiles: vi.fn(),
    onRemoveAttachment: vi.fn(),
    onRefreshAttachment: vi.fn(),
    uploading: false,
    ...overrides,
  };
  render(
    <TestWrapper>
      <PodDialog {...props} />
    </TestWrapper>
  );
  return props;
}

describe('PodDialog', () => {
  test('warns while an attachment has not scanned clean', () => {
    // The backend rejects the whole capture with POD_DOCUMENT_NOT_CLEAN, so the
    // condition has to be visible before the round-trip is even attempted.
    renderDialog({ attachments: [attachment('Pending')] });

    expect(screen.getByText('pod.notCleanWarning')).toBeInTheDocument();
    expect(screen.getByText('pod.recheck')).toBeInTheDocument();
  });

  test('a clean attachment raises no warning and offers no re-check', () => {
    renderDialog({ attachments: [attachment('Clean')] });

    expect(screen.queryByText('pod.notCleanWarning')).not.toBeInTheDocument();
    expect(screen.queryByText('pod.recheck')).not.toBeInTheDocument();
  });

  test('re-checking asks for the scan verdict of that document', () => {
    const props = renderDialog({ attachments: [attachment('Pending')] });

    fireEvent.click(screen.getByText('pod.recheck'));

    expect(props.onRefreshAttachment).toHaveBeenCalledWith(DOC_ID);
  });

  test('removing an attachment reports the document id', () => {
    const props = renderDialog({ attachments: [attachment('Clean')] });

    fireEvent.click(screen.getByText('generic.delete'));

    expect(props.onRemoveAttachment).toHaveBeenCalledWith(DOC_ID);
  });

  test('a stop with no delivery lines says the POD covers the whole stop', () => {
    renderDialog({ deliveries: [] });

    expect(screen.getByText('pod.noDeliveries')).toBeInTheDocument();
  });
});
