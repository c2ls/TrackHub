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

// Help content validator + manifest generator.
//
// Parses public/help/{lang}/*.md, validates the authoring contract, and emits
// public/help/manifest.json (gitignored). Run modes:
//   node scripts/build-help.mjs           validate + write manifest (predev/prebuild)
//   node scripts/build-help.mjs --check   validate only (CI: npm run help:check)
//
// All violations are collected and reported together (never fail-fast).

import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { pathToFileURL, fileURLToPath } from 'node:url';
import matter from 'gray-matter';

export const CATEGORIES = {
  'getting-started': 1,
  operation: 2,
  administration: 3,
  troubleshooting: 4,
  reference: 5,
};

const REQUIRED_FIELDS = ['id', 'title', 'description', 'category', 'screens', 'order'];

/**
 * Extract route keys from routes.tsx without executing it. Each entry lists
 * `type` before `key`, so a pairwise non-greedy match is sufficient. Returns
 * the keys of user-visible screens (type: "route") only — hidden/title/divider
 * entries need no help topic.
 */
export function extractRouteKeys(routesSource) {
  const keys = [];
  const entryPattern = /type:\s*["'](route|hidden|title|divider|collapse)["'][\s\S]*?key:\s*["']([^"']+)["']/g;
  let match;
  while ((match = entryPattern.exec(routesSource)) !== null) {
    if (match[1] === 'route') {
      keys.push(match[2]);
    }
  }
  return keys;
}

/** Strip fenced code blocks and inline code so their content is never validated as markup. */
function stripCode(body) {
  return body
    .replace(/```[\s\S]*?```/g, '')
    .replace(/~~~[\s\S]*?~~~/g, '')
    .replace(/`[^`\n]*`/g, '');
}

function lineOf(content, index) {
  return content.slice(0, index).split('\n').length;
}

/**
 * Validate all help content under `helpDir` against the route keys of
 * `routesFile`. Returns { errors, languages, topics } where `topics` is the
 * merged per-topic model keyed by id (present even when there are errors, for
 * reporting; only trust it when errors is empty).
 */
export function validateHelpContent({ helpDir, routesFile }) {
  const errors = [];
  const addError = (file, line, message) =>
    errors.push({ file, line, message });

  if (!fs.existsSync(helpDir)) {
    addError(helpDir, null, 'help content directory does not exist');
    return { errors, languages: [], topics: new Map() };
  }

  const routesSource = fs.readFileSync(routesFile, 'utf8');
  const routeKeys = extractRouteKeys(routesSource);
  if (routeKeys.length === 0) {
    addError(routesFile, null, 'no route keys could be extracted from routes.tsx');
  }

  const languages = fs
    .readdirSync(helpDir, { withFileTypes: true })
    .filter((entry) => entry.isDirectory() && entry.name !== 'assets')
    .map((entry) => entry.name)
    .sort();
  if (!languages.includes('en')) {
    addError(helpDir, null, "canonical language folder 'en' is missing");
    return { errors, languages, topics: new Map() };
  }

  // topics: id -> { perLang: { [lang]: { file, data, body, raw } } }
  const topics = new Map();

  for (const lang of languages) {
    const langDir = path.join(helpDir, lang);
    const files = fs
      .readdirSync(langDir)
      .filter((name) => name.endsWith('.md'))
      .sort();
    if (files.length === 0) {
      addError(langDir, null, `language folder '${lang}' contains no topics`);
    }

    for (const name of files) {
      const file = path.join(langDir, name);
      const raw = fs.readFileSync(file, 'utf8');
      let parsed;
      try {
        parsed = matter(raw);
      } catch (error) {
        addError(file, null, `frontmatter does not parse: ${error.message}`);
        continue;
      }
      const { data, content: body } = parsed;
      const id = path.basename(name, '.md');

      for (const field of REQUIRED_FIELDS) {
        if (data[field] === undefined || data[field] === null || data[field] === '') {
          addError(file, 1, `frontmatter is missing required field '${field}'`);
        }
      }
      if (data.id !== undefined && data.id !== id) {
        addError(file, 1, `frontmatter id '${data.id}' does not match filename '${id}'`);
      }
      if (data.category !== undefined && !(data.category in CATEGORIES)) {
        addError(
          file, 1,
          `unknown category '${data.category}' (expected one of: ${Object.keys(CATEGORIES).join(', ')})`
        );
      }
      if (data.screens !== undefined) {
        if (!Array.isArray(data.screens)) {
          addError(file, 1, `'screens' must be an array of route keys (or [])`);
        } else {
          for (const screen of data.screens) {
            if (!routeKeys.includes(screen)) {
              addError(file, 1, `screen '${screen}' is not a route key in routes.tsx (known: ${routeKeys.join(', ')})`);
            }
          }
        }
      }
      if (data.related !== undefined && !Array.isArray(data.related)) {
        addError(file, 1, `'related' must be an array of topic ids`);
      }
      if (data.tags !== undefined && !Array.isArray(data.tags)) {
        addError(file, 1, `'tags' must be an array of strings`);
      }
      if (data.order !== undefined && typeof data.order !== 'number') {
        addError(file, 1, `'order' must be a number`);
      }

      // Body: exactly one H1, equal to the title.
      const h1s = [...stripCode(body).matchAll(/^#\s+(.+)$/gm)];
      if (h1s.length !== 1) {
        addError(file, h1s.length ? lineOf(body, h1s[1].index) : null, `topic must have exactly one H1 (found ${h1s.length})`);
      } else if (typeof data.title === 'string' && h1s[0][1].trim() !== data.title.trim()) {
        addError(file, lineOf(body, h1s[0].index), `H1 '${h1s[0][1].trim()}' must equal frontmatter title '${data.title}'`);
      }

      // No raw HTML (autolinks like <https://…> are not matched: the char
      // after the tag name must be whitespace, '/', or '>').
      const codeless = stripCode(body);
      const htmlPattern = /<\/?[a-zA-Z][a-zA-Z0-9-]*(\s[^>]*)?\/?>/g;
      let htmlMatch;
      while ((htmlMatch = htmlPattern.exec(codeless)) !== null) {
        addError(file, lineOf(codeless, htmlMatch.index), `raw HTML is not allowed: '${htmlMatch[0]}'`);
      }

      // Links: topic: scheme only for internal links; .md links are an error.
      const linkPattern = /\[[^\]]*\]\(([^)\s]+)[^)]*\)/g;
      let linkMatch;
      while ((linkMatch = linkPattern.exec(body)) !== null) {
        const target = linkMatch[1];
        const line = lineOf(body, linkMatch.index);
        if (/\.md(#[^)]*)?$/i.test(target) && !/^https?:/i.test(target)) {
          addError(file, line, `relative .md link '${target}' is not allowed — use the topic: scheme`);
        }
      }

      // Images: relative assets/… paths only, and the file must exist.
      const imagePattern = /!\[[^\]]*\]\(([^)\s]+)[^)]*\)/g;
      let imageMatch;
      while ((imageMatch = imagePattern.exec(body)) !== null) {
        const target = imageMatch[1];
        const line = lineOf(body, imageMatch.index);
        if (!target.startsWith('assets/')) {
          addError(file, line, `image '${target}' must use a relative assets/ path`);
        } else if (!fs.existsSync(path.join(helpDir, target))) {
          addError(file, line, `image file '${target}' does not exist under public/help/`);
        }
      }

      if (!topics.has(id)) topics.set(id, { perLang: {} });
      topics.get(id).perLang[lang] = { file, data, body, raw };
    }
  }

  // Language parity + cross-language frontmatter consistency (EN canonical).
  for (const [id, topic] of topics) {
    for (const lang of languages) {
      if (!topic.perLang[lang]) {
        addError(
          path.join(helpDir, lang, `${id}.md`), null,
          `topic '${id}' is missing in language '${lang}' (copy the en file as an explicit placeholder if translation lags)`
        );
      }
    }
    const en = topic.perLang.en;
    if (!en) continue;
    for (const lang of languages) {
      const other = topic.perLang[lang];
      if (!other || lang === 'en') continue;
      for (const field of ['category', 'order', 'featureKey']) {
        if (JSON.stringify(other.data[field] ?? null) !== JSON.stringify(en.data[field] ?? null)) {
          addError(other.file, 1, `'${field}' differs from the en file (structural fields must be identical across languages)`);
        }
      }
      for (const field of ['screens', 'related']) {
        if (JSON.stringify(other.data[field] ?? []) !== JSON.stringify(en.data[field] ?? [])) {
          addError(other.file, 1, `'${field}' differs from the en file (structural fields must be identical across languages)`);
        }
      }
    }
  }

  // topic: link and related targets must exist.
  for (const [, topic] of topics) {
    for (const lang of Object.keys(topic.perLang)) {
      const { file, data, body } = topic.perLang[lang];
      const topicLinkPattern = /\[[^\]]*\]\(topic:([a-z0-9-]+)(#[^)]*)?\)/g;
      let match;
      while ((match = topicLinkPattern.exec(body)) !== null) {
        if (!topics.has(match[1])) {
          addError(file, lineOf(body, match.index), `topic: link target '${match[1]}' does not exist`);
        }
      }
      for (const related of Array.isArray(data.related) ? data.related : []) {
        if (!topics.has(related)) {
          addError(file, 1, `related topic '${related}' does not exist`);
        }
      }
    }
  }

  // Every non-hidden route key must be covered by ≥1 topic.
  const coveredScreens = new Set(
    [...topics.values()].flatMap((topic) =>
      Object.values(topic.perLang).flatMap(({ data }) => (Array.isArray(data.screens) ? data.screens : []))
    )
  );
  for (const key of routeKeys) {
    if (!coveredScreens.has(key)) {
      addError(routesFile, null, `route key '${key}' has no help topic (every screen needs ≥1 topic whose screens include it)`);
    }
  }

  return { errors, languages, topics };
}

function contentHash(...parts) {
  return crypto.createHash('sha256').update(parts.join('\n ')).digest('hex').slice(0, 12);
}

/** Build the manifest object from a valid (error-free) validation result. */
export function buildManifest({ languages, topics }) {
  const usedCategories = new Set();
  const manifestTopics = [];

  for (const [id, topic] of topics) {
    const en = topic.perLang.en;
    usedCategories.add(en.data.category);
    const i18n = {};
    for (const lang of languages) {
      const entry = topic.perLang[lang] ?? en;
      i18n[lang] = {
        title: entry.data.title,
        description: entry.data.description,
        tags: Array.isArray(entry.data.tags) ? entry.data.tags : [],
      };
    }
    manifestTopics.push({
      id,
      category: en.data.category,
      order: en.data.order,
      screens: Array.isArray(en.data.screens) ? en.data.screens : [],
      related: Array.isArray(en.data.related) ? en.data.related : [],
      featureKey: en.data.featureKey ?? null,
      hash: contentHash(...languages.map((lang) => (topic.perLang[lang] ?? en).raw)),
      i18n,
    });
  }

  manifestTopics.sort(
    (a, b) =>
      CATEGORIES[a.category] - CATEGORIES[b.category] ||
      a.order - b.order ||
      a.id.localeCompare(b.id)
  );

  return {
    version: contentHash(...manifestTopics.map((topic) => topic.hash)),
    languages,
    categories: Object.entries(CATEGORIES)
      .filter(([id]) => usedCategories.has(id))
      .map(([id, order]) => ({ id, order })),
    topics: manifestTopics,
  };
}

function main() {
  const checkOnly = process.argv.includes('--check');
  const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
  const helpDir = path.join(root, 'public', 'help');
  const routesFile = path.join(root, 'src', 'routes.tsx');

  const result = validateHelpContent({ helpDir, routesFile });
  if (result.errors.length > 0) {
    console.error(`Help content validation failed with ${result.errors.length} error(s):\n`);
    for (const { file, line, message } of result.errors) {
      console.error(`  ${path.relative(root, file)}${line ? `:${line}` : ''}  ${message}`);
    }
    process.exit(1);
  }

  const manifest = buildManifest(result);
  if (checkOnly) {
    console.log(
      `Help content OK: ${manifest.topics.length} topics × ${manifest.languages.length} languages (version ${manifest.version}).`
    );
    return;
  }
  const manifestFile = path.join(helpDir, 'manifest.json');
  fs.writeFileSync(manifestFile, JSON.stringify(manifest, null, 2) + '\n');
  console.log(
    `Wrote ${path.relative(root, manifestFile)}: ${manifest.topics.length} topics × ${manifest.languages.length} languages (version ${manifest.version}).`
  );
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  main();
}
