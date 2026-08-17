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

import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import enTranslations from 'locales/en.json'; 
import esTranslations from 'locales/es.json';

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: {
        translation: enTranslations
      },
      es: {
        translation: esTranslations
      }
    },
    fallbackLng: 'en', // use 'en' as the fallback language
    supportedLngs: ['en', 'es'],
    interpolation: {
      escapeValue: false
    }
  });

import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClientProvider } from "@tanstack/react-query";
import { queryClient } from "queries/queryClient";
import App from "App";
import AuthWrapper from 'AuthWrapper';

// Soft UI Context Provider
import { ArgonControllerProvider } from "context";
import { NotificationProvider } from "context/NotificationContext";

// react-perfect-scrollbar component
import PerfectScrollbar from "react-perfect-scrollbar";

// react-perfect-scrollbar styles
import "react-perfect-scrollbar/dist/css/styles.css";

const container = document.getElementById("root");
if (!container) {
  throw new Error('Root container "#root" not found in the document.');
}
const root = createRoot(container);

root.render(
  <BrowserRouter>
    <ArgonControllerProvider>
      <NotificationProvider>
        <QueryClientProvider client={queryClient}>
          <PerfectScrollbar>
            <AuthWrapper>
              <App />
            </AuthWrapper>
          </PerfectScrollbar>
        </QueryClientProvider>
      </NotificationProvider>
    </ArgonControllerProvider>
  </BrowserRouter>
);

