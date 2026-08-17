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
* Formats an ISO 8601 duration string (e.g., "PT2H30M15S") into a human-readable format.
* Returns the formatted duration string (e.g., "2 hr 30 min 15 s") or an empty string if the input is invalid.
*/
export function formatISODuration(value: string): string {
    const match = value.match(/PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?/);
    if (!match) return ""; // Return empty if format is invalid

    const parts: string[] = [];
    if (match[1]) parts.push(`${match[1]} hr`);
    if (match[2]) parts.push(`${match[2]} min`);
    if (match[3]) parts.push(`${match[3]} s`);

    return parts.join(" ");
}

/**
* Formats a timestamp into a time string in the format "HH:MM:SS".
* The value can be a number (milliseconds since epoch), a string, or a Date object.
*/
export function formatTime(value: number | string | Date): string {
    const date = new Date(value);
    return `${(date.getHours()).toString().padStart(2, '0')}:${(date.getMinutes()).toString().padStart(2, '0')}:${(date.getSeconds()).toString().padStart(2, '0')}`;
}
