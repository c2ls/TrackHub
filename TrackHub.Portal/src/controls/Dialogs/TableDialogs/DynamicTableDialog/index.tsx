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
import type { ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Dialog, DialogActions, DialogContent, DialogTitle } from '@mui/material';
import 'controls/Dialogs/styles/tableStyle.css';

interface TableColumn {
    field: string;
    headerName: ReactNode;
}

type TableRow = Record<string, ReactNode>;

interface DynamicTableDialogProps {
    title: ReactNode;
    children?: ReactNode;
    handleAdd: () => void | Promise<void>;
    handleDelete: (selectedRows: number[]) => void | Promise<void>;
    handleClose?: () => void | Promise<void>;
    open: boolean;
    data: TableRow[];
    columns: TableColumn[];
}

const DynamicTableDialog = ({
        title,
        children,
        handleAdd,
        handleDelete,
        handleClose,
        open,
        data,
        columns
    }: DynamicTableDialogProps) => {
    const { t } = useTranslation();

    const [selectedRows, setSelectedRows] = useState<number[]>([]);

    const handleRowSelection = (index: number) => {
        const alreadySelected = selectedRows.includes(index);
        const newSelectedRows = alreadySelected
            ? selectedRows.filter((i) => i !== index)
            : [...selectedRows, index];
        setSelectedRows(newSelectedRows);
    };

    const handleSelectAll = () => {
        if (selectedRows.length < data.length) {
            setSelectedRows(data.map((_, index) => index));
        } else {
            setSelectedRows([]);
        }
    };

    const onSave = async () => {
        await handleAdd();
    };

    const onDelete = async () => {
        await handleDelete(selectedRows);
    };

    const onClose = async () => {
        await handleClose?.();
    };

    return (
        <Dialog open={open} onClose={onClose}>
            <DialogTitle>{title}</DialogTitle>
                <DialogContent>
                    {children}
                    <div className="table-container">
                        <table className="table">
                            <thead>
                                <tr>
                                    <th className="th"><input type="checkbox" onChange={handleSelectAll} checked={selectedRows.length === data.length} /></th>
                                    {columns.map((column) => <th key={column.field} className="th">{column.headerName}</th>)}
                                </tr>
                            </thead>
                            <tbody>
                                {data.map((row, index) => (
                                    <tr key={index}>
                                        <td className="td"><input type="checkbox" checked={selectedRows.includes(index)} onChange={() => handleRowSelection(index)} /></td>
                                        {columns.map((column) => <td key={column.field} className="td">{row[column.field]}</td>)}
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>{t('generic.close')}</Button>
                <Button onClick={onDelete}>{t('generic.delete')}</Button>
                <Button onClick={onSave} autoFocus>{t('generic.add')}</Button>
            </DialogActions>
        </Dialog>
    );
};

export default DynamicTableDialog;
