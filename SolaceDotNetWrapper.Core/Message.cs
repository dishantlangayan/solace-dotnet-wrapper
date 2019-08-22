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
using System.Collections.ObjectModel;
using System.Text;

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Create a general Solace message with wrapper API default settigns.
    /// </summary>
    public abstract class Message
    {
        private readonly Lazy<Dictionary<string, object>> _lzHeaders = new Lazy<Dictionary<string, object>>(false);
        private ArraySegment<byte> _body;

        public Message(bool isPersistent, bool elidingEligible, bool dmqEligible)
        {
            this.IsPersistent = isPersistent;
            this.ElidingEligible = elidingEligible;
            this.DMQEligible = dmqEligible;
        }

        public bool IsPersistent { get; }

        public Destination ReplyTo { get; set; }

        //
        // Solace specific message properties
        //

        public bool AckImmediately { get; set; }

        public long AdMessageId { get; set; }

        public string ApplicationMessageId { get; set; }

        public string ApplicationMessageType { get; set; }

        public long CacheRequestId { get; set; }

        public string CorrelationId { get; set; }

        public Destination Destination { get; set; }

        public bool DiscardIndication { get; set; }

        public bool DMQEligible { get; set; }

        public bool ElidingEligible { get; set; }

        public bool IsReplyMessage { get; set; }

        public long ReceiverTimestamp { get; set; }

        public bool Redelivered { get; set; }

        public string SenderId { get; set; }

        public long SenderTimestamp { get; set; }

        public long TimeToLive { get; set; }

        public byte[] UserData { get; set; }

        public IDictionary<string, object> AppHeaders
        {
            get
            {
                return _lzHeaders.Value;
            }
        }

        public bool AppHeadersPresent
        {
            get { return (_lzHeaders.IsValueCreated && _lzHeaders.Value.Count > 0); }
        }

        public string Body
        {
            get
            {
                if (_body.Count == 0)
                    return null;
                else
                    return Encoding.UTF8.GetString(_body.Array, _body.Offset, _body.Count);
            }
            set
            {
                if (value == null)
                    _body = new ArraySegment<byte>();
                else
                    _body = new ArraySegment<byte>(Encoding.UTF8.GetBytes(value));
            }
        }

        public ArraySegment<byte> BodyAsBytes
        {
            get => _body;
            set => _body = value;
        }

        public abstract void Acknowledge();
    }
}
