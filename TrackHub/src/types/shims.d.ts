/**
 * Module declarations for dependencies that ship no types and have no
 * published @types package. Revisit each when its usage is converted
 *; delete a shim if upstream gains types.
 */

// The Google Maps JS API attaches its namespace to `window.google` at
// runtime once the loader script resolves. @types/google.maps declares the
// global `google` namespace but not this Window property.
interface Window {
  google: typeof google;
}

declare module 'leaflet-fullscreen' {
  // Side-effect plugin: augments L.Map with fullscreen control options.
  // Typed minimally; consumers interact with it through the leaflet API.
  const plugin: unknown;
  export default plugin;
}

// Side-effect-only deep import of the plugin bundle (registers L.control.fullscreen).
declare module 'leaflet-fullscreen/dist/Leaflet.fullscreen.js';

// NOTE: the `leaflet` module augmentation for leaflet-editable / leaflet-fullscreen
// lives in `leaflet-plugins.d.ts` (a module file) so it *merges* with @types/leaflet
// instead of shadowing it — a `declare module 'leaflet'` inside this script file
// would replace the package's own types.

declare module '@pathofdev/react-tag-input' {
  import * as React from 'react';

  export interface ReactTagInputProps {
    tags: string[];
    onChange: (tags: string[]) => void;
    placeholder?: string;
    maxTags?: number;
    editable?: boolean;
    readOnly?: boolean;
    removeOnBackspace?: boolean;
    validator?: (value: string) => boolean;
  }

  const ReactTagInput: React.FC<ReactTagInputProps>;
  export default ReactTagInput;
}

declare module 'react-github-btn' {
  import * as React from 'react';

  export interface GitHubButtonProps {
    href: string;
    'data-icon'?: string;
    'data-size'?: string;
    'data-show-count'?: boolean | string;
    'aria-label'?: string;
    children?: React.ReactNode;
  }

  const GitHubButton: React.FC<GitHubButtonProps>;
  export default GitHubButton;
}
