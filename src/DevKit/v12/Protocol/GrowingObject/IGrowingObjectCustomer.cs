﻿//----------------------------------------------------------------------- 
// ETP DevKit, 1.2
//
// Copyright 2019 Energistics
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Energistics.Etp.Common;
using Energistics.Etp.Common.Datatypes;

namespace Energistics.Etp.v12.Protocol.GrowingObject
{
    /// <summary>
    /// Defines the interface that must be implemented by the customer role of the growing object protocol.
    /// </summary>
    /// <seealso cref="Energistics.Etp.Common.IProtocolHandler" />
    [ProtocolRole((int)Protocols.GrowingObject, "customer", "store")]
    public interface IGrowingObjectCustomer : IProtocolHandler
    {
        /// <summary>
        /// Gets the metadata for growing object parts.
        /// </summary>
        /// <param name="uris">The collection of growing object URIs.</param>
        /// <returns>The message identifier.</returns>
        long GetPartsMetadata(IList<string> uris);

        /// <summary>
        /// Gets a single list item in a growing object, by its ID.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <returns>The message identifier.</returns>
        long GetPart(string uri, string uid);

        /// <summary>
        /// Gets all list items in a growing object within an index range.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="uom">The unit of measure.</param>
        /// <param name="depthDatum">The depth datum.</param>
        /// <param name="includeOverlappingIntervals"><c>true</c> if overlapping intervals should be included; otherwise, <c>false</c>.</param>
        /// <returns>The message identifier.</returns>
        long GetPartsByRange(string uri, object startIndex, object endIndex, string uom, string depthDatum, bool includeOverlappingIntervals = false);

        /// <summary>
        /// Adds or updates a list item in a growing object.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <param name="contentType">The content type string for the parent object.</param>
        /// <param name="data">The data (list items) to be added to the growing object.</param>
        /// <returns>The message identifier.</returns>
        long PutPart(string uri, string uid, string contentType, byte[] data);

        /// <summary>
        /// Deletes one list item in a growing object.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <returns>The message identifier.</returns>
        long DeletePart(string uri, string uid);

        /// <summary>
        /// Deletes all list items in a range of index values.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="uom">The unit of measure.</param>
        /// <param name="depthDatum">The depth datum.</param>
        /// <param name="includeOverlappingIntervals"><c>true</c> if overlapping intervals should be included; otherwise, <c>false</c>.</param>
        /// <returns>The message identifier.</returns>
        long DeletePartsByRange(string uri, object startIndex, object endIndex, string uom, string depthDatum, bool includeOverlappingIntervals = false);

        /// <summary>
        /// Replaces a list item in a growing object.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <param name="contentType">The content type string for the parent object.</param>
        /// <param name="data">The data (list items) to be added to the growing object.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="uom">The unit of measure.</param>
        /// <param name="depthDatum">The depth datum.</param>
        /// <param name="includeOverlappingIntervals"><c>true</c> if overlapping intervals should be included; otherwise, <c>false</c>.</param>
        /// <returns>The message identifier.</returns>
        long ReplacePartsByRange(string uri, string uid, string contentType, byte[] data, object startIndex, object endIndex, string uom, string depthDatum, bool includeOverlappingIntervals);

        /// <summary>
        /// Handles the ObjectPart event from a store.
        /// </summary>
        event ProtocolEventHandler<ObjectPart> OnObjectPart;

        /// <summary>
        /// Handles the GetPartsMetadataResponse event from a store.
        /// </summary>
        event ProtocolEventHandler<GetPartsMetadataResponse> OnGetPartsMetadataResponse;
    }
}
