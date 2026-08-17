/**
 * The build stamp is the surface a deployment/rollback check reads, so what matters is
 * that it is actually substituted and stays parseable — not which version it happens to
 * be today. Asserting the literal version would fail on every package.json bump.
 */
import { describe, it, expect } from 'vitest';
import { APP_VERSION, BUILD_TIME, versionLabel, buildLabel } from 'constants/appVersion';

describe('appVersion', () => {
  it('substitutes a real semver from package.json rather than the ReferenceError fallback', () => {
    expect(APP_VERSION).toMatch(/^\d+\.\d+\.\d+/);
    expect(APP_VERSION).not.toBe('0.0.0');
  });

  it('stamps a parseable build time', () => {
    expect(BUILD_TIME).not.toBe('');
    expect(Number.isNaN(new Date(BUILD_TIME).getTime())).toBe(false);
  });

  it('renders the compact footer label', () => {
    expect(versionLabel()).toBe(`v${APP_VERSION}`);
  });

  it('renders version and UTC build time together for the status page', () => {
    // The two builds this has to tell apart usually share a version, so the timestamp —
    // not the version — is the discriminating half. It must always be present.
    expect(buildLabel()).toMatch(/^v\d+\.\d+\.\d+.* · \d{4}-\d{2}-\d{2} \d{2}:\d{2} UTC$/);
    expect(buildLabel().startsWith(versionLabel())).toBe(true);
  });
});
