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

import { useTranslation } from 'react-i18next';

import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import CircularProgress from '@mui/material/CircularProgress';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import Icon from '@mui/material/Icon';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import useMediaQuery from '@mui/material/useMediaQuery';
import { useTheme } from '@mui/material/styles';

import { normalizeHelpLanguage, useHelpManifest } from 'queries/help';
import type { HelpManifest, HelpTopicMeta } from 'queries/help';
import {
  isTopicVisible,
  topicsForScreen,
  useHelpContext,
} from 'context/help/HelpContext';
import type { HelpNavEntry } from 'context/help/HelpContext';
import IndexView from 'controls/Help/IndexView';
import TopicView from 'controls/Help/TopicView';

interface ResolvedView {
  kind: 'index' | 'topic' | 'missing';
  topic?: HelpTopicMeta;
  anchor?: string;
}

/**
 * Resolve the top navigation entry against the manifest. `auto` becomes the
 * current screen's primary topic; a topic id absent from the manifest (stale
 * cache) degrades to the index with a notice.
 */
function resolveView(
  entry: HelpNavEntry | undefined,
  manifest: HelpManifest,
  currentScreenKey: string | null,
  allowedScreens: string[],
  isFeatureEnabled: (featureKey?: string | null) => boolean
): ResolvedView {
  if (!entry || entry.type === 'index') return { kind: 'index' };
  if (entry.type === 'auto') {
    const primary = topicsForScreen(manifest, currentScreenKey).find((topic) =>
      isTopicVisible(topic, allowedScreens, isFeatureEnabled)
    );
    return primary ? { kind: 'topic', topic: primary } : { kind: 'index' };
  }
  const topic = manifest.topics.find((candidate) => candidate.id === entry.id);
  return topic ? { kind: 'topic', topic, anchor: entry.anchor } : { kind: 'missing' };
}

/** Contextual help modal. Rendered once by HelpProvider. */
function HelpDialog() {
  const { t, i18n } = useTranslation();
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));
  const {
    open,
    entries,
    closeHelp,
    navigateToTopic,
    openIndex,
    goBack,
    currentScreenKey,
    allowedScreens,
    isFeatureEnabled,
  } = useHelpContext();

  // Lazy: nothing is fetched until the dialog first opens.
  const { data: manifest, isPending, isError } = useHelpManifest({ enabled: open });

  const lang = normalizeHelpLanguage(i18n.language, manifest?.languages ?? ['en']);
  const view = manifest
    ? resolveView(
        entries[entries.length - 1],
        manifest,
        currentScreenKey,
        allowedScreens,
        isFeatureEnabled
      )
    : null;

  const title =
    view?.kind === 'topic' && view.topic
      ? (view.topic.i18n[lang] ?? view.topic.i18n.en).title
      : t('help.title');

  return (
    <Dialog
      open={open}
      onClose={closeHelp}
      maxWidth="md"
      fullWidth
      fullScreen={fullScreen}
      aria-labelledby="help-dialog-title"
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pr: 1 }}>
        {entries.length > 1 && (
          <IconButton size="small" onClick={goBack} aria-label={t('help.back')}>
            <Icon>arrow_back</Icon>
          </IconButton>
        )}
        <Typography id="help-dialog-title" variant="h6" component="span" sx={{ flexGrow: 1 }}>
          {title}
        </Typography>
        {view?.kind === 'topic' && (
          <IconButton size="small" onClick={openIndex} aria-label={t('help.index')}>
            <Icon>toc</Icon>
          </IconButton>
        )}
        <IconButton size="small" onClick={closeHelp} aria-label={t('generic.close')}>
          <Icon>close</Icon>
        </IconButton>
      </DialogTitle>
      <DialogContent dividers>
        {isPending && (
          <Box display="flex" justifyContent="center" py={6}>
            <CircularProgress size={28} />
          </Box>
        )}
        {isError && <Alert severity="error">{t('help.unavailable')}</Alert>}
        {manifest && view && (
          <>
            {view.kind === 'missing' && (
              <Alert severity="warning" sx={{ mb: 2 }}>
                {t('help.topicUnavailable')}
              </Alert>
            )}
            {view.kind === 'topic' && view.topic ? (
              <TopicView
                manifest={manifest}
                topic={view.topic}
                lang={lang}
                anchor={view.anchor}
              />
            ) : (
              <IndexView manifest={manifest} lang={lang} onOpenTopic={navigateToTopic} />
            )}
          </>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={closeHelp}>{t('generic.close')}</Button>
      </DialogActions>
    </Dialog>
  );
}

export default HelpDialog;
