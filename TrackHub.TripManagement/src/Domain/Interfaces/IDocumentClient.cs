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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Reads document metadata from Manager to validate POD attachments. This module introduces no
/// storage surface of its own — POD signatures and photos are ordinary Manager <c>Document</c>
/// records created through the existing REST upload and linked here by id (spec 11 §11).
/// </summary>
public interface IDocumentClient
{
    /// <summary>
    /// Returns the document's account and scan status, or null when it does not exist.
    /// The caller rejects anything that is not in the trip's account or not
    /// <c>ScanStatus = Clean</c> (<c>POD_DOCUMENT_NOT_CLEAN</c>).
    /// </summary>
    Task<DocumentStateVm?> GetDocumentStateAsync(Guid documentId, CancellationToken cancellationToken);
}

public readonly record struct DocumentStateVm(Guid DocumentId, Guid AccountId, string ScanStatus, string Status);
