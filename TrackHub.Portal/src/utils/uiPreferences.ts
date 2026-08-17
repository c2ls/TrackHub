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
 * Last-known shell preferences (theme, sidenav, language), mirrored into localStorage.
 *
 * The authoritative copy lives in the Manager backend (`userSettings`), but that read
 * only lands a second or more after the shell has already painted. Seeding the Argon
 * controller from this mirror lets the first frame use the theme the user actually
 * chose, instead of showing a light shell that flips to dark once the query resolves.
 *
 * The mirror is deliberately not cleared on sign-out: on a shared browser the next
 * user briefly sees the previous user's theme before their own settings land, which is
 * exactly what every user sees today anyway, whereas clearing it would bring the flash
 * back for the far more common sign-out/sign-in-again case.
 *
 * `index.html` reads the same key inline, before the bundle loads — keep the key and
 * the `darkMode` field name in sync with that snippet.
 */

const STORAGE_KEY = 'trackhub.ui-preferences';

/** Shape persisted under {@link STORAGE_KEY}. */
export interface UiPreferences {
  darkMode: boolean;
  miniSidenav: boolean;
  language?: string;
}

/** Subset of `UserSettings` the shell reads; kept loose so callers can pass partials. */
interface UserStyleSettings {
  style?: string | null;
  navbar?: string | null;
  language?: string | null;
}

/** Backend stores the style as a free-form string; anything but `light` is dark. */
export const isDarkStyle = (style?: string | null): boolean => style !== 'light';

/** Backend stores the navbar mode as a free-form string; anything but `none` collapses it. */
export const isMiniSidenav = (navbar?: string | null): boolean => navbar !== 'none';

export function toUiPreferences(settings: UserStyleSettings): UiPreferences {
  return {
    darkMode: isDarkStyle(settings.style),
    miniSidenav: isMiniSidenav(settings.navbar),
    language: settings.language ?? undefined,
  };
}

/**
 * Returns the mirrored preferences, or null when nothing usable is stored — callers
 * then fall back to the light defaults rather than guessing.
 */
export function readUiPreferences(): UiPreferences | null {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const parsed: unknown = JSON.parse(raw);
    if (!parsed || typeof parsed !== 'object') return null;
    const { darkMode, miniSidenav, language } = parsed as Partial<UiPreferences>;
    if (typeof darkMode !== 'boolean' || typeof miniSidenav !== 'boolean') return null;
    return { darkMode, miniSidenav, language: typeof language === 'string' ? language : undefined };
  } catch {
    // Storage disabled (private mode, blocked cookies) or corrupt payload.
    return null;
  }
}

export function writeUiPreferences(settings: UserStyleSettings): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(toUiPreferences(settings)));
  } catch {
    // Storage disabled — the shell just keeps flashing on first paint, nothing worse.
  }
}
