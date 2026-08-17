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

import TextField from '@mui/material/TextField';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';

/**
 * Search box for a SERVER-paged list. The value is a draft that
 * `controls/Tables/useServerList` debounces into the query's `search` argument,
 * so the search covers the whole result set rather than the loaded page.
 */
export interface ServerSearchProps {
  value: string;
  onChange: (value: string) => void;
}

function ServerSearch({ value, onChange }: ServerSearchProps) {
  const { t } = useTranslation();

  return (
    <ArgonBox mb={1} maxWidth="320px">
      <TextField
        fullWidth
        size="small"
        label={t('filters.search')}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
    </ArgonBox>
  );
}

export default ServerSearch;
