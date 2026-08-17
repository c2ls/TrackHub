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
 * The map overlay pill (`.mapcontrol`, top-right of the dashboard map — the refresh
 * countdown) hardcodes a white background. Its text colour must be pinned too: left to
 * inherit, it follows the page and renders near-white on white under the dark theme,
 * which is how the countdown became invisible. Same failure as the marker-cluster count.
 */

import { render } from '@testing-library/react';
import { ThemeProvider } from '@mui/material/styles';
import themeDark from 'assets/theme-dark';
import MapControlStyle from 'controls/Maps/styles/MapControl';

const renderPill = () =>
  render(
    <ThemeProvider theme={themeDark}>
      <MapControlStyle>
        <div className="mapcontrol">30 s.</div>
      </MapControlStyle>
    </ThemeProvider>
  );

describe('map overlay pill', () => {
  test('pins a foreground colour to go with its hardcoded white background', () => {
    const { container } = renderPill();
    const pill = container.querySelector('.mapcontrol');

    expect(pill).not.toBeNull();
    const { color, background } = getComputedStyle(pill!);
    expect(background).toBe('rgb(255, 255, 255)');
    // Anything inherited would be the dark theme's near-white body colour.
    expect(color).toBe('rgb(51, 51, 51)');
  });
});
