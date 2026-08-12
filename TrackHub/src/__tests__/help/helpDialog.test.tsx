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

// End-to-end behavior of the help modal: route→primary-topic resolution,
// topic: link navigation with back, language selection, and F1.

import { afterEach, beforeAll, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

import enTranslations from 'locales/en.json';
import esTranslations from 'locales/es.json';
import HelpProvider, { useHelp } from 'context/help';

const MANIFEST = {
  version: 'v1',
  languages: ['en', 'es'],
  categories: [
    { id: 'operation', order: 2 },
    { id: 'administration', order: 3 },
  ],
  topics: [
    {
      id: 'dashboard-live-map',
      category: 'operation',
      order: 10,
      screens: ['dashboard'],
      related: [],
      featureKey: null,
      hash: 'aaa',
      i18n: {
        en: { title: 'Live map', description: 'Fleet in real time.', tags: ['map'] },
        es: { title: 'Mapa en vivo', description: 'Flota en tiempo real.', tags: ['mapa'] },
      },
    },
    {
      id: 'reports',
      category: 'operation',
      order: 40,
      screens: ['reports'],
      related: [],
      featureKey: null,
      hash: 'bbb',
      i18n: {
        en: { title: 'Reports', description: 'Generate reports.', tags: ['excel'] },
        es: { title: 'Informes', description: 'Genera informes.', tags: ['excel'] },
      },
    },
  ],
};

const TOPICS: Record<string, string> = {
  'en/dashboard-live-map': '# Live map\n\nWatch units. See [Reports](topic:reports).',
  'en/reports': '# Reports\n\nReport content here.',
  'es/dashboard-live-map': '# Mapa en vivo\n\nObserva unidades. Ve [Informes](topic:reports).',
  'es/reports': '# Informes\n\nContenido de informes.',
};

function mockHelpFetch() {
  return vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = String(input);
    if (url.startsWith('/help/manifest.json')) {
      return new Response(JSON.stringify(MANIFEST), {
        headers: { 'content-type': 'application/json' },
      });
    }
    const match = /^\/help\/(en|es)\/([a-z0-9-]+)\.md/.exec(url);
    const body = match && TOPICS[`${match[1]}/${match[2]}`];
    if (body) {
      return new Response(`---\nid: ${match![2]}\n---\n\n${body}`, {
        headers: { 'content-type': 'text/markdown' },
      });
    }
    return new Response('<!doctype html><html></html>', {
      headers: { 'content-type': 'text/html' },
    });
  });
}

function OpenButton() {
  const { openHelp } = useHelp();
  return (
    <button type="button" onClick={() => openHelp()}>
      open-help
    </button>
  );
}

function renderShell(initialPath = '/dashboard') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <QueryClientProvider client={queryClient}>
        <HelpProvider allowedScreens={['dashboard', 'reports']} isFeatureEnabled={() => true}>
          <OpenButton />
        </HelpProvider>
      </QueryClientProvider>
    </MemoryRouter>
  );
}

beforeAll(async () => {
  await i18n.use(initReactI18next).init({
    lng: 'en',
    fallbackLng: 'en',
    resources: {
      en: { translation: enTranslations },
      es: { translation: esTranslations },
    },
    interpolation: { escapeValue: false },
  });
});

afterEach(async () => {
  vi.restoreAllMocks();
  await i18n.changeLanguage('en');
});

describe('HelpDialog', () => {
  it('opens on the current screen primary topic and navigates topic: links with back', async () => {
    mockHelpFetch();
    renderShell('/dashboard');

    fireEvent.click(screen.getByText('open-help'));
    expect(await screen.findByText('Watch units.', { exact: false })).toBeInTheDocument();

    // topic: link navigates inside the modal…
    fireEvent.click(screen.getByRole('button', { name: 'Reports' }));
    expect(await screen.findByText('Report content here.')).toBeInTheDocument();

    // …and back pops the navigation stack.
    fireEvent.click(screen.getByLabelText('Back'));
    expect(await screen.findByText('Watch units.', { exact: false })).toBeInTheDocument();
  });

  it('opens the index view on screens without a topic', async () => {
    mockHelpFetch();
    renderShell('/profile');

    fireEvent.click(screen.getByText('open-help'));
    // Index view: search box plus the topic list.
    expect(await screen.findByPlaceholderText('Search help topics...')).toBeInTheDocument();
    expect(screen.getByText('Live map')).toBeInTheDocument();
  });

  it('serves Spanish content when the active language is es', async () => {
    const fetchSpy = mockHelpFetch();
    await i18n.changeLanguage('es');
    renderShell('/dashboard');

    fireEvent.click(screen.getByText('open-help'));
    expect(await screen.findByText('Observa unidades.', { exact: false })).toBeInTheDocument();
    expect(fetchSpy).toHaveBeenCalledWith('/help/es/dashboard-live-map.md?v=aaa');
  });

  it('opens with F1', async () => {
    mockHelpFetch();
    renderShell('/dashboard');

    fireEvent.keyDown(window, { key: 'F1' });
    expect(await screen.findByText('Watch units.', { exact: false })).toBeInTheDocument();
  });

  it('filters the index by search in the active language', async () => {
    mockHelpFetch();
    renderShell('/profile');

    fireEvent.click(screen.getByText('open-help'));
    const search = await screen.findByPlaceholderText('Search help topics...');
    fireEvent.change(search, { target: { value: 'excel' } });
    expect(screen.getByText('Reports')).toBeInTheDocument();
    expect(screen.queryByText('Live map')).not.toBeInTheDocument();
  });
});
