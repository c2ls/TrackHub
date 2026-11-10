/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

import { createContext, useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';
import Card from '@mui/material/Card';
import { useTranslation } from 'react-i18next';
import ArgonBox from 'components/ArgonBox';
import ArgonTypography from 'components/ArgonTypography';

interface SectionEntry {
  key: string;
  title: string;
  group?: string;
}

interface SectionNavValue {
  active: string | null;
  register: (entry: SectionEntry) => () => void;
}

/**
 * Present on screens laid out as a section list beside one open section. A `TableAccordion`
 * rendered under it lists itself in the navigation and shows its content only while selected.
 */
export const SectionNavContext = createContext<SectionNavValue | null>(null);

const SectionGroupContext = createContext<string | undefined>(undefined);
export { SectionGroupContext };

/** Names the navigation group of the sections rendered inside it. */
export function SectionGroup({ label, children }: { label: string; children: ReactNode }) {
  return <SectionGroupContext.Provider value={label}>{children}</SectionGroupContext.Provider>;
}

export function SectionNavLayout({ children }: { children: ReactNode }) {
  const { t } = useTranslation();
  const [entries, setEntries] = useState<SectionEntry[]>([]);
  const [active, setActive] = useState<string | null>(null);
  // Until the user picks (or closes) a section, the first one listed is shown.
  const chosenRef = useRef(false);

  const register = useCallback((entry: SectionEntry) => {
    setEntries((current) =>
      current.some((item) => item.key === entry.key)
        ? current.map((item) => (item.key === entry.key ? entry : item))
        : [...current, entry]
    );
    return () => setEntries((current) => current.filter((item) => item.key !== entry.key));
  }, []);

  useEffect(() => {
    if (!chosenRef.current && entries.length > 0 && !entries.some((item) => item.key === active)) {
      setActive(entries[0].key);
    }
  }, [entries, active]);

  const value = useMemo(() => ({ active, register }), [active, register]);

  // Sections register as they mount, so a group whose sections arrive late (feature flags) is
  // still listed as one block, in the order its first section appeared.
  const groups = useMemo(() => {
    const byGroup = new Map<string, SectionEntry[]>();
    entries.forEach((entry) => {
      const label = entry.group ?? '';
      byGroup.set(label, [...(byGroup.get(label) ?? []), entry]);
    });
    return [...byGroup.entries()];
  }, [entries]);

  const select = (key: string) => {
    chosenRef.current = true;
    setActive((current) => (current === key ? null : key));
  };

  return (
    <SectionNavContext.Provider value={value}>
      <ArgonBox display="flex" flexWrap="wrap" alignItems="flex-start" gap={3}>
        <Card
          component="nav"
          sx={({ breakpoints }) => ({
            flex: '1 1 14rem',
            p: 1,
            maxHeight: '40vh',
            overflowY: 'auto',
            [breakpoints.up('md')]: {
              flex: '0 0 15rem',
              position: 'sticky',
              top: 80,
              maxHeight: 'calc(100vh - 104px)',
            },
          })}
        >
          {groups.map(([label, items]) => (
            <ArgonBox key={label} mb={1}>
              {label && (
                <ArgonTypography
                  variant="caption"
                  fontWeight="bold"
                  textTransform="uppercase"
                  color="secondary"
                  display="block"
                  sx={{ letterSpacing: '0.06em', px: 1.25, pt: 1, pb: 0.5 }}
                >
                  {label}
                </ArgonTypography>
              )}
              {items.map(({ key, title }) => {
                const selected = key === active;
                return (
                  <ArgonBox
                    key={key}
                    component="button"
                    onClick={() => select(key)}
                    aria-expanded={selected}
                    data-testid={`section-${key}-header`}
                    sx={({ palette, borders, typography, functions: { rgba, pxToRem } }) => ({
                      display: 'block',
                      width: '100%',
                      minHeight: pxToRem(34),
                      px: 1.25,
                      py: 0.75,
                      border: 0,
                      borderRadius: borders.borderRadius.md,
                      background: selected ? rgba(palette.info.main, 0.12) : 'transparent',
                      color: selected ? palette.info.main : palette.text.main,
                      fontFamily: 'inherit',
                      fontSize: typography.size.sm,
                      fontWeight: selected ? typography.fontWeightBold : typography.fontWeightMedium,
                      textAlign: 'left',
                      cursor: 'pointer',
                      '&:hover': {
                        background: selected ? rgba(palette.info.main, 0.12) : palette.grey[100],
                      },
                    })}
                  >
                    {title}
                  </ArgonBox>
                );
              })}
            </ArgonBox>
          ))}
        </Card>
        <ArgonBox flex="999 1 28rem" minWidth={0}>
          {children}
          {active === null && entries.length > 0 && (
            <ArgonTypography variant="button" fontWeight="regular" color="secondary">
              {t('generic.selectSection')}
            </ArgonTypography>
          )}
        </ArgonBox>
      </ArgonBox>
    </SectionNavContext.Provider>
  );
}
