﻿//----------------------------------------------------------------------- 
// ETP DevKit, 1.1
//
// Copyright 2016 Energistics
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

using Avro.IO;
using Energistics.Common;
using Energistics.Datatypes;
using Energistics.Datatypes.Object;

namespace Energistics.Protocol.GrowingObject
{
    /// <summary>
    /// Base implementation of the <see cref="IGrowingObjectCustomer"/> interface.
    /// </summary>
    /// <seealso cref="Energistics.Common.EtpProtocolHandler" />
    /// <seealso cref="Energistics.Protocol.GrowingObject.IGrowingObjectCustomer" />
    public class GrowingObjectCustomerHandler : EtpProtocolHandler, IGrowingObjectCustomer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrowingObjectCustomerHandler"/> class.
        /// </summary>
        public GrowingObjectCustomerHandler() : base(Protocols.GrowingObject, "customer", "store")
        {
        }

        /// <summary>
        /// Gets a single list item in a growing object, by its ID.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <returns>The message identifier.</returns>
        public long GrowingObjectGet(string uri, string uid)
        {
            var header = CreateMessageHeader(Protocols.GrowingObject, MessageTypes.GrowingObject.GrowingObjectGet);

            var message = new GrowingObjectGet
            {
                Uri = uri,
                Uid = uid
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Gets all list items in a growing object within an index range.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <param name="uom">The unit of measure.</param>
        /// <param name="depthDatum">The depth datum.</param>
        /// <returns>The message identifier.</returns>
        public long GrowingObjectGetRange(string uri, object startIndex, object endIndex, string uom, string depthDatum)
        {
            var header = CreateMessageHeader(Protocols.GrowingObject, MessageTypes.GrowingObject.GrowingObjectGetRange);

            var message = new GrowingObjectGetRange
            {
                Uri = uri,
                StartIndex = new GrowingObjectIndex { Item = startIndex },
                EndIndex = new GrowingObjectIndex { Item = endIndex },
                Uom = uom,
                DepthDatum = depthDatum
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Adds or updates a list item in a growing object.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="contentType">The content type string for the parent object.</param>
        /// <param name="data">The data (list items) to be added to the growing object.</param>
        /// <returns>The message identifier.</returns>
        public long GrowingObjectPut(string uri, string contentType, byte[] data)
        {
            var header = CreateMessageHeader(Protocols.GrowingObject, MessageTypes.GrowingObject.GrowingObjectPut);

            var message = new GrowingObjectPut
            {
                Uri = uri,
                ContentType = contentType,
                ContentEncoding = "text/xml",
                Data = data
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Deletes one list item in a growing object.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="uid">The ID of the element within the list.</param>
        /// <returns>The message identifier.</returns>
        public long GrowingObjectDelete(string uri, string uid)
        {
            var header = CreateMessageHeader(Protocols.GrowingObject, MessageTypes.GrowingObject.GrowingObjectDelete);

            var message = new GrowingObjectDelete
            {
                Uri = uri,
                Uid = uid
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Deletes all list items in a range of index values. Range is inclusive of the limits.
        /// </summary>
        /// <param name="uri">The URI of the parent object.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>The message identifier.</returns>
        public long GrowingObjectDeleteRange(string uri, object startIndex, object endIndex)
        {
            var header = CreateMessageHeader(Protocols.GrowingObject, MessageTypes.GrowingObject.GrowingObjectDeleteRange);

            var message = new GrowingObjectDeleteRange
            {
                Uri = uri,
                StartIndex = new GrowingObjectIndex { Item = startIndex },
                EndIndex = new GrowingObjectIndex { Item = endIndex }
            };

            return Session.SendMessage(header, message);
        }

        /// <summary>
        /// Handles the ObjectFragment event from a store.
        /// </summary>
        public event ProtocolEventHandler<ObjectFragment> OnObjectFragment;

        /// <summary>
        /// Decodes the message based on the message type contained in the specified <see cref="MessageHeader" />.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="decoder">The message decoder.</param>
        /// <param name="body">The message body.</param>
        protected override void HandleMessage(MessageHeader header, Decoder decoder, string body)
        {
            switch (header.MessageType)
            {
                case (int)MessageTypes.GrowingObject.ObjectFragment:
                    HandleObjectFragment(header, decoder.Decode<ObjectFragment>(body));
                    break;

                default:
                    base.HandleMessage(header, decoder, body);
                    break;
            }
        }

        /// <summary>
        /// Handles the ObjectFragment message from a store.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="message">The ObjectFragment message.</param>
        protected virtual void HandleObjectFragment(MessageHeader header, ObjectFragment message)
        {
            Notify(OnObjectFragment, header, message);
        }
    }
}
