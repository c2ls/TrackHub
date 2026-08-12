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

import type { ReactNode } from "react";

// react-router components
import { Link } from "react-router-dom";

// @mui material components
import Card from "@mui/material/Card";
import MuiLink from "@mui/material/Link";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonAvatar from "components/ArgonAvatar";

export type DefaultBlogCardCategoryColor =
  | "primary"
  | "secondary"
  | "info"
  | "success"
  | "warning"
  | "error"
  | "dark";

export interface DefaultBlogCardProps {
  image: string;
  category?:
    | false
    | {
        color: DefaultBlogCardCategoryColor;
        label: string;
      };
  title: string;
  description: string;
  author?:
    | false
    | {
        image: string;
        name: string;
        date: ReactNode;
      };
  action: {
    type: "external" | "internal";
    route: string;
  };
}

function DefaultBlogCard({
  image,
  category = false,
  title,
  description,
  author = false,
  action,
}: DefaultBlogCardProps) {
  return (
    <Card>
      <ArgonBox mt={2} mx={2}>
        {action.type === "internal" ? (
          <Link to={action.route}>
            <ArgonBox
              component="img"
              src={image}
              alt={title}
              width="100%"
              borderRadius="lg"
            />
          </Link>
        ) : (
          <MuiLink href={action.route} target="_blank" rel="noreferrer">
            <ArgonBox
              component="img"
              src={image}
              alt={title}
              width="100%"
              borderRadius="lg"
            />
          </MuiLink>
        )}
      </ArgonBox>
      <ArgonBox pb={3} px={3}>
        {category && (
          <ArgonTypography
            variant="caption"
            color={category.color}
            textTransform="uppercase"
            fontWeight="medium"
            textGradient
          >
            {category.label}
          </ArgonTypography>
        )}
        <ArgonBox display="block" mt={0.5} mb={1}>
          {action.type === "internal" ? (
            <Link to={action.route}>
              <ArgonTypography
                display="inline"
                variant="h5"
                textTransform="capitalize"
                className="color-background"
              >
                {title}
              </ArgonTypography>
            </Link>
          ) : (
            <MuiLink href={action.route} target="_blank" rel="noreferrer">
              <ArgonTypography
                display="inline"
                variant="h5"
                textTransform="capitalize"
                className="color-background"
              >
                {title}
              </ArgonTypography>
            </MuiLink>
          )}
        </ArgonBox>
        <ArgonTypography variant="body2" component="p" color="text">
          {description}
        </ArgonTypography>
        {author && (
          <ArgonBox display="flex" alignItems="center" mt={3}>
            <ArgonAvatar variant="rounded" src={author.image} alt={author.name} shadow="md" />
            <ArgonBox pl={2} lineHeight={0}>
              <ArgonTypography component="h6" variant="button" fontWeight="medium" gutterBottom>
                {author.name}
              </ArgonTypography>
              <ArgonTypography variant="caption" color="text">
                {author.date}
              </ArgonTypography>
            </ArgonBox>
          </ArgonBox>
        )}
      </ArgonBox>
    </Card>
  );
}

export default DefaultBlogCard;
