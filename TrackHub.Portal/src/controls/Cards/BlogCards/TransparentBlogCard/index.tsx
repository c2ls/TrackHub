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

// react-router components
import { Link } from "react-router-dom";

// @mui material components
import Card from "@mui/material/Card";
import CardMedia from "@mui/material/CardMedia";
import Icon from "@mui/material/Icon";
import MuiLink from "@mui/material/Link";
import type { SxProps, Theme } from "@mui/material/styles";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

export type TransparentBlogCardActionColor =
  | "inherit"
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "light"
  | "dark"
  | "text";

export interface TransparentBlogCardProps {
  image: string;
  title: string;
  description: string;
  action: {
    type?: "external" | "internal";
    route: string;
    label: string;
    color: TransparentBlogCardActionColor;
  };
}

function TransparentBlogCard({ image, title, description, action }: TransparentBlogCardProps) {
  const cardImageStyles: SxProps<Theme> = {
    width: "100%",
    height: "auto",
    minHeight: ({ functions: { pxToRem } }) => pxToRem(235),
    margin: 0,
    boxShadow: ({ boxShadows: { md } }) => md,
    transformOrigin: "50% 0",
    transform: `perspective(999px) rotateX(0deg) translateZ(0)`,
    WebkitBackfaceVisibility: "hidden",
    backfaceVisibility: "hidden",
    willChange: "transform, box-shadow",
    transition: ({ transitions }) =>
      transitions.create("transform", {
        easing: transitions.easing.easeIn,
        duration: transitions.duration.shorter,
      }),

    "&:hover": {
      transform: `perspective(999px) rotateX(7deg) translate3d(0, -4px, 5px)`,
    },
  };

  const cardActionStyles: SxProps<Theme> = {
    display: "inline-block",
    textDecoration: "none",

    "& .MuiTypography-root": {
      display: "flex",
      alignItems: "center",
    },

    "& .material-icons, .material-icons-round,": {
      fontSize: "1.125rem",
      transform: `translateX(2px)`,
      transition: "transform 0.2s cubic-bezier(0.34,1.61,0.7,1.3)",
    },

    "&:hover .material-icons, &:focus .material-icons, &:hover .material-icons-round, &:focus .material-icons-round":
      {
        transform: `translateX(6px)`,
      },
  };

  return (
    <Card
      sx={{
        background: "transparent",
        boxShadow: "none",
        overflow: "visible",
      }}
    >
      {action.type === "internal" ? (
        <Link to={action.route}>
          <CardMedia src={image} component="img" title={title} sx={cardImageStyles} />
        </Link>
      ) : (
        <MuiLink href={action.route} target="_blank" rel="noreferrer">
          <CardMedia src={image} component="img" title={title} sx={cardImageStyles} />
        </MuiLink>
      )}
      <ArgonBox py={3}>
        {action.type === "internal" ? (
          <Link to={action.route}>
            <ArgonTypography variant="h5" gutterBottom>
              {title}
            </ArgonTypography>
          </Link>
        ) : (
          <MuiLink href={action.route} target="_blank" rel="noreferrer" sx={cardActionStyles}>
            <ArgonTypography variant="h5" gutterBottom>
              {title}
            </ArgonTypography>
          </MuiLink>
        )}
        <ArgonBox mb={2}>
          <ArgonTypography variant="body2" component="p" color="text">
            {description}
          </ArgonTypography>
        </ArgonBox>
        {action.type === "internal" ? (
          <Link to={action.route}>
            <ArgonTypography
              variant="body2"
              color={action.color}
              textTransform="capitalize"
              component="span"
            >
              {action.label}
              <Icon sx={{ fontWeight: "bold" }}>arrow_forward</Icon>
            </ArgonTypography>
          </Link>
        ) : (
          <MuiLink href={action.route} target="_blank" rel="noreferrer" sx={cardActionStyles}>
            <ArgonTypography
              variant="body2"
              color={action.color}
              textTransform="capitalize"
              component="span"
            >
              {action.label}
              <Icon sx={{ fontWeight: "bold" }}>arrow_forward</Icon>
            </ArgonTypography>
          </MuiLink>
        )}
      </ArgonBox>
    </Card>
  );
}

export default TransparentBlogCard;
