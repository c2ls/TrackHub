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

import { createContext, useContext, useState, useCallback, useEffect } from "react";
import type { ReactNode, SyntheticEvent } from "react";
import Snackbar from "@mui/material/Snackbar";
import Alert from "@mui/material/Alert";
import type { AlertColor } from "@mui/material/Alert";
import type { SnackbarCloseReason } from "@mui/material/Snackbar";
import { useTranslation } from "react-i18next";
import type { AppErrorDetail } from "utils/errorHandler";

// Public API exposed to consumers via the useNotification hook.
export interface NotificationContextValue {
  showError: (message: string) => void;
  showSuccess: (message: string) => void;
  showWarning: (message: string) => void;
}

interface NotificationState {
  open: boolean;
  message: string;
  severity: AlertColor;
}

const NotificationContext = createContext<NotificationContextValue | undefined>(undefined);

export const useNotification = (): NotificationContextValue | undefined =>
  useContext(NotificationContext);

const AUTO_HIDE_DURATION = 6000;

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider = ({ children }: NotificationProviderProps) => {
  const { t } = useTranslation();
  const [notification, setNotification] = useState<NotificationState>({
    open: false,
    message: "",
    severity: "error",
  });

  const showError = useCallback((message: string) => {
    setNotification({ open: true, message, severity: "error" });
  }, []);

  const showSuccess = useCallback((message: string) => {
    setNotification({ open: true, message, severity: "success" });
  }, []);

  const showWarning = useCallback((message: string) => {
    setNotification({ open: true, message, severity: "warning" });
  }, []);

  const handleClose = useCallback(
    (_event: Event | SyntheticEvent, reason?: SnackbarCloseReason) => {
      if (reason === "clickaway") return;
      setNotification((prev) => ({ ...prev, open: false }));
    },
    []
  );

  // Listen for custom error events dispatched from non-React code (e.g., errorHandler.js)
  useEffect(() => {
    // The i18nKey on the event is a runtime-provided string, not a compile-time
    // known key, so translate it through a widened view of `t`.
    const translate = t as unknown as (key: string) => string;
    const handler = (e: Event) => {
      const detail = (e as CustomEvent<AppErrorDetail>).detail;
      showError(detail.i18nKey ? translate(detail.i18nKey) : detail.message);
    };
    window.addEventListener("app-error", handler);
    return () => window.removeEventListener("app-error", handler);
  }, [showError, t]);

  return (
    <NotificationContext.Provider value={{ showError, showSuccess, showWarning }}>
      {children}
      <Snackbar
        open={notification.open}
        autoHideDuration={AUTO_HIDE_DURATION}
        onClose={handleClose}
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
      >
        <Alert
          onClose={handleClose}
          severity={notification.severity}
          variant="filled"
          sx={{ width: "100%" }}
        >
          {notification.message}
        </Alert>
      </Snackbar>
    </NotificationContext.Provider>
  );
};
