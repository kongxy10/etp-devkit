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

using Avro.Specific;
using Energistics.Etp.Common.Datatypes;

namespace Energistics.Etp.Common
{
    /// <summary>
    /// Represents the method that will handle a protocol handler event.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ProtocolEventArgs{T}"/> instance containing the event data.</param>
    public delegate void ProtocolEventHandler<T>(object sender, ProtocolEventArgs<T> e) where T : ISpecificRecord;

    /// <summary>
    /// Represents the method that will handle a protocol handler event.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <typeparam name="TErrorInfo">The type of error info.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ProtocolEventWithErrorsArgs{T, E}"/> instance containing the event data.</param>
    public delegate void ProtocolEventWithErrorsHandler<T, TErrorInfo>(object sender, ProtocolEventWithErrorsArgs<T, TErrorInfo> e) where T : ISpecificRecord;

    /// <summary>
    /// Represents the method that will handle a protocol handler event.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ProtocolEventArgs{T, V}"/> instance containing the event data.</param>
    public delegate void ProtocolEventHandler<T, TContext>(object sender, ProtocolEventArgs<T, TContext> e) where T : ISpecificRecord;

    /// <summary>
    /// Represents the method that will handle a protocol handler event.
    /// </summary>
    /// <typeparam name="T">The type of the message.</typeparam>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TErrorInfo">The type of error info.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ProtocolEventWithErrorsArgs{T, V, E}"/> instance containing the event data.</param>
    public delegate void ProtocolEventWithErrorsHandler<T, TContext, TErrorInfo>(object sender, ProtocolEventWithErrorsArgs<T, TContext, TErrorInfo> e)
        where T : ISpecificRecord
        where TErrorInfo : IErrorInfo;
}
