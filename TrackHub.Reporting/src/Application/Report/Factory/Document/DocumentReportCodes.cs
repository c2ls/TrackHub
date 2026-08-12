// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

namespace TrackHub.Reporting.Application.Report.Factory.Document;

// Report codes for the document reports. Local literals (no TrackHubCommon repack).
internal static class DocumentReportCodes
{
    public const string ExpiringDocuments = "documents-expiring";
    public const string MissingRequiredDocuments = "documents-missing-required";
    public const string ShareActivity = "documents-share-activity";
    public const string UploadVolume = "documents-upload-volume";
}
