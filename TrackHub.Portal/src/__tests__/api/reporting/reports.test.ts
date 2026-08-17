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

import { downloadFile, restRequest } from 'api/core/restClient';
import { downloadReport, previewReport } from 'api/reporting/reports';
import type { ReportPreview } from 'api/reporting/reports';

vi.mock('i18next', () => ({ default: { language: 'en' } }));
vi.mock('api/core/restClient', () => ({
  downloadFile: vi.fn(),
  restRequest: vi.fn(),
}));
vi.mock('api/core/endpoints', () => ({
  REST_ENDPOINTS: {
    reportingBasicReports: 'https://reporting/api/BasicReports',
    reportingBasicReportsPreview: 'https://reporting/api/BasicReports/preview',
  },
}));

describe('downloadReport', () => {
  beforeEach(() => vi.clearAllMocks());

  test('defaults to xlsx and streams a .xlsx filename', async () => {
    await downloadReport('LiveReport', 'Units Report', {});

    const [url, body, filename] = vi.mocked(downloadFile).mock.calls[0];
    expect(url).toBe('https://reporting/api/BasicReports');
    expect(filename).toBe('Units Report.xlsx');
    expect((body as { format: string }).format).toBe('xlsx');
  });

  test('sends format=pdf and a .pdf filename when requested', async () => {
    await downloadReport('accounts-by-status', 'Accounts by Status', {}, 'pdf');

    const [, body, filename] = vi.mocked(downloadFile).mock.calls[0];
    expect(filename).toBe('Accounts by Status.pdf');
    expect((body as { format: string }).format).toBe('pdf');
  });

  test('maps form values onto the FilterDto shape with numeric coercion', async () => {
    await downloadReport(
      'gps.position-history',
      'GPS Position History',
      { selectedItem1: 't-1', selectedDate1: '2026-07-01T00:00', selectedNumber1: '250' },
      'xlsx'
    );

    const [, body] = vi.mocked(downloadFile).mock.calls[0];
    const filters = (body as { filters: Record<string, unknown> }).filters;
    expect(filters.name).toBe('GPS Position History');
    expect(filters.stringFilter1).toBe('t-1');
    expect(filters.dateTimeFilter1).not.toBeNull();
    // NumericFilter1 is a `double?` server-side — must be a real number, not a string.
    expect(filters.numericFilter1).toBe(250);
    expect(typeof filters.numericFilter1).toBe('number');
    expect(filters.numericFilter2).toBeNull();
    expect(filters.language).toBe('en');
  });
});

describe('previewReport', () => {
  beforeEach(() => vi.clearAllMocks());

  test('POSTs to the preview endpoint and returns the parsed dataset', async () => {
    const payload: ReportPreview = {
      columns: [{ name: 'status', header: 'Status' }],
      rows: [['ACTIVE']],
      totalRows: 1,
      truncated: false,
    };
    vi.mocked(restRequest).mockResolvedValue(payload);

    const result = await previewReport('accounts-by-status', 'Accounts by Status', {
      selectedItem1: 'ACTIVE',
    });

    expect(result).toBe(payload);
    const config = vi.mocked(restRequest).mock.calls[0][0];
    expect(config.method).toBe('POST');
    expect(config.url).toBe('https://reporting/api/BasicReports/preview');
    const data = config.data as { reportCode: string; filters: Record<string, unknown> };
    expect(data.reportCode).toBe('accounts-by-status');
    expect(data.filters.stringFilter1).toBe('ACTIVE');
    // Preview body carries no `format` field.
    expect(data).not.toHaveProperty('format');
  });
});
