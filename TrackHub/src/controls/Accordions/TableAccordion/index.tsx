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

import type { MouseEvent, ReactNode } from "react";
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
  expanded: boolean;
  showAddIcon?: boolean;
  setOpen?: (open: boolean) => void;
  setExpanded: (expanded: boolean) => void;
  handleAddClick?: () => void;
  children: ReactNode;
}

const TableAccordion = ({
  title,
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

  return (
    <Accordion
      expanded={expanded}
      onChange={() => setExpanded(!expanded)}
      slots={{ heading: "div" }}
      sx={accordionSx}
    >
      <AccordionSummary
        component="div"
        expandIcon={<ExpandMoreIcon />}
        aria-controls={`${title}-content`}
        id={`${title}-header`}
        sx={summarySx}
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
