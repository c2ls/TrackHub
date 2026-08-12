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

/**
=========================================================
* Argon Dashboard 2 MUI - v3.0.1
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2023 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

 =========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

// react-router-dom components
import { Link } from "react-router-dom";

// @mui material components
import Card from "@mui/material/Card";
import CardMedia from "@mui/material/CardMedia";
import Tooltip from "@mui/material/Tooltip";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonButton from "components/ArgonButton";
import ArgonAvatar from "components/ArgonAvatar";

export type DefaultProjectCardActionColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "light"
  | "dark"
  | "white";

export interface DefaultProjectCardAuthor {
  image: string;
  name: string;
}

export interface DefaultProjectCardProps {
  image: string;
  label: string;
  title: string;
  description: string;
  action: {
    type?: "external" | "internal";
    route: string;
    color: DefaultProjectCardActionColor;
    label: string;
  };
  authors?: DefaultProjectCardAuthor[];
}

function DefaultProjectCard({
  image,
  label,
  title,
  description,
  action,
  authors = [],
}: DefaultProjectCardProps) {
  const renderAuthors = authors.map(({ image: media, name }) => (
    <Tooltip key={name} title={name} placement="bottom">
      <ArgonAvatar
        src={media}
        alt={name}
        size="xs"
        sx={({ borders: { borderWidth }, palette: { white } }) => ({
          border: `${borderWidth[2]} solid ${white.main}`,
          cursor: "pointer",
          position: "relative",
          ml: -1.25,

          "&:hover, &:focus": {
            zIndex: "10",
          },
        })}
      />
    </Tooltip>
  ));

  return (
    <Card
      sx={{
        display: "flex",
        flexDirection: "column",
        backgroundColor: "transparent",
        boxShadow: "none",
        overflow: "visible",
      }}
    >
      <ArgonBox position="relative" width="100.25%" shadow="md" borderRadius="xl">
        <CardMedia
          src={image}
          component="img"
          title={title}
          sx={{
            maxWidth: "100%",
            margin: 0,
            boxShadow: ({ boxShadows: { md } }) => md,
            objectFit: "cover",
            objectPosition: "center",
          }}
        />
      </ArgonBox>
      <ArgonBox pt={2} px={0.5}>
        <ArgonTypography
          variant="button"
          fontWeight="regular"
          textTransform="capitalize"
          textGradient
        >
          {label}
        </ArgonTypography>
        <ArgonBox mb={1}>
          {action.type === "internal" ? (
            <ArgonTypography
              component={Link}
              {...({ to: action.route } as Record<string, unknown>)}
              variant="h5"
              textTransform="capitalize"
            >
              {title}
            </ArgonTypography>
          ) : (
            <ArgonTypography
              component="a"
              {...({ href: action.route, target: "_blank", rel: "noreferrer" } as Record<
                string,
                unknown
              >)}
              variant="h5"
              textTransform="capitalize"
            >
              {title}
            </ArgonTypography>
          )}
        </ArgonBox>
        <ArgonBox mb={3} lineHeight={0}>
          <ArgonTypography variant="button" fontWeight="regular" color="text">
            {description}
          </ArgonTypography>
        </ArgonBox>
        <ArgonBox display="flex" justifyContent="space-between" alignItems="center">
          {action.type === "internal" ? (
            <ArgonButton
              component={Link}
              {...({ to: action.route } as Record<string, unknown>)}
              variant="outlined"
              size="small"
              color={action.color}
            >
              {action.label}
            </ArgonButton>
          ) : (
            <ArgonButton
              component="a"
              {...({ href: action.route, target: "_blank", rel: "noreferrer" } as Record<
                string,
                unknown
              >)}
              variant="outlined"
              size="small"
              color={action.color}
            >
              {action.label}
            </ArgonButton>
          )}
          <ArgonBox display="flex">{renderAuthors}</ArgonBox>
        </ArgonBox>
      </ArgonBox>
    </Card>
  );
}

export default DefaultProjectCard;
