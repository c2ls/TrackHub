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

import { useContext, useEffect } from "react";
import type { MouseEvent, ReactNode } from "react";
import Card from "@mui/material/Card";
import { SectionNavContext, SectionGroupContext } from "controls/Accordions/SectionNav";
import type { Theme } from "@mui/material/styles";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import IconButton from "@mui/material/IconButton";
import Tooltip from "@mui/material/Tooltip";
import AddIcon from "@mui/icons-material/Add";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { useTranslation } from "react-i18next";
import ArgonTypography from "components/ArgonTypography";
import ArgonBox from "components/ArgonBox";
import ArgonButton from "components/ArgonButton";

const accordionSx = {
  boxShadow: "none",
  border: ({ borders: { borderWidth, borderColor } }: Theme) =>
    `${borderWidth[1]} solid ${borderColor}`,
  borderRadius: ({ borders: { borderRadius } }: Theme) => `${borderRadius.md} !important`,
  overflow: "hidden",
  mb: 1.5,
  "&:before": { display: "none" },
  "&.Mui-expanded": { margin: 0, mb: 1.5 },
};

const summarySx = {
  px: 2,
  minHeight: 56,
  "& .MuiAccordionSummary-content": { my: 1.5 },
  "&.Mui-expanded": {
    minHeight: 56,
    borderBottom: ({ borders: { borderWidth, borderColor } }: Theme) =>
      `${borderWidth[1]} solid ${borderColor}`,
  },
  "&.Mui-expanded .MuiAccordionSummary-content": { my: 1.5 },
};

export interface TableAccordionProps {
  title: string;
  /**
   * Stable, language-independent identifier for the section, rendered as
   * `data-testid="section-<key>"`. Screens stack many structurally identical
   * sections, so an end-to-end test needs a handle that does not move with the
   * locale or the section order.
   */
  sectionKey?: string;
  expanded: boolean;
  showAddIcon?: boolean;
  setOpen?: (open: boolean) => void;
  setExpanded: (expanded: boolean) => void;
  handleAddClick?: () => void;
  children: ReactNode;
}

const TableAccordion = ({
  title,
  sectionKey,
  expanded,
  showAddIcon = false,
  setOpen,
  setExpanded,
  handleAddClick = () => {},
  children,
}: TableAccordionProps) => {
  const { t } = useTranslation();

  const handleOpen = (event: MouseEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    handleAddClick();
    setOpen?.(true);
  };

  const nav = useContext(SectionNavContext);
  const group = useContext(SectionGroupContext);
  const navKey = sectionKey ?? title;
  const register = nav?.register;
  const selected = nav?.active === navKey;

  useEffect(() => register?.({ key: navKey, title, group }), [register, navKey, title, group]);

  // The section loads its data when expanded, so being selected in the navigation expands it.
  useEffect(() => {
    if (nav && selected !== expanded) setExpanded(selected);
  }, [nav, selected, expanded, setExpanded]);

  if (nav) {
    if (!selected) return null;
    return (
      <Card data-testid={`section-${navKey}`}>
        <ArgonBox
          display="flex"
          alignItems="center"
          justifyContent="space-between"
          px={2.5}
          minHeight={64}
          sx={({ borders: { borderWidth, borderColor } }: Theme) => ({
            borderBottom: `${borderWidth[1]} solid ${borderColor}`,
          })}
        >
          <ArgonTypography variant="h5" fontWeight="bold">
            {title}
          </ArgonTypography>
          {showAddIcon && (
            <ArgonButton
              variant="contained"
              color="primary"
              size="small"
              onClick={handleOpen}
              data-testid={`section-${navKey}-add`}
            >
              <AddIcon fontSize="small" sx={{ mr: 0.5 }} />
              {t("generic.add", { defaultValue: "Add" })}
            </ArgonButton>
          )}
        </ArgonBox>
        <ArgonBox p={2.5}>{children}</ArgonBox>
      </Card>
    );
  }

  return (
    <Accordion
      expanded={expanded}
      onChange={() => setExpanded(!expanded)}
      slots={{ heading: "div" }}
      sx={accordionSx}
      data-testid={sectionKey ? `section-${sectionKey}` : undefined}
    >
      <AccordionSummary
        component="div"
        expandIcon={<ExpandMoreIcon />}
        aria-controls={`${title}-content`}
        id={`${title}-header`}
        sx={summarySx}
        data-testid={sectionKey ? `section-${sectionKey}-header` : undefined}
      >
        <ArgonBox display="flex" alignItems="center" justifyContent="space-between" width="100%">
          <ArgonTypography variant="h6" fontWeight="medium">
            {title}
          </ArgonTypography>
          {expanded && showAddIcon && (
            <Tooltip title={t("generic.add", { defaultValue: "Add" })}>
              <IconButton
                size="small"
                color="primary"
                onClick={handleOpen}
                aria-label={t("generic.add", { defaultValue: "Add" })}
                data-testid={sectionKey ? `section-${sectionKey}-add` : undefined}
                sx={{ mr: 1 }}
              >
                <AddIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </ArgonBox>
      </AccordionSummary>
      <AccordionDetails sx={{ p: 2 }}>
        <ArgonBox
          sx={{
            "& .MuiTableRow-root:not(:last-child)": {
              "& td": {
                borderBottom: ({ borders: { borderWidth, borderColor } }: Theme) =>
                  `${borderWidth[1]} solid ${borderColor}`,
              },
            },
          }}
        >
          {children}
        </ArgonBox>
      </AccordionDetails>
    </Accordion>
  );
};

export default TableAccordion;
