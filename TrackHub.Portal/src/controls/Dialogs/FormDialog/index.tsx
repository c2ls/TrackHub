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

import type { Dispatch, ReactNode, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material';
import type { DialogProps } from '@mui/material';

interface FormDialogProps {
    title: ReactNode;
    children: ReactNode;
    handleSave: () => void | Promise<void>;
    open: boolean;
    setOpen: Dispatch<SetStateAction<boolean>>;
    handleCancel?: () => void | Promise<void>;
    maxWidth?: DialogProps['maxWidth'];
    fullWidth?: boolean;
}

const FormDialog = ({
        title,
        children,
        handleSave,
        open,
        setOpen,
        handleCancel = () => {},
        maxWidth = 'sm',
        fullWidth = true,
    }: FormDialogProps) => {
    const { t } = useTranslation();
    const handleClose = () => {
        handleCancel();
        setOpen(false);
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth={maxWidth} fullWidth={fullWidth}>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                {children}
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose}>{t('generic.cancel')}</Button>
                <Button onClick={handleSave} autoFocus>{t('generic.save')}</Button>
            </DialogActions>
        </Dialog>
    );
};

export default FormDialog;
