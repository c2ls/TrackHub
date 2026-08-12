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

// Rendering behaviour of the public status page and the signed-in shell banner:
// the page must render with zero backends reachable, the admin tier must stay
// invisible to non-administrators, and shell announcements must dismiss per session.

import { afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import type { ReactNode } from 'react';

import enTranslations from 'locales/en.json';
import { TestWrapper } from '../components/testHelpers';

const mockIsAdmin = vi.fn();
const mockIsManager = vi.fn();
const mockIsAuthenticated = vi.fn();
const mockGetVisibleAnnouncements = vi.fn();
const mockGetBackgroundJobStatus = vi.fn();
const mockGetPlatformSyncActivity = vi.fn();
const mockGetPlatformAnnouncements = vi.fn();

// endpoints.ts resolves REACT_APP_* URLs from the runtime env the dev/build
// pipeline injects; a static map makes every backend "configured" so the total
// outage case is actually exercised (an unset backend probes as "unknown").
vi.mock('api/core/endpoints', () => ({
  HEALTH_ENDPOINTS: {
    authority: 'https://example.test/Identity/health',
    security: 'https://example.test/Security/health',
    manager: 'https://example.test/Manager/health',
    router: 'https://example.test/Router/health',
    telemetry: 'https://example.test/Telemetry/health',
    geofencing: 'https://example.test/Geofence/health',
    reporting: 'https://example.test/Reporting/health',
  },
  GRAPHQL_ENDPOINTS: {},
  REST_ENDPOINTS: { managerPlatformAnnouncements: 'https://example.test/Manager/api/PlatformStatus/announcements' },
}));

vi.mock('api/security/users', () => ({ isAdmin: () => mockIsAdmin(), isManager: () => mockIsManager() }));
vi.mock('AuthContext', () => ({ useAuth: () => ({ isAuthenticated: mockIsAuthenticated() }) }));
vi.mock('api/manager/platformStatus', () => ({
  getVisibleAnnouncements: () => mockGetVisibleAnnouncements(),
  getBackgroundJobStatus: () => mockGetBackgroundJobStatus(),
  getPlatformAnnouncements: () => mockGetPlatformAnnouncements(),
  createPlatformAnnouncement: vi.fn(),
  updatePlatformAnnouncement: vi.fn(),
  deletePlatformAnnouncement: vi.fn(),
}));
vi.mock('api/telemetry/platformStatus', async () => {
  const actual = await vi.importActual<typeof import('api/telemetry/platformStatus')>(
    'api/telemetry/platformStatus'
  );
  return { ...actual, getPlatformSyncActivity: () => mockGetPlatformSyncActivity() };
});

const renderWithProviders = (ui: ReactNode) => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 } },
  });
  return render(
    <TestWrapper>
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>{ui}</MemoryRouter>
      </QueryClientProvider>
    </TestWrapper>
  );
};

beforeAll(async () => {
  await i18n.use(initReactI18next).init({
    lng: 'en',
    fallbackLng: 'en',
    resources: { en: { translation: enTranslations } },
    interpolation: { escapeValue: false },
  });
});

beforeEach(() => {
  // Every backend refuses the probe: the hardest case the page must survive.
  global.fetch = vi.fn(async () => {
    throw new TypeError('Failed to fetch');
  }) as unknown as typeof fetch;
  mockIsManager.mockResolvedValue(false);
  mockGetVisibleAnnouncements.mockResolvedValue([]);
  mockGetBackgroundJobStatus.mockResolvedValue([]);
  mockGetPlatformAnnouncements.mockResolvedValue([]);
  mockGetPlatformSyncActivity.mockResolvedValue({
    lastSyncRunAt: null,
    lastHealthCheckAt: null,
    syncRunsLastHour: 0,
    healthChecksLastHour: 0,
    hasEnabledGpsIntegration: false,
  });
  sessionStorage.clear();
});

afterEach(() => {
  vi.clearAllMocks();
});

describe('/status with zero backends reachable', () => {
  it('still renders the page and marks every service as not working', async () => {
    mockIsAuthenticated.mockReturnValue(false);
    mockIsAdmin.mockResolvedValue(false);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    expect(screen.getByText(enTranslations.platformStatus.title)).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByText(enTranslations.platformStatus.overall.down)).toBeInTheDocument();
    });
    // Seven tiles, all down — a total outage must not blank the page.
    await waitFor(() => {
      const tiles = screen.getAllByTestId('service-tile');
      expect(tiles).toHaveLength(7);
      expect(tiles.every((tile) => tile.getAttribute('data-state') === 'down')).toBe(true);
    });
  });

  it('tells signed-out visitors they can bookmark it', async () => {
    mockIsAuthenticated.mockReturnValue(false);
    mockIsAdmin.mockResolvedValue(false);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    expect(screen.getByText(enTranslations.platformStatus.bookmarkHint)).toBeInTheDocument();
  });
});

