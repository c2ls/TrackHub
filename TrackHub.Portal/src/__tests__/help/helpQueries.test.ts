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

// SPA-fallback detection + EN fallback for the help fetch layer: nginx serves
// index.html with HTTP 200 for missing static files, so a miss is detected by
// content, never by status.

import { afterEach, describe, expect, it, vi } from 'vitest';
import {
  fetchHelpManifest,
  fetchHelpTopic,
  normalizeHelpLanguage,
  stripFrontmatter,
} from 'queries/help';

const SPA_FALLBACK = '<!doctype html><html><head></head><body>app</body></html>';
const TOPIC_MD = '---\nid: geofences\ntitle: Geofences\n---\n\n# Geofences\n\nBody.';

function mockFetch(responses: Record<string, { body: string; contentType?: string; status?: number }>) {
  return vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = String(input);
    const key = Object.keys(responses).find((candidate) => url.startsWith(candidate));
    if (!key) throw new Error(`Unexpected fetch: ${url}`);
    const { body, contentType = 'text/markdown', status = 200 } = responses[key];
    return new Response(body, { status, headers: { 'content-type': contentType } });
  });
}

afterEach(() => vi.restoreAllMocks());

describe('normalizeHelpLanguage', () => {
  it('maps regional tags to the base language', () => {
    expect(normalizeHelpLanguage('es-CO', ['en', 'es'])).toBe('es');
  });
  it('falls back to en for unsupported languages', () => {
    expect(normalizeHelpLanguage('fr', ['en', 'es'])).toBe('en');
    expect(normalizeHelpLanguage('', ['en', 'es'])).toBe('en');
  });
});

describe('stripFrontmatter', () => {
  it('removes the leading YAML block only', () => {
    expect(stripFrontmatter(TOPIC_MD)).toBe('\n# Geofences\n\nBody.');
  });
  it('leaves bodies without frontmatter untouched', () => {
    expect(stripFrontmatter('# Plain')).toBe('# Plain');
  });
});

describe('fetchHelpTopic', () => {
  it('returns the requested language when present', async () => {
    const spy = mockFetch({ '/help/es/geofences.md': { body: TOPIC_MD } });
    const result = await fetchHelpTopic('es', { id: 'geofences', hash: 'abc123' });
    expect(result).toEqual({ markdown: '\n# Geofences\n\nBody.', resolvedLang: 'es' });
    expect(spy).toHaveBeenCalledWith('/help/es/geofences.md?v=abc123');
  });

  it('detects the SPA index.html fallback and retries in English', async () => {
    mockFetch({
      '/help/es/geofences.md': { body: SPA_FALLBACK, contentType: 'text/html' },
      '/help/en/geofences.md': { body: TOPIC_MD },
    });
    const result = await fetchHelpTopic('es', { id: 'geofences', hash: 'abc123' });
    expect(result.resolvedLang).toBe('en');
    expect(result.markdown).toContain('# Geofences');
  });

  it('detects an HTML body even without a text/html content type', async () => {
    mockFetch({
      '/help/es/geofences.md': { body: `  ${SPA_FALLBACK}`, contentType: 'text/markdown' },
      '/help/en/geofences.md': { body: TOPIC_MD },
    });
    const result = await fetchHelpTopic('es', { id: 'geofences', hash: 'abc123' });
    expect(result.resolvedLang).toBe('en');
  });

  it('resolves with null markdown when the topic is missing in every language', async () => {
    mockFetch({
      '/help/es/geofences.md': { body: SPA_FALLBACK, contentType: 'text/html' },
      '/help/en/geofences.md': { body: 'nope', status: 404 },
    });
    const result = await fetchHelpTopic('es', { id: 'geofences', hash: 'abc123' });
    expect(result).toEqual({ markdown: null, resolvedLang: 'es' });
  });
});

describe('fetchHelpManifest', () => {
  it('parses the manifest', async () => {
    mockFetch({
      '/help/manifest.json': {
        body: JSON.stringify({ version: 'v', languages: ['en'], categories: [], topics: [] }),
        contentType: 'application/json',
      },
    });
    await expect(fetchHelpManifest()).resolves.toMatchObject({ version: 'v' });
  });

  it('throws when the SPA fallback answers instead', async () => {
    mockFetch({ '/help/manifest.json': { body: SPA_FALLBACK, contentType: 'text/html' } });
    await expect(fetchHelpManifest()).rejects.toThrow('Help manifest is not available');
  });
});
