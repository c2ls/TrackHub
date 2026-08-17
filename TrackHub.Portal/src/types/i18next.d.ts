/**
 * react-i18next module augmentation: makes t() keys compile-checked against the
 * English translation bundle. The i18n instance (src/index.jsx) registers a
 * single default namespace 'translation' loaded from locales/en.json, so that
 * bundle is the source of truth for the key space.
 */
import 'i18next';

declare module 'i18next' {
  interface CustomTypeOptions {
    defaultNS: 'translation';
    resources: {
      // The runtime bundle is the core translations deep-merged with the edition
      // bundle (src/edition/locales, empty in this repository), so the key space
      // is the intersection type of both files.
      translation: typeof import('../locales/en.json') &
        typeof import('../edition/locales/en.json');
    };
    // i18next v25 / react-i18next v16 gate unknown-key compile errors behind
    // this flag; without it t() falls back to accepting any string.
    strictKeyChecks: true;
  }
}