describe('tier gating', () => {
  it('hides the admin tier from a signed-out visitor', async () => {
    mockIsAuthenticated.mockReturnValue(false);
    mockIsAdmin.mockResolvedValue(false);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    await waitFor(() => expect(screen.getAllByTestId('service-tile').length).toBeGreaterThan(0));
    expect(screen.queryByText(enTranslations.platformStatus.jobs.title)).not.toBeInTheDocument();
    expect(screen.queryByText(enTranslations.platformStatus.manage.open)).not.toBeInTheDocument();
    expect(mockGetBackgroundJobStatus).not.toHaveBeenCalled();
  });

  it('hides everything above the public tier from a plain signed-in user', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockIsAdmin.mockResolvedValue(false);
    mockIsManager.mockResolvedValue(false);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    await waitFor(() => expect(mockIsAdmin).toHaveBeenCalled());
    expect(screen.queryByText(enTranslations.platformStatus.jobs.title)).not.toBeInTheDocument();
    expect(screen.queryByText(enTranslations.platformStatus.services.syncWorker.name)).not.toBeInTheDocument();
    // Neither privileged query is even issued.
    expect(mockGetBackgroundJobStatus).not.toHaveBeenCalled();
    expect(mockGetPlatformSyncActivity).not.toHaveBeenCalled();
  });

  it('shows a MANAGER the GPS synchronisation tile but not the administrator internals', async () => {
    // Sergio, 2026-07-19: Managers see the sync tile (it pairs with the gpsIntegration dashboard
    // they own); background jobs and announcement management stay SuperAdministrator-only.
    mockIsAuthenticated.mockReturnValue(true);
    mockIsAdmin.mockResolvedValue(false);
    mockIsManager.mockResolvedValue(true);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    await waitFor(() => {
      expect(screen.getByText(enTranslations.platformStatus.services.syncWorker.name)).toBeInTheDocument();
    });
    expect(mockGetPlatformSyncActivity).toHaveBeenCalled();

    // The administrator-only surfaces stay invisible AND unqueried.
    expect(screen.queryByText(enTranslations.platformStatus.jobs.title)).not.toBeInTheDocument();
    expect(screen.queryByText(enTranslations.platformStatus.manage.open)).not.toBeInTheDocument();
    expect(mockGetBackgroundJobStatus).not.toHaveBeenCalled();
    expect(mockGetPlatformAnnouncements).not.toHaveBeenCalled();
  });

  it('shows the full admin tier to an administrator', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockIsAdmin.mockResolvedValue(true);
    mockIsManager.mockResolvedValue(false);
    const PlatformStatus = (await import('layouts/platformstatus')).default;

    renderWithProviders(<PlatformStatus />);

    await waitFor(() => {
      expect(screen.getByText(enTranslations.platformStatus.jobs.title)).toBeInTheDocument();
    });
    expect(screen.getByText(enTranslations.platformStatus.manage.open)).toBeInTheDocument();
    expect(screen.getByText(enTranslations.platformStatus.services.syncWorker.name)).toBeInTheDocument();
  });
});

describe('shell announcement banner', () => {
  const announcement = {
    id: 'a1',
    messageEn: 'Scheduled maintenance tonight',
    messageEs: null,
    severity: 'WARNING' as const,
    startsAt: null,
    endsAt: null,
  };

  it('renders a visible announcement and dismisses it for the session', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockGetVisibleAnnouncements.mockResolvedValue([announcement]);
    const AnnouncementBanner = (await import('components/AnnouncementBanner')).default;

    renderWithProviders(<AnnouncementBanner />);

    await waitFor(() => {
      expect(screen.getByText(announcement.messageEn)).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole('button', { name: /close/i }));

    await waitFor(() => {
      expect(screen.queryByText(announcement.messageEn)).not.toBeInTheDocument();
    });
    expect(JSON.parse(sessionStorage.getItem('dismissed_announcements') ?? '[]')).toContain('a1');
  });

  it('stays dismissed on a later mount within the same session', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockGetVisibleAnnouncements.mockResolvedValue([announcement]);
    sessionStorage.setItem('dismissed_announcements', JSON.stringify(['a1']));
    const AnnouncementBanner = (await import('components/AnnouncementBanner')).default;

    renderWithProviders(<AnnouncementBanner />);

    await waitFor(() => expect(mockGetVisibleAnnouncements).toHaveBeenCalled());
    expect(screen.queryByText(announcement.messageEn)).not.toBeInTheDocument();
  });

  it('renders nothing when there are no announcements', async () => {
    mockIsAuthenticated.mockReturnValue(true);
    mockGetVisibleAnnouncements.mockResolvedValue([]);
    const AnnouncementBanner = (await import('components/AnnouncementBanner')).default;

    const { container } = renderWithProviders(<AnnouncementBanner />);

    await waitFor(() => expect(mockGetVisibleAnnouncements).toHaveBeenCalled());
    expect(container.querySelectorAll('[data-testid="shell-announcement"]')).toHaveLength(0);
  });
});
