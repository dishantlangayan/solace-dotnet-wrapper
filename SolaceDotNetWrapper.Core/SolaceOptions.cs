//  ------------------------------------------------------------------------------------
// Copyright (c) Dishant Langayan
// All rights reserved. 
// 
// Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this 
// file except in compliance with the License. You may obtain a copy of the License at 
// http://www.apache.org/licenses/LICENSE-2.0  
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR 
// NON-INFRINGEMENT. 
// 
// See the Apache Version 2.0 License for specific language governing permissions and 
// limitations under the License.
//  ------------------------------------------------------------------------------------

using System;
using SolaceSystems.Solclient.Messaging;

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Options class for configuring connections, the Solace .NET API and the 
    /// wrapper API.
    /// </summary>
    public class SolaceOptions
    {
        // Basic connection properties
        public string Host { get; set; };
        public string MsgVpnName { get; set; } = "default";
        public string Username { get; set; } = "default";
        public string Password { get; set; }

        // Reconnect properties
        public int ReconnectRetries { get; set; } = 5;
        public int ConnectRetries { get; set; } = 1;
        public int ConnectRetriesPerHost { get; set; } = 20;
        public int ReconnectRetriesWaitInMs { get; set; } = 3000;

        // Misc Solace API properties
        // TODO: add support for other session props

        // Wrapper API Properties
        public string SolaceApiLogLevel { get; set; } = "Notice";

        public SessionProperties ToSessionProperties()
        {
            var sessionProps = new SessionProperties();
            sessionProps.Host = Host;
            sessionProps.VPNName = MsgVpnName;
            sessionProps.UserName = Username;
            sessionProps.Password = Password;
            sessionProps.ReconnectRetries = ReconnectRetries;
            sessionProps.ConnectRetries = ConnectRetries;
            sessionProps.ConnectRetriesPerHost = ConnectRetriesPerHost;
            sessionProps.ReconnectRetriesWaitInMsecs = ReconnectRetriesWaitInMs;
            return sessionProps;
        }
    }
}
