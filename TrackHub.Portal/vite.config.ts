/// <reference types="vitest/config" />
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import fs from 'node:fs';

const pkg = JSON.parse(fs.readFileSync('package.json', 'utf-8')) as { version: string };

export default defineConfig(({ mode }) => {
  // PERMANENT: all REACT_APP_* reads are centralized
  // in src/api/core/endpoints.ts, which deliberately keeps the CRA
  // process.env.REACT_APP_* convention so the existing .env files,
  // .env.production.template, and deployment docs stay valid. This shim
  // statically replaces those references at build time. Skipped in test mode:
  // tests assign process.env.REACT_APP_* at runtime (endpoints.ts OAuth getters
  // are lazy for exactly that reason), which static replacement would corrupt.
  // NODE_ENV reads elsewhere need no shim — Vite defines those natively.
  const isTest = mode === 'test';
  const env = loadEnv(mode, process.cwd(), 'REACT_APP_');
  const envDefine = isTest
    ? {}
    : Object.fromEntries(
        Object.entries(env).map(([key, value]) => [`process.env.${key}`, JSON.stringify(value)])
      );

  // Build stamp shown in the UI (src/constants/appVersion.ts). package.json "version" is
  // the release identity; the build time makes two builds of the SAME version
  // distinguishable, which is what a deploy/rollback check actually needs — the image is
  // what gets rolled back, not the version number. Unlike the REACT_APP_* shim above these
  // are defined in test mode too: they are plain globals, not process.env references, so
  // there is nothing for a runtime assignment in a suite to conflict with.
  const define = {
    ...envDefine,
    __APP_VERSION__: JSON.stringify(pkg.version),
    __BUILD_TIME__: JSON.stringify(new Date().toISOString()),
  };

  const https =
    fs.existsSync('cert.crt') && fs.existsSync('cert.key')
      ? { cert: fs.readFileSync('cert.crt'), key: fs.readFileSync('cert.key') }
      : undefined;

  return {
    plugins: [react()],
    resolve: {
      // Honors tsconfig "paths" (absolute imports rooted at src/)
      tsconfigPaths: true,
    },
    define,
    server: {
      port: 3000,
      // The OAuth callback is registered at https://localhost:3000/... —
      // fail loudly if the port is taken instead of drifting to 3001.
      strictPort: true,
      https,
    },
    build: {
      outDir: 'build',
    },
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['src/setupTests.ts'],
      include: ['src/**/*.test.{js,jsx,ts,tsx}'],
    },
  };
});
