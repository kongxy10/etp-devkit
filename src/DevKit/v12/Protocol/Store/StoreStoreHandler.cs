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

using Energistics.Etp.Common;
using Energistics.Etp.Common.Datatypes;
using Energistics.Etp.v12.Datatypes.Object;

namespace Energistics.Etp.v12.Protocol.Store
{
    /// <summary>
    /// Base implementation of the <see cref="IStoreStore"/> interface.
    /// </summary>
    /// <seealso cref="Etp12ProtocolHandler" />
    /// <seealso cref="Energistics.Etp.v12.Protocol.Store.IStoreStore" />
    public class StoreStoreHandler : Etp12ProtocolHandler, IStoreStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoreStoreHandler"/> class.
        /// </summary>
        public StoreStoreHandler() : base((int)Protocols.Store, "store", "customer")
        {
            RegisterMessageHandler<GetDataObjects>(Protocols.Store, MessageTypes.Store.GetDataObjects, HandleGetDataObjects);
            RegisterMessageHandler<PutDataObjects>(Protocols.Store, MessageTypes.Store.PutDataObjects, HandlePutDataObjects);
            RegisterMessageHandler<DeleteDataObjects>(Protocols.Store, MessageTypes.Store.DeleteDataObjects, HandleDeleteDataObjects);
        }

        /// <summary>
        /// Sends an GetDataObjectsResponse message to a customer.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="messageFlag">The message flag.</param>
        /// <returns>The message identifier.</returns>
        public virtual long GetDataObjectsResponse(DataObject dataObject, long correlationId, MessageFlags messageFlag = MessageFlags.MultiPartAndFinalPart)
        {
            var header = CreateMessageHeader(Protocols.Store, MessageTypes.Store.GetDataObjectsResponse, correlationId, messageFlag);

            // TODO: Optimize reponse by sending multiple DataObject at a time
            var response = new GetDataObjectsResponse
            {
                DataObjects = new[] { dataObject }
            };

            return Session.SendMessage(header, response);
        }

        /// <summary>
        /// Handles the GetDataObjects event from a customer.
        /// </summary>
        public event ProtocolEventHandler<GetDataObjects, DataObject> OnGetDataObjects;

        /// <summary>
        /// Handles the PutDataObjects event from a customer.
        /// </summary>
        public event ProtocolEventHandler<PutDataObjects> OnPutDataObjects;

        /// <summary>
        /// Handles the DeleteDataObjects event from a customer.
        /// </summary>
        public event ProtocolEventHandler<DeleteDataObjects> OnDeleteDataObjects;

        /// <summary>
        /// Handles the GetDataObjects message from a customer.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="getDataObjects">The GetDataObjects message.</param>
        protected virtual void HandleGetDataObjects(IMessageHeader header, GetDataObjects getDataObjects)
        {
            var args = Notify(OnGetDataObjects, header, getDataObjects, new DataObject());
            HandleGetDataObjects(args);

            if (args.Cancel)
                return;

            if (args.Context.Data == null || args.Context.Data.Length == 0)
                GetDataObjectsResponse(args.Context, header.MessageId, MessageFlags.NoData);
            else
                GetDataObjectsResponse(args.Context, header.MessageId);
        }

        /// <summary>
        /// Handles the GetDataObjects message from a customer.
        /// </summary>
        /// <param name="args">The <see cref="ProtocolEventArgs{GetDataObjects, DataObject}"/> instance containing the event data.</param>
        protected virtual void HandleGetDataObjects(ProtocolEventArgs<GetDataObjects, DataObject> args)
        {
        }

        /// <summary>
        /// Handles the PutDataObjects message from a customer.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="putDataObjects">The PutDataObjects message.</param>
        protected virtual void HandlePutDataObjects(IMessageHeader header, PutDataObjects putDataObjects)
        {
            Notify(OnPutDataObjects, header, putDataObjects);
        }

        /// <summary>
        /// Handles the DeleteDataObjects message from a customer.
        /// </summary>
        /// <param name="header">The message header.</param>
        /// <param name="deleteDataObjects">The DeleteDataObjects message.</param>
        protected virtual void HandleDeleteDataObjects(IMessageHeader header, DeleteDataObjects deleteDataObjects)
        {
            Notify(OnDeleteDataObjects, header, deleteDataObjects);
        }
    }
}
