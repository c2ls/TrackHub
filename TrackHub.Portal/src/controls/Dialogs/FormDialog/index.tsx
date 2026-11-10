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

import { useState } from 'react';
import type { Dispatch, ReactNode, SetStateAction } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, DialogActions, DialogContent, DialogTitle, Drawer, Icon, IconButton } from '@mui/material';
import type { DialogProps } from '@mui/material';

interface FormDialogProps {
    title: ReactNode;
    children: ReactNode;
    handleSave: () => void | Promise<void>;
    open: boolean;
    setOpen: Dispatch<SetStateAction<boolean>>;
    handleCancel?: () => void | Promise<void>;
    maxWidth?: DialogProps['maxWidth'];
}

// Forms open as a panel on the right edge; `maxWidth` keeps its dialog meaning as a size hint.
const DRAWER_WIDTH: Record<string, number> = { xs: 420, sm: 560, md: 760, lg: 960, xl: 1200 };

const FormDialog = ({
        title,
        children,
        handleSave,
        open,
        setOpen,
        handleCancel = () => {},
        maxWidth = 'sm',
    }: FormDialogProps) => {
    const { t } = useTranslation();
    // A save in flight disables Save, so a double click cannot submit twice.
    const [saving, setSaving] = useState(false);
    const save = async () => {
        if (saving) return;
        setSaving(true);
        try {
            await handleSave();
        } finally {
            setSaving(false);
        }
    };
    const handleClose = () => {
        handleCancel();
        setOpen(false);
    };

    return (
        <Drawer
            anchor="right"
            open={open}
            // A click on the dimmed page does not dismiss a form: the click that finishes drawing
            // a shape lands there as the panel opens, and a stray one would discard typed input.
            onClose={(_event, reason) => {
                if (reason !== 'backdropClick') handleClose();
            }}
            data-testid="dialog-form"
            // Forms also open from inside dialogs, so the panel stacks with them, not below.
            sx={{ zIndex: (theme) => theme.zIndex.modal }}
            slotProps={{
                paper: {
                    role: 'dialog',
                    'aria-modal': true,
                    sx: {
                        width: DRAWER_WIDTH[maxWidth || 'sm'] ?? DRAWER_WIDTH.sm,
                        maxWidth: '100vw',
                        height: '100vh',
                        margin: 0,
                        borderRadius: 0,
                        backgroundColor: 'background.paper',
                        backgroundImage: 'none',
                    },
                },
            }}
        >
            <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
                {title}
                <IconButton size="small" onClick={handleClose} aria-label={t('generic.close')}>
                    <Icon fontSize="small">close</Icon>
                </IconButton>
            </DialogTitle>
            <DialogContent dividers sx={{ flex: 1, borderBottom: 0 }}>
                {children}
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose}>{t('generic.cancel')}</Button>
                <Button onClick={save} disabled={saving} autoFocus>{t('generic.save')}</Button>
            </DialogActions>
        </Drawer>
    );
};

export default FormDialog;
