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
    /// Custom Messaging exception class for the SolaceDotNetWrapper API.
    /// </summary>
    public class MessagingException : Exception
    {
        /// <summary>
        ///     Empty Constructor.
        /// </summary>
        public MessagingException()
        {
        }

        /// <summary>
        ///     Constructor with message.
        /// </summary>
        /// <param name="message">Description of the exception. </param>
        public MessagingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructor with message and inner exception.
        /// </summary>
        /// <param name="message">Description of the exception. </param>
        /// <param name="inner">Inner Exception.</param>
        public MessagingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
