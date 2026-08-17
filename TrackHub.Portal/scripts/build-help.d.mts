/** Hand-written declarations so vitest suites under src/ can import the build script. */

export interface HelpValidationError {
  file: string;
  line: number | null;
  message: string;
}

export interface HelpTopicLangEntry {
  file: string;
  data: Record<string, unknown>;
  body: string;
  raw: string;
}

export interface HelpValidationResult {
  errors: HelpValidationError[];
  languages: string[];
  topics: Map<string, { perLang: Record<string, HelpTopicLangEntry> }>;
}

export interface HelpManifest {
  version: string;
  languages: string[];
  categories: { id: string; order: number }[];
  topics: {
    id: string;
    category: string;
    order: number;
    screens: string[];
    related: string[];
    featureKey: string | null;
    hash: string;
    i18n: Record<string, { title: string; description: string; tags: string[] }>;
  }[];
}

export declare const CATEGORIES: Record<string, number>;
export declare function extractRouteKeys(routesSource: string): string[];
export declare function validateHelpContent(options: {
  helpDir: string;
  routesFile: string;
}): HelpValidationResult;
export declare function buildManifest(result: HelpValidationResult): HelpManifest;
