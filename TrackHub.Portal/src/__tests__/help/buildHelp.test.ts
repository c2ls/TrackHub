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

// Validator-rule coverage for scripts/build-help.mjs: every rule
// gets a broken fixture; a valid fixture proves the happy path and manifest.

import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { buildManifest, extractRouteKeys, validateHelpContent } from '../../../scripts/build-help.mjs';

const ROUTES_SOURCE = `
const routes = [
  { type: "route", name: "screen.dashboard", key: "dashboard", route: "/dashboard" },
  { type: "route", name: "screen.reports", key: "reports", route: "/reports" },
  { type: "title", title: "screen.account", key: "account-pages" },
  { type: "hidden", name: "Callback", key: "callback", route: "/authentication/callback" },
];
`;

let root: string;
let helpDir: string;
let routesFile: string;

function writeTopic(lang: string, id: string, frontmatter: Record<string, unknown>, body: string) {
  const yaml = Object.entries(frontmatter)
    .map(([key, value]) => `${key}: ${JSON.stringify(value)}`)
    .join('\n');
  fs.mkdirSync(path.join(helpDir, lang), { recursive: true });
  fs.writeFileSync(path.join(helpDir, lang, `${id}.md`), `---\n${yaml}\n---\n\n${body}`);
}

const baseTopic = (overrides: Record<string, unknown> = {}) => ({
  id: 'dashboard-live-map',
  title: 'Live map',
  description: 'Watch your fleet.',
  category: 'operation',
  screens: ['dashboard'],
  related: [],
  tags: ['map'],
  order: 10,
  ...overrides,
});

/** Valid two-topic, two-language corpus covering both route keys. */
function writeValidCorpus() {
  for (const lang of ['en', 'es']) {
    writeTopic(lang, 'dashboard-live-map', baseTopic({ related: ['reports'] }), '# Live map\n\nBody.');
    writeTopic(
      lang,
      'reports',
      baseTopic({ id: 'reports', title: 'Reports', screens: ['reports'], order: 20 }),
      '# Reports\n\nSee [Live map](topic:dashboard-live-map#usage).\n\n## Usage\n\nMore.'
    );
  }
}

function errorsOf() {
  return validateHelpContent({ helpDir, routesFile }).errors.map((error) => error.message);
}

beforeEach(() => {
  root = fs.mkdtempSync(path.join(os.tmpdir(), 'help-test-'));
  helpDir = path.join(root, 'help');
  fs.mkdirSync(helpDir, { recursive: true });
  routesFile = path.join(root, 'routes.tsx');
  fs.writeFileSync(routesFile, ROUTES_SOURCE);
});

afterEach(() => {
  fs.rmSync(root, { recursive: true, force: true });
});

describe('extractRouteKeys', () => {
  it('returns visible route keys only (no hidden/title entries)', () => {
    expect(extractRouteKeys(ROUTES_SOURCE)).toEqual(['dashboard', 'reports']);
  });
});

describe('validateHelpContent', () => {
  it('accepts a valid corpus', () => {
    writeValidCorpus();
    expect(errorsOf()).toEqual([]);
  });

  it('fails when a translation is missing', () => {
    writeValidCorpus();
    fs.rmSync(path.join(helpDir, 'es', 'reports.md'));
    expect(errorsOf()).toEqual([expect.stringContaining("missing in language 'es'")]);
  });

  it('fails on a topic: link to a nonexistent topic', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(
        lang,
        'dashboard-live-map',
        baseTopic(),
        '# Live map\n\nSee [ghost](topic:nope).'
      );
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("link target 'nope' does not exist")])
    );
  });

  it('fails on an unknown screen key', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic({ screens: ['nope'] }), '# Live map\n\nBody.');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("screen 'nope' is not a route key")])
    );
  });

  it('fails when a route key has no covering topic', () => {
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic(), '# Live map\n\nBody.');
    }
    expect(errorsOf()).toEqual([expect.stringContaining("route key 'reports' has no help topic")]);
  });

  it('fails on raw HTML in the body', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic(), '# Live map\n\nLine<br>break.');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining('raw HTML is not allowed')])
    );
  });

  it('allows autolinks and code spans containing angle brackets', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(
        lang,
        'dashboard-live-map',
        baseTopic(),
        '# Live map\n\nVisit <https://example.com> and use `<placeholder>` in code.\n\n```\n<div>fenced html is fine</div>\n```'
      );
    }
    expect(errorsOf()).toEqual([]);
  });

  it('fails on relative .md links', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic(), '# Live map\n\nSee [old](11-trips.md).');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("relative .md link '11-trips.md'")])
    );
  });

  it('fails when the frontmatter id does not match the filename', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic({ id: 'other-id' }), '# Live map\n\nBody.');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("id 'other-id' does not match filename")])
    );
  });

  it('fails on a referenced image that does not exist', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic(), '# Live map\n\n![shot](assets/nope.png)');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("image file 'assets/nope.png' does not exist")])
    );
  });

  it('fails when the H1 does not match the title', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic(), '# Something else\n\nBody.');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("must equal frontmatter title")])
    );
  });

  it('fails when structural frontmatter differs across languages', () => {
    writeValidCorpus();
    writeTopic('es', 'dashboard-live-map', baseTopic({ order: 99, related: ['reports'] }), '# Live map\n\nBody.');
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("'order' differs from the en file")])
    );
  });

  it('fails on a related id that does not exist', () => {
    writeValidCorpus();
    for (const lang of ['en', 'es']) {
      writeTopic(lang, 'dashboard-live-map', baseTopic({ related: ['ghost'] }), '# Live map\n\nBody.');
    }
    expect(errorsOf()).toEqual(
      expect.arrayContaining([expect.stringContaining("related topic 'ghost' does not exist")])
    );
  });
});

describe('buildManifest', () => {
  it('emits sorted topics with hashes and localized metadata', () => {
    writeValidCorpus();
    const result = validateHelpContent({ helpDir, routesFile });
    expect(result.errors).toEqual([]);
    const manifest = buildManifest(result);

    expect(manifest.languages).toEqual(['en', 'es']);
    expect(manifest.categories).toEqual([{ id: 'operation', order: 2 }]);
    expect(manifest.topics.map((topic) => topic.id)).toEqual(['dashboard-live-map', 'reports']);
    expect(manifest.version).toMatch(/^[0-9a-f]{12}$/);
    for (const topic of manifest.topics) {
      expect(topic.hash).toMatch(/^[0-9a-f]{12}$/);
      expect(topic.i18n.en.title).toBeTruthy();
      expect(topic.i18n.es.title).toBeTruthy();
    }
  });

  it('changes the topic hash when any language content changes', () => {
    writeValidCorpus();
    const before = buildManifest(validateHelpContent({ helpDir, routesFile }));
    writeTopic('es', 'reports', baseTopic({ id: 'reports', title: 'Reports', screens: ['reports'], order: 20 }), '# Reports\n\nCambiado.');
    const after = buildManifest(validateHelpContent({ helpDir, routesFile }));
    const hashOf = (manifest: typeof before, id: string) =>
      manifest.topics.find((topic) => topic.id === id)?.hash;
    expect(hashOf(after, 'reports')).not.toEqual(hashOf(before, 'reports'));
    expect(hashOf(after, 'dashboard-live-map')).toEqual(hashOf(before, 'dashboard-live-map'));
    expect(after.version).not.toEqual(before.version);
  });
});
