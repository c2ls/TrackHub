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

import { useState, useEffect } from "react";
import { useNavigate } from 'react-router-dom';
import { useTranslation } from "react-i18next";
import { useAuth } from "AuthContext";

// @mui material components
import Card from "@mui/material/Card";
import Container from "@mui/material/Container";
import Alert from "@mui/material/Alert";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonButton from "components/ArgonButton";

const ErrorPage = () => {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { resetAuthError, login } = useAuth();
  const [errorMessage, setErrorMessage] = useState("");
  const [canRetry, setCanRetry] = useState(false);
  const [countdown, setCountdown] = useState(30);

  useEffect(() => {
    // Get the specific error from session storage
    const authError = sessionStorage.getItem('auth_error');
    
    // If no error exists, redirect to dashboard (user shouldn't be on this page)
    if (!authError) {
      navigate('/dashboard', { replace: true });
      return;
    }
    
    switch(authError) {
      case 'token_exchange_failed':
        setErrorMessage(t('authError.tokenExchangeFailed'));
        break;
      case 'no_code':
        setErrorMessage(t('authError.noCode'));
        break;
      default:
        setErrorMessage(authError || t('authError.generic'));
    }

    // Start countdown for retry button
    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          setCanRetry(true);
          clearInterval(timer);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const handleRetry = () => {
    // Clear error states
    sessionStorage.removeItem('auth_error');
    resetAuthError();
    
    // Attempt login again
    login();
  };

  const handleGoBack = () => {
    sessionStorage.removeItem('auth_error');
    resetAuthError();
    navigate('/');
  };

  return (
    <ArgonBox
      minHeight="100vh"
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background: "linear-gradient(195deg, #42424a 0%, #191919 100%)",
      }}
    >
      <Container maxWidth="sm">
        <Card>
          <ArgonBox p={3} textAlign="center">
            <ArgonBox mb={2}>
              <ArgonTypography variant="h3" fontWeight="bold" color="error">
                {t('authError.title')}
              </ArgonTypography>
            </ArgonBox>
            
            <ArgonBox mb={3}>
              <Alert severity="error" sx={{ textAlign: "left" }}>
                {errorMessage}
              </Alert>
            </ArgonBox>

            <ArgonBox mb={2}>
              <ArgonTypography variant="body2" color="text">
                {t('authError.causesIntro')}
              </ArgonTypography>
              <ArgonBox component="ul" textAlign="left" pl={4} mt={1}>
                <li>
                  <ArgonTypography variant="body2" color="text">
                    {t('authError.causeUnavailable')}
                  </ArgonTypography>
                </li>
                <li>
                  <ArgonTypography variant="body2" color="text">
                    {t('authError.causeNetwork')}
                  </ArgonTypography>
                </li>
                <li>
                  <ArgonTypography variant="body2" color="text">
                    {t('authError.causeInvalidRequest')}
                  </ArgonTypography>
                </li>
              </ArgonBox>
            </ArgonBox>

            <ArgonBox mt={4} display="flex" gap={2} justifyContent="center">
              <ArgonButton
                variant="gradient"
                color="info"
                onClick={handleRetry}
                disabled={!canRetry}
              >
                {canRetry ? t('authError.retry') : t('authError.retryIn', { seconds: countdown })}
              </ArgonButton>
              <ArgonButton
                variant="outlined"
                color="dark"
                onClick={handleGoBack}
              >
                {t('authError.goBack')}
              </ArgonButton>
            </ArgonBox>

            {!canRetry && (
              <ArgonBox mt={2}>
                <ArgonTypography variant="caption" color="text">
                  {t('authError.waitNote')}
                </ArgonTypography>
              </ArgonBox>
            )}
          </ArgonBox>
        </Card>
      </Container>
    </ArgonBox>
  );
};

export default ErrorPage;
