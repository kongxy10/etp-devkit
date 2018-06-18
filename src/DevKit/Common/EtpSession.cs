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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Avro.IO;
using Avro.Specific;
using Energistics.Datatypes;
using Energistics.Properties;
using Newtonsoft.Json.Linq;

namespace Energistics.Common
{
    /// <summary>
    /// Provides common functionality for all ETP sessions.
    /// </summary>
    /// <seealso cref="Energistics.Common.EtpBase" />
    /// <seealso cref="Energistics.Common.IEtpSession" />
    public abstract class EtpSession : EtpBase, IEtpSession
    {
        private long _messageId;
        private bool? _isJsonEncoding;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="EtpSession"/> class.
        /// </summary>
        /// <param name="application">The application name.</param>
        /// <param name="version">The application version.</param>
        /// <param name="headers">The WebSocket or HTTP headers.</param>
        protected EtpSession(string application, string version, IDictionary<string, string> headers)
        {
            Headers = headers ?? new Dictionary<string, string>();
            Handlers = new Dictionary<object, IProtocolHandler>();
            ApplicationName = application;
            ApplicationVersion = version;
            ValidateHeaders();
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName { get; }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <value>The application version.</value>
        public string ApplicationVersion { get; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>The session identifier.</value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is json encoding.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is json encoding; otherwise, <c>false</c>.
        /// </value>
        public bool IsJsonEncoding
        {
            get
            {
                if (!_isJsonEncoding.HasValue)
                {
                    string header;
                    Headers.TryGetValue(Settings.Default.EtpEncodingHeader, out header);
                    _isJsonEncoding = Settings.Default.EtpEncodingJson.Equals(header);
                }

                return _isJsonEncoding.Value;
            }
        }

        /// <summary>
        /// Gets the collection of WebSocket or HTTP headers.
        /// </summary>
        /// <value>The headers.</value>
        protected IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the collection of registered protocol handlers.
        /// </summary>
        /// <value>The handlers.</value>
        protected IDictionary<object, IProtocolHandler> Handlers { get; }

        /// <summary>
        /// Gets the registered protocol handler for the specified ETP interface.
        /// </summary>
        /// <typeparam name="T">The protocol handler interface.</typeparam>
        /// <returns>The registered protocol handler instance.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public T Handler<T>() where T : IProtocolHandler
        {
            IProtocolHandler handler;

            if (Handlers.TryGetValue(typeof(T), out handler) && handler is T)
            {
                return (T)handler;
            }

            Logger.Error(Log("[{0}] Protocol handler not registered for {1}.", SessionId, typeof(T).FullName));
            throw new NotSupportedException($"Protocol handler not registered for { typeof(T).FullName }.");
        }

        /// <summary>
        /// Determines whether this instance can handle the specified protocol.
        /// </summary>
        /// <typeparam name="T">The protocol handler interface.</typeparam>
        /// <returns>
        ///   <c>true</c> if the specified protocol handler has been registered; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle<T>() where T : IProtocolHandler
        {
            return Handlers.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Called when the ETP session is opened.
        /// </summary>
        /// <param name="requestedProtocols">The requested protocols.</param>
        /// <param name="supportedProtocols">The supported protocols.</param>
        public override void OnSessionOpened(IList<SupportedProtocol> requestedProtocols, IList<SupportedProtocol> supportedProtocols)
        {
            HandleUnsupportedProtocols(supportedProtocols);

            // notify protocol handlers about new session
            foreach (var item in Handlers)
            {
                if (item.Key is Type)
                    item.Value.OnSessionOpened(requestedProtocols, supportedProtocols);
            }
        }

        /// <summary>
        /// Called when the ETP session is closed.
        /// </summary>
        public override void OnSessionClosed()
        {
            // notify protocol handlers about closed session
            foreach (var item in Handlers)
            {
                if (item.Key is Type)
                    item.Value.OnSessionClosed();
            }
        }

        /// <summary>
        /// Called when WebSocket data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        public virtual void OnDataReceived(byte[] data)
        {
            Decode(data);
        }

        /// <summary>
        /// Called when a WebSocket message is received.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void OnMessageReceived(string message)
        {
            Decode(message);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header">The header.</param>
        /// <param name="body">The body.</param>
        /// <param name="onBeforeSend">Action called just before sending the message with the actual header having the definitive message ID.</param>
        /// <returns>The message identifier.</returns>
        public long SendMessage<T>(MessageHeader header, T body, Action<MessageHeader> onBeforeSend = null) where T : ISpecificRecord
        {
            try
            {
                // Wait for the send semaphore to ensure only 1 thread a time attempts to send a message.
                _sendLock.Wait();

                // Set the message after waiting for the semaphore to ensure all message IDs are sequential.
                header.MessageId = NewMessageId();

                // Call the pre-send action in case any deterministic handling is needed with the actual message ID.
                // Must be invoked before sending to ensure the response is not asynchronously processed before this method returns.
                onBeforeSend?.Invoke(header);

                if (IsJsonEncoding)
                {
                    var message = this.Serialize(new object[] {header, body});
                    Send(message);
                }
                else
                {
                    var data = body.Encode(header);
                    Send(data, 0, data.Length);
                }
                _sendLock.Release();
            }
            catch (Exception ex)
            {
                _sendLock.Release();
                return Handler(header.Protocol)
                    .ProtocolException((int) EtpErrorCodes.InvalidState, ex.Message, header.MessageId);
            }

            Sent(header, body);

            return header.MessageId;
        }

        /// <summary>
        /// Gets the supported protocols.
        /// </summary>
        /// <param name="isSender">if set to <c>true</c> the current session is the sender.</param>
        /// <returns>A list of supported protocols.</returns>
        public IList<SupportedProtocol> GetSupportedProtocols(bool isSender = false)
        {
            var supportedProtocols = new List<SupportedProtocol>();
            var version = new Datatypes.Version()
            {
                Major = 1,
                Minor = 1
            };

            // Skip Core protocol (0)
            foreach (var handler in Handlers.Values.Where(x => x.Protocol > 0))
            {
                var role = isSender ? handler.RequestedRole : handler.Role;

                if (supportedProtocols.Contains(handler.Protocol, role))
                    continue;

                supportedProtocols.Add(new SupportedProtocol()
                {
                    Protocol = handler.Protocol,
                    ProtocolVersion = version,
                    ProtocolCapabilities = handler.GetCapabilities(),
                    Role = role
                });
            }

            return supportedProtocols;
        }

        /// <summary>
        /// Generates a new unique message identifier for the current session.
        /// </summary>
        /// <returns>The message identifier.</returns>
        public long NewMessageId()
        {
            return Interlocked.Increment(ref _messageId);
        }

        /// <summary>
        /// Closes the WebSocket connection for the specified reason.
        /// </summary>
        /// <param name="reason">The reason.</param>
        public abstract void Close(string reason);

        /// <summary>
        /// Sends the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        protected abstract void Send(byte[] data, int offset, int length);

        /// <summary>
        /// Sends the specified messages.
        /// </summary>
        /// <param name="message">The message.</param>
        protected abstract void Send(string message);

        /// <summary>
        /// Decodes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        protected void Decode(byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            {
                // create avro binary decoder to read from memory stream
                var decoder = new BinaryDecoder(inputStream);
                // deserialize the header
                var header = decoder.Decode<MessageHeader>(null);

                // log message metadata
                if (Logger.IsDebugEnabled)
                {
                    Logger.DebugFormat("[{0}] Message received: {1}", SessionId, this.Serialize(header));
                }

                // call processing action
                HandleMessage(header, decoder, null);
            }
        }

        /// <summary>
        /// Decodes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void Decode(string message)
        {
            // split message header and body
            var array = JArray.Parse(message);
            var header = array[0].ToString();
            var body = array[1].ToString();

            // log message metadata
            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("[{0}] Message received: {1}", SessionId, header);
            }

            // call processing action
            HandleMessage(this.Deserialize<MessageHeader>(header), null, body);
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="decoder">The decoder.</param>
        /// <param name="body">The body.</param>
        protected void HandleMessage(MessageHeader header, Decoder decoder, string body)
        {
            if (Handlers.ContainsKey(header.Protocol))
            {
                var handler = Handler(header.Protocol);

                try
                {
                    handler.HandleMessage(header, decoder, body);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    handler.ProtocolException((int)EtpErrorCodes.InvalidState, ex.Message, header.MessageId);
                }
            }
            else
            {
                var message = $"Protocol handler not registered for protocol { header.Protocol }.";

                Handler((int)Protocols.Core)
                    .ProtocolException((int)EtpErrorCodes.UnsupportedProtocol, message, header.MessageId);
            }
        }

        /// <summary>
        /// Registers a protocol handler for the specified contract type.
        /// </summary>
        /// <param name="contractType">Type of the contract.</param>
        /// <param name="handlerType">Type of the handler.</param>
        protected override void Register(Type contractType, Type handlerType)
        {
            base.Register(contractType, handlerType);

            var handler = CreateInstance(contractType);

            if (handler != null)
            {
                handler.Session = this;
                Handlers[contractType] = handler;
                Handlers[handler.Protocol] = handler;
            }
        }

        /// <summary>
        /// Get the registered handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns>The registered protocol handler instance.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected IProtocolHandler Handler(int protocol)
        {
            if (Handlers.ContainsKey(protocol))
            {
                return Handlers[protocol];
            }

            Logger.Error(Log("[{0}] Protocol handler not registered for protocol {1}.", SessionId, protocol));
            throw new NotSupportedException($"Protocol handler not registered for protocol { protocol }.");
        }

        /// <summary>
        /// Handles the unsupported protocols.
        /// </summary>
        /// <param name="supportedProtocols">The supported protocols.</param>
        protected virtual void HandleUnsupportedProtocols(IList<SupportedProtocol> supportedProtocols)
        {
            // remove unsupported handler mappings (excluding Core protocol)
            Handlers
                .Where(x => x.Value.Protocol > 0 && !supportedProtocols.Contains(x.Value.Protocol, x.Value.Role))
                .ToList()
                .ForEach(x =>
                {
                    x.Value.Session = null;
                    Handlers.Remove(x.Key);
                    Handlers.Remove(x.Value.Protocol);
                });

            // update remaining handler mappings by protocol
            foreach (var handler in Handlers.Values.ToArray())
            {
                if (!Handlers.ContainsKey(handler.Protocol))
                    Handlers[handler.Protocol] = handler;
            }
        }

        /// <summary>
        /// Logs the specified header and message body.
        /// </summary>
        /// <typeparam name="T">The type of message.</typeparam>
        /// <param name="header">The header.</param>
        /// <param name="body">The message body.</param>
        protected void Sent<T>(MessageHeader header, T body)
        {
            if (Output != null)
            {
                Log("[{0}] Message sent at {1}", SessionId, DateTime.Now.ToString(TimestampFormat));
                Log(this.Serialize(header));
                Log(this.Serialize(body, true));
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.DebugFormat("[{0}] Message sent: {1}", SessionId, this.Serialize(header));
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources;
        ///     <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var handlers = Handlers
                    .Where(x => x.Key is int)
                    .Select(x => x.Value)
                    .OfType<IDisposable>();

                foreach (var handler in handlers)
                    handler.Dispose();

                _sendLock.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Validates the headers.
        /// </summary>
        protected virtual void ValidateHeaders()
        {
        }
    }
}
