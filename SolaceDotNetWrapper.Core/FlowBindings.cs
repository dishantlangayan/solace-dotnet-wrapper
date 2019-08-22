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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SolaceDotNetWrapper.Core.Utils;
using SolaceSystems.Solclient.Messaging;

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Internal class to keep track of Topic/Flow binding with a BufferBlock.
    /// Dispatches received messages to the appropriate BufferBlock assoicated with
    /// the corresponding Topic/Flow subscription. 
    /// </summary>
    internal class FlowBindings
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, FlowBindingElement> bindings = new Dictionary<string, FlowBindingElement>();

        public FlowBindings(ILogger<FlowBindings> logger)
        {
            this.logger = logger;
        }

        public bool AddBinding(Destination dest, BufferBlock<Message> messageQueue, out FlowBindingElement bind)
        {
            string key = MakeDestinationKey(dest);
            bool created = false;
            lock (bindings)
            {
                if (!bindings.ContainsKey(key))
                {
                    bindings.Add(key, new FlowBindingElement(dest, messageQueue, logger));
                    created = true;
                }
                bind = bindings[key];
            }
            return created;
        }

        public FlowBindingElement GetBinding(Destination dest)
        {
            string key = MakeDestinationKey(dest);
            lock (bindings)
            {
                if (bindings.ContainsKey(key))
                {
                    return bindings[key];
                }
            }

            return null;
        }

        public bool RemoveBinding(Destination dest, out FlowBindingElement bind)
        {
            string key = MakeDestinationKey(dest);
            bind = null;
            bool removed = false;
            lock (bindings)
            {
                if (bindings.TryGetValue(key, out bind))
                {
                    bindings.Remove(key);
                    removed = true;
                }
            }
            return removed;
        }

        private static string MakeDestinationKey(Destination dest)
        {
            if (dest is Queue)
                return "Q:" + dest.Name;
            else if (dest is Topic)
                return "T:" + dest.Name;
            else
                return "UNKNOWN:" + dest.Name;
        }


        /// <summary>
        /// Holds a single topic or queue-flow to the Solace broker and the buffer
        /// blocks that are subscribing to those destinations.
        /// </summary>
        public class FlowBindingElement
        {
            private readonly ILogger logger;

            public FlowBindingElement(Destination dest, BufferBlock<Message> messageQueue, ILogger logger)
            {
                Destination = dest;
                MessageQueue = messageQueue;
                this.logger = logger;
            }

            public Destination Destination { get; set; }
            public BufferBlock<Message> MessageQueue { get; set; }

            public IFlow Flow { get; set; }
            public IDispatchTarget TopicDispatchTarget { get; set; }

            private void ApplicationAck(long id)
            {
                if (Flow == null)
                    return;

                if (Flow.Properties.AckMode != MessageAckMode.AutoAck)
                {
                    ReturnCode rc = Flow.Ack(id);
                    if (rc != ReturnCode.SOLCLIENT_IN_PROGRESS && rc != ReturnCode.SOLCLIENT_OK)
                        throw new MessagingException("Ack failure: " + rc.ToString());
                }
                else
                {
                    logger.LogDebug("Application tried to ack a message but the flow has AutoAck enabled.");
                }
            }

            public async Task DispatchMessageAsync(Message msg)
            {
                var adMessageId = (long)msg.Headers[SolaceHeadersStr.AdMessageId];
                bool isAppAck = (msg is PersistentMessage && Flow != null && adMessageId != 0);

                if (isAppAck)
                {
                    // setup acknowledgement
                    ((PersistentMessage)msg).AckHandlerFunction = ApplicationAck;

                    // Make sure pre-dispatching, we expect our own ack, so that callbacks acking 
                    // during the dispatch don't cause an ack before we're done.
                    ((PersistentMessage)msg).IncrementExpectedAcks();
                }

                // Dispatch msg to app
                await MessageQueue.SendAsync(msg).ConfigureAwait(false);

                if (isAppAck)
                {
                    // Remove our own ack flag by decrementing.
                    ((PersistentMessage)msg).Acknowledge();
                }
            }
        }
    }
}