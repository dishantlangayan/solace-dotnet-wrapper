// //  ------------------------------------------------------------------------------------
// //  Copyright (c) Dishant Langayan
// //  All rights reserved. 
// //  
// //  Licensed under the Apache License, Version 2.0 (the ""License""); you may not use this 
// //  file except in compliance with the License. You may obtain a copy of the License at 
// //  http://www.apache.org/licenses/LICENSE-2.0  
// //  
// //  THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// //  EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// //  CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR 
// //  NON-INFRINGEMENT. 
// // 
// //  See the Apache Version 2.0 License for specific language governing permissions and 
// //  limitations under the License.
// //  ------------------------------------------------------------------------------------

using System;

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Enables an application to be notified when asynchronous connection events
    /// related to the client occur.For ex, a connection to the broker is lost, or
    /// the provider API is attempting to reconnect to the broker, etc.Note
    /// although, auto-reconnect will be built into the Messaging API, these events
    /// can be useful for notify the application that the underlying API is attempt
    /// to re-establish the connection or has exhausted its retry attempts.
    /// </summary>
    public class EventCallback
    {
        public EventCallback()
        {
        }
    }
}
