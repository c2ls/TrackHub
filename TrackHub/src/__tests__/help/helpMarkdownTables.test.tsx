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

import { render } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { ThemeProvider } from '@mui/material/styles';
import Table from '@mui/material/Table';
import TableHead from '@mui/material/TableHead';
import theme from 'assets/theme';
import themeDark from 'assets/theme-dark';
import { buildMarkdownComponents } from 'controls/Help/markdown';

const markdown = `| Tile | Centred | What it covers |
|---|:---:|---|
| Sign-in | Yes | Logging in to TrackHub. |
`;

/**
 * jsdom's getComputedStyle does not run the emotion cascade — it reports the UA
 * default for `display` whatever the stylesheet says, so it cannot tell these
 * cases apart. Read the emitted rules for the element's own classes instead.
 */
function emittedCssFor(element: Element): string {
  const classes = [...element.classList];
  return [...document.querySelectorAll('style')]
    .map((style) => style.textContent ?? '')
    .join('\n')
    .split('}')
    .filter((rule) => classes.some((className) => rule.includes(`.${className}`)))
    .join('}');
}

/**
 * The winning value of a property: emotion emits every declaration in the
 * cascade into one rule (MUI default, then theme override, then sx), so the
 * last occurrence is the one that actually applies.
 */
function effectiveValue(css: string, property: string): string | undefined {
  const matches = [...css.matchAll(new RegExp(`${property}:([^;}]+)`, 'g'))];
  return matches.at(-1)?.[1].trim();
}

function renderTopic(appTheme: typeof theme) {
  return render(
    <ThemeProvider theme={appTheme}>
      <ReactMarkdown remarkPlugins={[remarkGfm]} skipHtml components={buildMarkdownComponents(() => {})}>
        {markdown}
      </ReactMarkdown>
    </ThemeProvider>
  );
}

describe('help markdown tables', () => {
  // Guards the reason markdown.tsx overrides display on thead. If this stops
  // being block, that override is redundant and its comment is stale.
  it.each([
    ['light', theme],
    ['dark', themeDark],
  ])('the %s app theme forces display:block on an unstyled MuiTableHead', (_name, appTheme) => {
    const { container } = render(
      <ThemeProvider theme={appTheme}>
        <Table>
          <TableHead>
            <tr>
              <th>x</th>
            </tr>
          </TableHead>
        </Table>
      </ThemeProvider>
    );
    expect(effectiveValue(emittedCssFor(container.querySelector('thead')!), 'display')).toBe(
      'block'
    );
  });

  it.each([
    ['light', theme],
    ['dark', themeDark],
  ])('keeps the help table header in the table layout model under the %s theme', (_name, appTheme) => {
    const { container } = renderTopic(appTheme);
    const css = emittedCssFor(container.querySelector('thead')!);
    // display:block drops the header out of the column grid, so its cells
    // shrink-wrap at the left while the body spans the full width. The theme's
    // declaration is still in the rule; what matters is which one wins.
    expect(effectiveValue(css, 'display')).toBe('table-header-group');
  });

  it('honours the column alignment from the :---: markers', () => {
    const { container } = renderTopic(theme);
    const centredHeader = [...container.querySelectorAll('th')].find(
      (cell) => cell.textContent === 'Centred'
    );
    expect(centredHeader?.style.textAlign).toBe('center');
    expect((container.querySelectorAll('td')[1] as HTMLElement).style.textAlign).toBe('center');
    // Unaligned columns must stay untouched rather than defaulting to centre.
    expect((container.querySelectorAll('td')[0] as HTMLElement).style.textAlign).toBe('');
  });
});
