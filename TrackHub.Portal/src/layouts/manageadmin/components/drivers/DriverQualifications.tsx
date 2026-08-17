/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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
 * Per-driver qualification CRUD. Gated by the `workforce` feature — cosmetically
 * only; the backend commands carry the authoritative [RequireFeature] gate.
 */

import { useContext, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Icon from '@mui/material/Icon';
import Grid from '@mui/material/Grid';
import Table from 'controls/Tables/Table';
import TableAccordion from 'controls/Accordions/TableAccordion';
import ConfirmDialog from 'controls/Dialogs/ConfirmDialog';
import useForm from 'controls/Dialogs/useForm';
import ArgonBadge from 'components/ArgonBadge';
import ArgonBox from 'components/ArgonBox';
import ArgonButton from 'components/ArgonButton';
import ArgonTypography from 'components/ArgonTypography';
import QualificationDialog from 'layouts/manageadmin/components/drivers/QualificationDialog';
import type { QualificationFormValues } from 'layouts/manageadmin/components/drivers/QualificationDialog';
import DocumentPanel from 'layouts/manageadmin/components/documents/DocumentPanel';
import {
  DriverPicker,
  TextCell,
  expiryColor,
  statusColor,
} from 'layouts/manageadmin/components/drivers/workforceShared';
import {
  qualificationTypeLabel,
  qualificationStatusLabel,
} from 'layouts/manageadmin/components/drivers/qualificationConstants';
import { useAccountByUser } from 'queries/accounts';
import {
  useDriversByAccount,
  useDriverQualifications,
  useCreateDriverQualification,
  useUpdateDriverQualification,
  useDeleteDriverQualification,
} from 'queries/drivers';
import { useDocumentsForOwner, useInvalidateDocuments } from 'queries/documents';
import type { DriverQualification } from 'api/manager/drivers';
import { LoadingContext } from 'LoadingContext';
import { formatDateOnly } from 'utils/dateUtils';

/** Spec 09 §11: qualification evidence is a spec-04 Document owned by the driver. */
const DOCUMENT_OWNER_ENTITY_TYPE = 'Driver';

interface ConfirmState {
  open: boolean;
  id: string | null;
}

function ManageDriverQualifications() {
  const { t } = useTranslation();
  const { setLoading } = useContext(LoadingContext);
  const [expanded, setExpanded] = useState(false);
  const [driverId, setDriverId] = useState('');
  const [open, setOpen] = useState(false);
  const [confirm, setConfirm] = useState<ConfirmState>({ open: false, id: null });
  const [values, handleChange, setValues, setErrors, validate, errors] =
    useForm<QualificationFormValues>({});

  const accountQuery = useAccountByUser({ enabled: expanded });
  const accountId = accountQuery.data?.accountId;
  const driversQuery = useDriversByAccount(accountId, { enabled: expanded && !!accountId });
  const drivers = driversQuery.data ?? [];

  const hasDriver = !!driverId;
  const qualificationsQuery = useDriverQualifications(accountId, driverId || null, null, {
    enabled: expanded && hasDriver,
  });
  const qualifications = qualificationsQuery.data ?? [];

  // The driver's spec-04 documents feed the dialog's document picker; the
  // embedded panel below uploads into the same owner scope.
  const documentsQuery = useDocumentsForOwner(accountId, DOCUMENT_OWNER_ENTITY_TYPE, driverId || null, {
    enabled: expanded && hasDriver,
  });
  const documents = documentsQuery.data ?? [];
  const invalidateDocuments = useInvalidateDocuments();

  const createQualification = useCreateDriverQualification();
  const updateQualification = useUpdateDriverQualification();
  const deleteQualification = useDeleteDriverQualification();

  useEffect(() => {
    setLoading(qualificationsQuery.isFetching || driversQuery.isFetching);
  }, [qualificationsQuery.isFetching, driversQuery.isFetching, setLoading]);

  const handleAddClick = () => {
    setValues({ status: 'Valid' });
    setErrors({});
  };

  const handleEdit = (qualification: DriverQualification) => {
    setValues({
      driverQualificationId: qualification.driverQualificationId,
      qualificationType: qualification.qualificationType,
      category: qualification.category,
      number: qualification.number,
      issuedAt: qualification.issuedAt,
      expiresAt: qualification.expiresAt,
      issuingAuthority: qualification.issuingAuthority,
      status: qualification.status,
      documentId: qualification.documentId,
      notes: qualification.notes,
    });
    setErrors({});
    setOpen(true);
  };

  const submit = async () => {
    if (!validate(['qualificationType', 'status']) || !accountId || !driverId) return;
    setLoading(true);
    try {
      // validate() + the id guards above ensure the required dto fields exist.
      const qualification = {
        accountId,
        driverId,
        qualificationType: values.qualificationType as string,
        category: values.category ?? null,
        number: values.number ?? null,
        issuedAt: values.issuedAt ?? null,
        expiresAt: values.expiresAt ?? null,
        issuingAuthority: values.issuingAuthority ?? null,
        status: values.status as string,
        documentId: values.documentId ?? null,
        notes: values.notes ?? null,
      };
      if (values.driverQualificationId) {
        await updateQualification.mutateAsync({
          driverQualificationId: values.driverQualificationId,
          qualification,
        });
      } else {
        await createQualification.mutateAsync(qualification);
      }
      setOpen(false);
    } catch {
      // Failure is surfaced by the global toast; keep the dialog open.
    } finally {
      setLoading(false);
    }
  };

  const doDelete = async () => {
    const id = confirm.id;
    setConfirm({ open: false, id: null });
    if (!id) return;
    setLoading(true);
    try {
      await deleteQualification.mutateAsync(id);
    } catch {
      // Failure is surfaced by the global toast.
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <TableAccordion
        title={t('workforce.qualifications.title')}
        expanded={expanded}
        showAddIcon={hasDriver}
        setOpen={setOpen}
        handleAddClick={handleAddClick}
        setExpanded={setExpanded}
      >
        <ArgonBox mb={2}>
          <Grid container spacing={1} sx={{ alignItems: "center" }}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DriverPicker
                id="qualificationDriverId"
                drivers={drivers}
                value={driverId}
                onChange={setDriverId}
                label={t('workforce.selectDriver')}
                placeholder={t('workforce.selectDriverPlaceholder')}
              />
            </Grid>
          </Grid>
        </ArgonBox>

        {!hasDriver ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.selectDriverHint')}
          </ArgonTypography>
        ) : qualifications.length === 0 ? (
          <ArgonTypography variant="caption" color="secondary">
            {t('workforce.qualifications.empty')}
          </ArgonTypography>
        ) : (
          <Table
            columns={[
              { name: 'type', title: t('workforce.qualifications.type'), align: 'left' },
              { name: 'category', title: t('workforce.qualifications.category'), align: 'center' },
              { name: 'number', title: t('workforce.qualifications.number'), align: 'center' },
              { name: 'issuedAt', title: t('workforce.qualifications.issuedAt'), align: 'center' },
              { name: 'expiresAt', title: t('workforce.qualifications.expiresAt'), align: 'center' },
              {
                name: 'authority',
                title: t('workforce.qualifications.issuingAuthority'),
                align: 'center',
              },
              { name: 'status', title: t('workforce.qualifications.status'), align: 'center' },
              { name: 'document', title: t('workforce.qualifications.documentId'), align: 'center' },
              { name: 'action', title: t('generic.action'), align: 'center' },
              { name: 'id' },
            ]}
            rows={qualifications.map((qualification) => ({
              type: (
                <ArgonTypography variant="caption" fontWeight="medium">
                  {qualificationTypeLabel(t, qualification.qualificationType)}
                </ArgonTypography>
              ),
              category: <TextCell>{qualification.category}</TextCell>,
              number: <TextCell>{qualification.number}</TextCell>,
              issuedAt: <TextCell>{formatDateOnly(qualification.issuedAt)}</TextCell>,
              expiresAt: (
                <ArgonBadge
                  badgeContent={formatDateOnly(qualification.expiresAt) || '-'}
                  color={expiryColor(qualification.expiresAt)}
                  size="xs"
                  container
                />
              ),
              authority: <TextCell>{qualification.issuingAuthority}</TextCell>,
              status: (
                <ArgonBadge
                  badgeContent={qualificationStatusLabel(t, qualification.status)}
                  color={statusColor(qualification.status)}
                  size="xs"
                  container
                />
              ),
              document: (
                <TextCell>
                  {qualification.documentId
                    ? (documents.find((doc) => doc.documentId === qualification.documentId)?.title ??
                      documents.find((doc) => doc.documentId === qualification.documentId)
                        ?.fileName ??
                      t('workforce.qualifications.documentUnavailable'))
                    : ''}
                </TextCell>
              ),
              action: (
                <>
                  <ArgonButton variant="text" color="dark" onClick={() => handleEdit(qualification)}>
                    <Icon>edit</Icon>&nbsp;{t('generic.edit')}
                  </ArgonButton>
                  <ArgonButton
                    variant="text"
                    color="error"
                    onClick={() =>
                      setConfirm({ open: true, id: qualification.driverQualificationId })
                    }
                  >
                    <Icon>delete</Icon>&nbsp;{t('generic.delete')}
                  </ArgonButton>
                </>
              ),
              id: qualification.driverQualificationId,
            }))}
            selectedField="type"
          />
        )}

        {/* Spec 09 §8/§11: the spec-04 document component, scoped to this driver.
            Evidence uploaded here becomes selectable in the qualification dialog. */}
        {hasDriver && (
          <>
            <ArgonBox mt={3} mb={1}>
              <ArgonTypography
                variant="button"
                fontWeight="bold"
                textTransform="uppercase"
                color="text"
              >
                {t('workforce.qualifications.documents')}
              </ArgonTypography>
            </ArgonBox>
            <DocumentPanel
              accountId={accountId}
              ownerEntityType={DOCUMENT_OWNER_ENTITY_TYPE}
              ownerEntityId={driverId}
              onDocumentsChanged={invalidateDocuments}
            />
          </>
        )}
      </TableAccordion>

      <QualificationDialog
        open={open}
        setOpen={setOpen}
        handleSubmit={submit}
        values={values}
        handleChange={handleChange}
        errors={errors}
        documents={documents}
      />
      <ConfirmDialog
        open={confirm.open}
        setOpen={(next) =>
          setConfirm((prev) => ({
            ...prev,
            open: typeof next === 'function' ? next(prev.open) : next,
          }))
        }
        title={t('workforce.qualifications.title')}
        message={t('workforce.qualifications.deleteConfirm')}
        onConfirm={doDelete}
      />
    </>
  );
}

export default ManageDriverQualifications;
