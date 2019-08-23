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
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace SolaceDotNetWrapper.Core
{
    internal class RequestReplyStateManager
    {
        private readonly ILogger<RequestReplyStateManager> logger;
        private readonly Dictionary<string, RequestContext> outstanding = new Dictionary<string, RequestContext>();

        public RequestReplyStateManager(ILogger<RequestReplyStateManager> logger)
        {
            this.logger = logger;
        }

        public void RegisterRequest(RequestContext ctx, int timeout)
        {
            Message reqMsg = ctx.Request;
            string correlationId = reqMsg.CorrelationId;
            if(correlationId == null)
                throw new ArgumentException("Request message should have CorrelationId set");
            if (reqMsg.ReplyTo != null)
                throw new ArgumentException("Request message should have ReplyTo address");
            if(reqMsg.Destination != null)
                throw new ArgumentException("Request message should have Destination");

            lock (outstanding)
            {
                if (outstanding.ContainsKey(correlationId))
                    throw new ArgumentException("Illegal CorrelationId: duplicate.");

                outstanding.Add(correlationId, ctx);
                ScheduleTimeout(ctx, timeout);
            }
        }

        public void CancelImmediately(RequestContext ctx)
        {
            Message reqMsg = ctx.Request;
            logger.LogDebug("Canceling request: {0}", reqMsg.CorrelationId);
            lock (outstanding)
            {
                CancelTimer(ctx);
                outstanding.Remove(reqMsg.CorrelationId);
            }
        }

        /// <summary>
        /// Handle an incoming message and indicate if caller should continue delivery
        /// (returns true) or stop delivery because we've handled the message (returns false).
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool HandleIncoming(Message msg)
        {
            if (!msg.IsReplyMessage)
            {
                // Not a response message, continue.
                return true; // continueProcessing
            }
            RequestContext ctx = null;
            lock (outstanding)
            {
                // Within the lock, we remove the RequestContext from the dictionary, we'll dispatch OUT of the lock.
                if (outstanding.TryGetValue(msg.CorrelationId, out ctx))
                {
                    outstanding.Remove(msg.CorrelationId);
                    CancelTimer(ctx);
                }
            }

            if (ctx != null)
            {
                logger.LogDebug("Reply message with CorrelationId '{0}', dispatching to request context.", msg.CorrelationId);
                ctx.Complete(msg, null);
                return false;
            }
            else
            {
                logger.LogDebug("Reply message received with unmatched CorrelationId '{0}', continue processing.", msg.CorrelationId);
                return true; //continueProcessing
            }
        }

        private void ScheduleTimeout(RequestContext ctx, int timeout)
        {
            var t = new Timer { AutoReset = false, Interval = timeout };
            t.Elapsed += (sender, e) =>
            {
                logger.LogWarning("Request operation timeout, correlation id:{0}, timeout:{1}", ctx.Request.CorrelationId, timeout);
                ctx.Complete(null, new MessagingException("Request with correlation ID: " + ctx.Request.CorrelationId + " timed out."));
            };
            ctx.Timer = t;
            t.Start();
        }

        private void CancelTimer(RequestContext ctx)
        {
            if (ctx.Timer != null)
            {
                ctx.Timer.Enabled = false;
                ctx.Timer.Close();
                ctx.Timer = null;
            }
        }
    }
}
