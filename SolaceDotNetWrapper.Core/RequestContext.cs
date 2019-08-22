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
using System.Threading.Tasks;
using System.Timers;

namespace SolaceDotNetWrapper.Core
{
    public class RequestContext
    {
        private readonly Message outMessage;
        private volatile Message respMessage;

        private volatile Timer timer;

        private TaskCompletionSource<Message> taskCompletion;

        public RequestContext(Message outMessage, TaskCompletionSource<Message> tcs)
        {
            this.outMessage = outMessage;
            this.taskCompletion = tcs;
        }

        public Message Response
        {
            get { return respMessage; }
        }

        public Message Request
        {
            get { return outMessage; }
        }

        public Timer Timer
        {
            get { return timer; }
            set { timer = value; }
        }

        public void Complete(Message response, Exception ex)
        {
            // setup the state BEFORE calling the async handler
            respMessage = response;

            if (ex != null)
                taskCompletion.TrySetException(ex);
            else
                taskCompletion.TrySetResult(response);
        }
    }
}
