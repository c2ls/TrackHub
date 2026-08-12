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

import { useEffect, useMemo, useRef } from 'react';
import { useTranslation } from 'react-i18next';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import Typography from '@mui/material/Typography';

import type { HelpManifest, HelpTopicMeta } from 'queries/help';
import { useHelpTopic } from 'queries/help';
import { isTopicVisible, useHelpContext } from 'context/help/HelpContext';
import IndexView from 'controls/Help/IndexView';
import { buildMarkdownComponents, helpUrlTransform, slugify } from 'controls/Help/markdown';

interface TopicViewProps {
  manifest: HelpManifest;
  topic: HelpTopicMeta;
  lang: string;
  anchor?: string;
}

/**
 * Rendered topic: category chip, GFM body mapped onto MUI, and "Related
 * topics" chips (frontmatter related + other topics sharing a screen). A
 * topic missing on the server (stale manifest cache) degrades to the index
 * view with a non-blocking notice.
 */
function TopicView({ manifest, topic, lang, anchor }: TopicViewProps) {
  const { t } = useTranslation();
  const { navigateToTopic, allowedScreens, isFeatureEnabled } = useHelpContext();
  const contentRef = useRef<HTMLDivElement>(null);
  const { data, isPending } = useHelpTopic(lang, topic);

  const components = useMemo(() => buildMarkdownComponents(navigateToTopic), [navigateToTopic]);

  const relatedTopics = useMemo(() => {
    const ids = new Set(topic.related);
    for (const other of manifest.topics) {
      if (other.id !== topic.id && other.screens.some((screen) => topic.screens.includes(screen))) {
        ids.add(other.id);
      }
    }
    ids.delete(topic.id);
    return manifest.topics.filter(
      (other) => ids.has(other.id) && isTopicVisible(other, allowedScreens, isFeatureEnabled)
    );
  }, [manifest, topic, allowedScreens, isFeatureEnabled]);

  // Anchor scrolling for topic:id#anchor links, once the body is rendered.
  useEffect(() => {
    if (!anchor || !data?.markdown) return;
    const element = contentRef.current?.querySelector(`#${CSS.escape(slugify(anchor))}`);
    element?.scrollIntoView({ block: 'start' });
  }, [anchor, data?.markdown]);

  if (isPending) {
    return (
      <Box display="flex" justifyContent="center" py={6}>
        <CircularProgress size={28} />
      </Box>
    );
  }

  if (!data?.markdown) {
    return (
      <>
        <Alert severity="warning" sx={{ mb: 2 }}>
          {t('help.topicUnavailable')}
        </Alert>
        <IndexView manifest={manifest} lang={lang} onOpenTopic={navigateToTopic} />
      </>
    );
  }

  return (
    <div ref={contentRef}>
      <Box display="flex" alignItems="center" gap={1} mb={1}>
        <Chip
          size="small"
          label={t(`help.category.${topic.category}` as 'help.category.operation')}
          variant="outlined"
        />
      </Box>
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        skipHtml
        urlTransform={helpUrlTransform}
        components={components}
      >
        {data.markdown}
      </ReactMarkdown>
      {relatedTopics.length > 0 && (
        <>
          <Divider sx={{ mt: 3, mb: 1.5 }} />
          <Typography variant="caption" color="text.secondary" component="p" sx={{ mb: 1 }}>
            {t('help.related')}
          </Typography>
          <Box display="flex" flexWrap="wrap" gap={1}>
            {relatedTopics.map((related) => (
              <Chip
                key={related.id}
                size="small"
                label={(related.i18n[lang] ?? related.i18n.en).title}
                onClick={() => navigateToTopic(related.id)}
              />
            ))}
          </Box>
        </>
      )}
    </div>
  );
}

export default TopicView;
