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
 * A `styleOverrides` key that no longer matches a Material UI slot is silently ignored —
 * no error, no warning, the styling just stops applying. The theme's `components` object
 * is cast to `ThemeOptions["components"]`, so the compiler does not catch it either. That
 * is how the Tabs indicator came to paint over the selected tab's label: MUI renamed
 * `flexContainer` to `list` and the z-index lift went dead.
 *
 * This pins every slot the theme targets against MUI's real class list. On a MUI upgrade,
 * a renamed slot fails here instead of quietly losing styling somewhere in the UI.
 */

import { render } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import { Tabs, Tab } from '@mui/material';
// The barrel re-exports every `<component>Classes` map; a per-component dynamic import
// would be a template specifier Vite cannot statically resolve.
import * as mui from '@mui/material';
import theme from 'assets/theme';
import themeDark from 'assets/theme-dark';

/** styleOverrides here are global CSS selectors, not component slots. */
const GLOBAL_CSS_COMPONENTS = new Set(['MuiCssBaseline']);

/**
 * Slots already stale when this guard was written (Material UI v9). Each is a real
 * dead override — the Argon styling it carries does not reach the DOM — but fixing them
 * changes how components look and needs eyeballing, so they are recorded rather than
 * hidden. Shrink this list; never grow it.
 */
const KNOWN_STALE = new Set([
  // v9 replaced the variant+color / variant+size composite classes with `variants`.
  'MuiButton.containedSizeSmall',
  'MuiButton.containedSizeLarge',
  'MuiButton.containedPrimary',
  'MuiButton.containedSecondary',
  'MuiButton.outlinedSizeSmall',
  'MuiButton.outlinedSizeLarge',
  'MuiButton.outlinedPrimary',
  'MuiButton.outlinedSecondary',
  'MuiButton.textSizeSmall',
  'MuiButton.textSizeLarge',
  'MuiButton.textPrimary',
  'MuiButton.textSecondary',
  // Now composed from `docked` + `anchorLeft`.
  'MuiDrawer.paperAnchorDockedLeft',
  // Removed by MUI; the slot is just `select`.
  'MuiSelect.selectMenu',
  // Now `sizeSmall` on the root rather than a composite input class.
  'MuiInput.inputSizeSmall',
  'MuiOutlinedInput.inputSizeSmall',
  'MuiFilledInput.inputSizeSmall',
]);

const lowerFirst = (value: string) => value.charAt(0).toLowerCase() + value.slice(1);

function staleSlots(components: Record<string, unknown> | undefined): string[] {
  const registry = mui as unknown as Record<string, Record<string, string> | undefined>;
  const stale: string[] = [];

  for (const [muiName, config] of Object.entries(components ?? {})) {
    if (GLOBAL_CSS_COMPONENTS.has(muiName)) continue;

    const overrides = (config as { styleOverrides?: Record<string, unknown> })?.styleOverrides;
    if (!overrides) continue;

    const classes = registry[`${lowerFirst(muiName.replace(/^Mui/, ''))}Classes`];
    expect(classes, `${muiName} exposes no classes map`).toBeDefined();

    for (const slot of Object.keys(overrides)) {
      if (!(slot in classes!)) stale.push(`${muiName}.${slot}`);
    }
  }

  return stale;
}

describe('theme styleOverrides slot names', () => {
  test.each([
    ['light', theme],
    ['dark', themeDark],
  ])('%s theme targets slots that still exist in Material UI', (_name, activeTheme) => {
    const stale = staleSlots(activeTheme.components as Record<string, unknown>);
    const unexpected = stale.filter((slot) => !KNOWN_STALE.has(slot));

    expect(unexpected).toEqual([]);
  });

  test('the tab list is lifted above the indicator that would otherwise cover it', () => {
    const { container } = render(
      <ThemeProvider theme={themeDark}>
        <Tabs value={0} onChange={() => {}}>
          <Tab label="Units" />
          <Tab label="Positions" />
        </Tabs>
      </ThemeProvider>
    );

    const list = container.querySelector('.MuiTabs-list');
    expect(list, 'MUI renamed the tab list slot again').not.toBeNull();
    // Without these the full-height indicator paints over the selected label.
    expect(getComputedStyle(list!).zIndex).toBe('10');
    expect(getComputedStyle(list!).position).toBe('relative');
  });
});
