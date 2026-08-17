import { readdirSync } from 'node:fs';
import type { CodegenConfig } from '@graphql-codegen/cli';

/**
 * GraphQL codegen for the six TrackHub GraphQL backends (Reporting is
 * REST-only). Schemas in schemas/<service>.graphql are exported by the
 * contract-test suite (TrackHub.IntegrationTests → SchemaSdlExport) — run
 * `dotnet test TrackHub.IntegrationTests/TrackHub.IntegrationTests.slnx`
 * to refresh them, then `npm run codegen`.
 *
 * Operations live in src/api/<backend>/**, written with the generated
 * `graphql()` tag; values always travel as GraphQL variables.
 */

/** HotChocolate scalar → TypeScript mappings (UTC ISO strings for date/time). */
const scalars = {
  UUID: 'string',
  DateTime: 'string',
  DateTimeOffset: 'string',
  Date: 'string',
  LocalDate: 'string',
  LocalTime: 'string',
  TimeSpan: 'string',
  Long: 'number',
  Short: 'number',
  Byte: 'number',
  Decimal: 'number',
  URL: 'string',
  JSON: 'unknown',
};

// Discovered from schemas/ so registering a backend is just dropping its SDL export
// there and creating src/api/<backend>/ — no config edit.
const backends = readdirSync('schemas')
  .filter((file) => file.endsWith('.graphql'))
  .map((file) => file.replace(/\.graphql$/, ''))
  .sort();

const config: CodegenConfig = {
  generates: Object.fromEntries(
    backends.map((backend) => [
      `src/api/${backend}/generated/`,
      {
        preset: 'client' as const,
        schema: `schemas/${backend}.graphql`,
        documents: [`src/api/${backend}/**/*.ts`, `!src/api/${backend}/generated/**`],
        presetConfig: {
          // Plain result types; fragment masking adds friction without a
          // fragment-heavy component tree.
          fragmentMasking: false,
        },
        config: {
          scalars,
          enumsAsTypes: true,
          useTypeImports: true,
        },
      },
    ])
  ),
  ignoreNoDocuments: true,
};

export default config;
