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

import { useEffect, useContext, useState } from "react";
import Card from "@mui/material/Card";
import ArgonButton from "components/ArgonButton";
import Icon from "@mui/material/Icon";
import CustomTextField from 'controls/Dialogs/CustomTextField';
import PasswordChangeForm from 'layouts/profile/components/Password';
import type { PasswordFormValues } from 'layouts/profile/components/Password';
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import { LoadingContext } from 'LoadingContext';
import { useTranslation } from 'react-i18next';
import useForm from 'controls/Dialogs/useForm';
import type { CurrentUser } from "api/security/users";

interface ProfileInfoCardProps {
  user: CurrentUser;
  updateCurrentUser: (values: Partial<CurrentUser>) => Promise<boolean>;
  updatePassword: (userId: string, userData: PasswordFormValues) => Promise<boolean>;
}

function ProfileInfoCard({ user, updateCurrentUser, updatePassword }: ProfileInfoCardProps) {
  const { setLoading } = useContext(LoadingContext);
  const { t } = useTranslation();
  const [openPassword, setOpenPassword] = useState(false);
  const [values, handleChange, setValues, setErrors, validate, errors] = useForm<Partial<CurrentUser>>({});
  const [passwordValues, handlePasswordChange, setPasswordValues, setPasswordErrors, validatePassword, passwordErrors, validateMatch] = useForm<PasswordFormValues>({});

  useEffect(() => {
    const fetchUserInfo = async () => {
      setValues(user);
      setErrors({});
    };

    fetchUserInfo();
  }, [user]);

  async function onSaveUser() {
    setLoading(true);
    let requiredFields = ['firstName', 'lastName'];

    if (validate(requiredFields)) {
        await updateCurrentUser(values);
    }
    setLoading(false);
  }

  const handleUpdatePasswordClick = () => {
    setPasswordValues({});
    setPasswordErrors({});
    setOpenPassword(true);
  };

  const handleSubmitPassword = async () => {
    setLoading(true);
    if (validatePassword(['password', 'confirmPassword']) && validateMatch('password', 'confirmPassword')) {
      passwordValues.userId = user.userId;
      await updatePassword(user.userId, passwordValues);
      setOpenPassword(false);
    }
    setLoading(false);
  };

  return (
    <>
      <Card sx={{ height: "100%" }}>
        <ArgonBox display="flex" justifyContent="space-between" alignItems="center" pt={2} px={2}>
          <ArgonTypography variant="h6" fontWeight="medium" textTransform="capitalize">
            {t('userprofile.info')}
          </ArgonTypography>
          <Icon
            sx={{ fontSize: 24, cursor: "pointer" }}
            onClick={handleUpdatePasswordClick}>
              lock
            </Icon>
        </ArgonBox>
        <ArgonBox p={2}>
          <form>
              <ArgonBox key="username" display="flex" py={1} pr={2}>
                  <ArgonTypography variant="button" fontWeight="bold" textTransform="capitalize">
                      {t('user.username')}:
                  </ArgonTypography>
                  <ArgonTypography variant="button" fontWeight="regular" color="text">
                      &nbsp;{user.username}
                  </ArgonTypography>
              </ArgonBox>
              <ArgonBox key="emailaddress" display="flex" py={1} pr={2}>
                  <ArgonTypography variant="button" fontWeight="bold" textTransform="capitalize">
                      {t('user.emailAddress')}:
                  </ArgonTypography>
                  <ArgonTypography variant="button" fontWeight="regular" color="text">
                      &nbsp;{user.emailAddress}
                  </ArgonTypography>
              </ArgonBox>
              <CustomTextField
                  name="firstName"
                  id="firstName"
                  label={t('user.firstName')}
                  type="text"
                  fullWidth
                  value={values.firstName || ''}
                  onChange={handleChange}
                  errorMsg={errors.firstName}
                  required
              />

              <CustomTextField
                  name="secondName"
                  id="secondName"
                  label={t('user.secondName')}
                  type="text"
                  fullWidth
                  value={values.secondName || ''}
                  onChange={handleChange}
              />

              <CustomTextField
                  name="lastName"
                  id="lastName"
                  label={t('user.lastName')}
                  type="text"
                  fullWidth
                  value={values.lastName || ''}
                  onChange={handleChange}
                  errorMsg={errors.lastName}
                  required
              />

              <CustomTextField
                  name="secondSurname"
                  id="secondSurname"
                  label={t('user.secondSurname')}
                  type="text"
                  fullWidth
                  value={values.secondSurname || ''}
                  onChange={handleChange}
              />

              <CustomTextField
                  name="dob"
                  id="dob"
                  label={t('user.dob')}
                  type="date"
                  fullWidth
                  value={values.dob || ''}
                  onChange={handleChange}
              />
          </form>

        </ArgonBox>
        <ArgonButton
            variant="gradient"
            onClick={onSaveUser}
            color="dark">
            <Icon sx={{ fontWeight: "bold" }}>save</Icon>
            &nbsp;{t('generic.save')}
          </ArgonButton>
      </Card>
      <PasswordChangeForm
        open={openPassword}
        setOpen={setOpenPassword}
        handleSubmit={handleSubmitPassword}
        values={passwordValues}
        handleChange={handlePasswordChange}
        errors={passwordErrors}
      />
    </>
  );
}

export default ProfileInfoCard;
