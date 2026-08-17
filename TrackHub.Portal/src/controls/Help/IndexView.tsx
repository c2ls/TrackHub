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

import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';

import Icon from '@mui/material/Icon';
import InputAdornment from '@mui/material/InputAdornment';
import List from '@mui/material/List';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import ListSubheader from '@mui/material/ListSubheader';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

import type { HelpManifest, HelpTopicMeta } from 'queries/help';
import { isTopicVisible, topicsForScreen, useHelpContext } from 'context/help/HelpContext';

interface IndexViewProps {
  manifest: HelpManifest;
  lang: string;
  onOpenTopic: (id: string) => void;
}

function topicI18n(topic: HelpTopicMeta, lang: string) {
  return topic.i18n[lang] ?? topic.i18n.en;
}

/**
 * Browsable, searchable topic index. Search is a client-side filter over the
 * localized title/description/tags; the current screen's topics are pinned in
 * a "This screen" group above the category groups.
 */
function IndexView({ manifest, lang, onOpenTopic }: IndexViewProps) {
  const { t } = useTranslation();
  const { currentScreenKey, allowedScreens, isFeatureEnabled } = useHelpContext();
  const [search, setSearch] = useState('');

  const visibleTopics = useMemo(
    () => manifest.topics.filter((topic) => isTopicVisible(topic, allowedScreens, isFeatureEnabled)),
    [manifest, allowedScreens, isFeatureEnabled]
  );

  const query = search.trim().toLowerCase();
  const searchResults = useMemo(() => {
    if (!query) return null;
    return visibleTopics.filter((topic) => {
      const { title, description, tags } = topicI18n(topic, lang);
      return [title, description, ...tags].some((text) => text.toLowerCase().includes(query));
    });
  }, [visibleTopics, query, lang]);

  const screenTopics = useMemo(
    () =>
      topicsForScreen(manifest, currentScreenKey).filter((topic) =>
        isTopicVisible(topic, allowedScreens, isFeatureEnabled)
      ),
    [manifest, currentScreenKey, allowedScreens, isFeatureEnabled]
  );

  const categories = useMemo(
    () => [...manifest.categories].sort((a, b) => a.order - b.order),
    [manifest]
  );

  const renderTopics = (topics: HelpTopicMeta[]) =>
    topics.map((topic) => {
      const { title, description } = topicI18n(topic, lang);
      return (
        <ListItemButton key={topic.id} onClick={() => onOpenTopic(topic.id)}>
          <ListItemText
            primary={title}
            secondary={description}
            primaryTypographyProps={{ variant: 'button', fontWeight: 'medium' }}
            secondaryTypographyProps={{ variant: 'caption' }}
          />
        </ListItemButton>
      );
    });

  return (
    <>
      <TextField
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder={t('help.search')}
        fullWidth
        size="small"
        autoFocus
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <Icon fontSize="small">search</Icon>
              </InputAdornment>
            ),
          },
        }}
        sx={{ mb: 1 }}
      />
      {searchResults ? (
        searchResults.length > 0 ? (
          <List dense disablePadding>
            {renderTopics(searchResults)}
          </List>
        ) : (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            {t('help.noResults')}
          </Typography>
        )
      ) : (
        <List dense disablePadding>
          {screenTopics.length > 0 && (
            <>
              <ListSubheader disableSticky>{t('help.thisScreen')}</ListSubheader>
              {renderTopics(screenTopics)}
            </>
          )}
          {categories.map((category) => {
            const topics = visibleTopics.filter((topic) => topic.category === category.id);
            if (topics.length === 0) return null;
            return (
              <div key={category.id}>
                <ListSubheader disableSticky>
                  {t(`help.category.${category.id}` as 'help.category.operation')}
                </ListSubheader>
                {renderTopics(topics)}
              </div>
            );
          })}
        </List>
      )}
    </>
  );
}

export default IndexView;
