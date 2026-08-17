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

import { useState, useEffect } from 'react';
import type { ChangeEvent, ReactNode } from 'react';
import 'controls/Dialogs/styles/tableStyle.css';

interface CheckboxTableRow {
    value: string | number;
    name: string;
    label: ReactNode;
}

interface CheckboxTableColumn {
    value: string | number;
    name: ReactNode;
}

interface CheckboxTableDialogProps {
    children?: ReactNode;
    rows: CheckboxTableRow[];
    columns: CheckboxTableColumn[];
    data: Record<string, Record<string, unknown> | undefined>;
    title?: ReactNode;
    handleSave: (rowId: string, columnId: string, checked: boolean) => boolean | Promise<boolean>;
}

const CheckboxTableDialog = ({ children, rows, columns, data, title, handleSave }: CheckboxTableDialogProps) => {

    const [checkedState, setCheckedState] = useState<Record<string, boolean>>({});

    useEffect(() => {
        const initialState: Record<string, boolean> = {};
        rows.forEach(row => {
            columns.forEach(column => {
                const cell = data[row.value]?.[column.value];
                if (cell !== undefined) {
                    initialState[`${row.value}-${column.value}`] = true;
                }
            });
        });
        setCheckedState(initialState);
    }, [data, rows, columns]);

    const handleCheckboxChange = async (event: ChangeEvent<HTMLInputElement>) => {
        const { rowId, columnId } = event.target.dataset;
        const { checked } = event.target;
        if (rowId === undefined || columnId === undefined) {
            return;
        }
        const result = await handleSave(rowId, columnId, checked);
        if (result) {
            setCheckedState(prevState => ({
                ...prevState,
                [`${rowId}-${columnId}`]: checked,
            }));
        }
    };

    return (
        <>
            {children}
            <div className="table-container">
                <table className="table">
                    <thead>
                        <tr>
                            <th className="th">{title}</th>
                            {columns.map(column => (
                                <th key={column.value} className="th">{column.name}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {rows.map(row => (
                            <tr key={row.name}>
                                <td className="td">{row.label}</td>
                                {columns.map(column => {
                                    const isChecked = checkedState[`${row.value}-${column.value}`];
                                    return (
                                        <td key={column.value} className="td">
                                            <input
                                                type="checkbox"
                                                data-row-id={row.value}
                                                data-column-id={column.value}
                                                onChange={handleCheckboxChange}
                                                checked={!!isChecked}
                                            />
                                        </td>
                                    );
                                })}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </>
    );
}

export default CheckboxTableDialog;
