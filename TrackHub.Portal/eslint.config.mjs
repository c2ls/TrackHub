import react from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import prettierRecommended from "eslint-plugin-prettier/recommended";
import globals from "globals";

// Parity with the retired .eslintrc.json (react/recommended + prettier, a few
// overrides). Deliberately NOT js/recommended: enabling it on 480 legacy files
// would drown real signal.
//
// NOTE: typescript-eslint is installed but disabled — its typescript-estree
// does not support TypeScript 7 yet (crashes on ts.ModuleKind.Cjs). Until it
// does, .ts/.tsx files are ignored here; `npm run typecheck` (tsc --noEmit)
// is the gate for TypeScript files. Re-check on each typescript-eslint release.
export default [
  {
    ignores: ["build/**", "node_modules/**", "coverage/**", "public/**", "**/*.ts", "**/*.tsx"],
  },
  react.configs.flat.recommended,
  prettierRecommended,
  {
    files: ["**/*.{js,jsx}"],
    plugins: { "react-hooks": reactHooks },
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: { ...globals.browser, JSX: true },
      parserOptions: { ecmaFeatures: { jsx: true } },
    },
    settings: { react: { version: "detect" } },
    rules: {
      ...reactHooks.configs.recommended.rules,
      "react/jsx-filename-extension": [1, { extensions: [".jsx", ".tsx"] }],
      "react/require-default-props": "off",
      "no-param-reassign": "off",
      "react/react-in-jsx-scope": "off",
      "react/display-name": "off",
    },
  },
  {
    files: ["src/**/*.test.{js,jsx}", "src/__tests__/**"],
    languageOptions: { globals: { ...globals.node } },
  },
  // Phase 7 guard: src/ is 100% TypeScript — any new .js/.jsx file is an error.
  {
    files: ["src/**/*.{js,jsx}"],
    rules: {
      "no-restricted-syntax": [
        "error",
        {
          selector: "Program",
          message:
            "New .js/.jsx files are forbidden under src/ — write TypeScript (.ts/.tsx). See specs/utils/26-frontend-typescript-migration.md.",
        },
      ],
    },
  },
  {
    files: ["eslint.config.mjs"],
    languageOptions: { globals: { ...globals.node } },
  },
];
