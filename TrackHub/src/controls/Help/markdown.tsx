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

import type { ReactNode } from 'react';
import { Children, isValidElement } from 'react';
import type { Components } from 'react-markdown';
import { defaultUrlTransform } from 'react-markdown';

import Box from '@mui/material/Box';
import Divider from '@mui/material/Divider';
import Link from '@mui/material/Link';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';

/** GitHub-style heading anchor slugs (also what authors target in topic:id#anchor links). */
export function slugify(text: string): string {
  return text
    .toLowerCase()
    .trim()
    .replace(/[^\p{L}\p{N}\s-]/gu, '')
    .replace(/\s+/g, '-');
}

function extractText(children: ReactNode): string {
  let text = '';
  Children.forEach(children, (child) => {
    if (typeof child === 'string' || typeof child === 'number') {
      text += child;
    } else if (isValidElement<{ children?: ReactNode }>(child)) {
      text += extractText(child.props.children);
    }
  });
  return text;
}

/**
 * The default transform strips unknown schemes; keep our internal topic:
 * links intact and let it sanitize everything else.
 */
export function helpUrlTransform(url: string): string {
  return url.startsWith('topic:') ? url : defaultUrlTransform(url);
}

export function parseTopicHref(href: string): { id: string; anchor?: string } | null {
  const match = /^topic:([a-z0-9-]+)(?:#(.*))?$/.exec(href);
  return match ? { id: match[1], anchor: match[2] || undefined } : null;
}

/**
 * Maps rendered Markdown onto MUI so topics inherit the app theme. The body
 * H1 duplicates the dialog header title, so it is not rendered. Raw HTML never
 * reaches these components (skipHtml + build-time ban).
 */
export function buildMarkdownComponents(
  onTopicLink: (id: string, anchor?: string) => void
): Components {
  const heading =
    (variant: 'h5' | 'h6' | 'subtitle2', component: 'h2' | 'h3' | 'h4') =>
    ({ children }: { children?: ReactNode }) => (
      <Typography
        id={slugify(extractText(children))}
        variant={variant}
        component={component}
        sx={{ mt: 3, mb: 1 }}
      >
        {children}
      </Typography>
    );

  return {
    h1: () => null,
    h2: heading('h5', 'h2'),
    h3: heading('h6', 'h3'),
    h4: heading('subtitle2', 'h4'),
    p: ({ children }) => (
      <Typography variant="body2" component="p" sx={{ my: 1 }}>
        {children}
      </Typography>
    ),
    a: ({ href, children }) => {
      const topicLink = href ? parseTopicHref(href) : null;
      if (topicLink) {
        return (
          <Link
            component="button"
            type="button"
            variant="body2"
            onClick={() => onTopicLink(topicLink.id, topicLink.anchor)}
            sx={{ verticalAlign: 'baseline' }}
          >
            {children}
          </Link>
        );
      }
      return (
        <Link href={href} target="_blank" rel="noopener noreferrer" variant="body2">
          {children}
        </Link>
      );
    },
    img: ({ src, alt }) => (
      <Box
        component="img"
        src={typeof src === 'string' && src.startsWith('assets/') ? `/help/${src}` : src}
        alt={alt ?? ''}
        sx={{ maxWidth: '100%', borderRadius: 1, my: 1 }}
      />
    ),
    ul: ({ children }) => (
      <Box component="ul" sx={{ pl: 3, my: 1, '& li': { typography: 'body2', mb: 0.5 } }}>
        {children}
      </Box>
    ),
    ol: ({ children }) => (
      <Box component="ol" sx={{ pl: 3, my: 1, '& li': { typography: 'body2', mb: 0.5 } }}>
        {children}
      </Box>
    ),
    blockquote: ({ children }) => (
      <Box
        component="blockquote"
        sx={{
          borderLeft: 3,
          borderColor: 'divider',
          pl: 2,
          my: 1.5,
          mx: 0,
          color: 'text.secondary',
        }}
      >
        {children}
      </Box>
    ),
    code: ({ children }) => (
      <Box
        component="code"
        sx={{
          fontFamily: 'monospace',
          fontSize: '0.85em',
          bgcolor: 'action.hover',
          px: 0.5,
          borderRadius: 0.5,
        }}
      >
        {children}
      </Box>
    ),
    pre: ({ children }) => (
      <Box
        component="pre"
        sx={{
          overflowX: 'auto',
          bgcolor: 'action.hover',
          p: 1.5,
          borderRadius: 1,
          my: 1.5,
          '& code': { bgcolor: 'transparent', px: 0 },
        }}
      >
        {children}
      </Box>
    ),
    hr: () => <Divider sx={{ my: 2 }} />,
    table: ({ children }) => (
      <TableContainer sx={{ my: 1.5 }}>
        <Table size="small">{children}</Table>
      </TableContainer>
    ),
    thead: ({ children }) => <TableHead>{children}</TableHead>,
    tbody: ({ children }) => <TableBody>{children}</TableBody>,
    tr: ({ children }) => <TableRow>{children}</TableRow>,
    th: ({ children }) => (
      <TableCell sx={{ fontWeight: 'bold', typography: 'caption' }}>{children}</TableCell>
    ),
    td: ({ children }) => <TableCell sx={{ typography: 'body2' }}>{children}</TableCell>,
  };
}
