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
    public enum ConnectionState
    {
        Closed = 4,
        Closing = 3,
        Created = 0,
        Faulted = 5,
        Opened = 2,
        Opening = 1,
        Reconnected = 7,
        Reconnecting = 6
    }
}
