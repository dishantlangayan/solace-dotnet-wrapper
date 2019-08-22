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

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Class for correlating acks with published messages with persistent delivery mode.
    /// </summary>
    public class MessageCorrelationContext
    {
        public bool Acked { get; set; }
        public bool Accepted { get; set; }
        public readonly Message Message;
        public readonly string Id;

        public MessageCorrelationContext(Message message, string id)
        {
            Acked = false;
            Accepted = false;
            Message = message;
            Id = id;
        }
    }
}
