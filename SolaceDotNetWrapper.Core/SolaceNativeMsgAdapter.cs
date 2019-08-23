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
using SolaceSystems.Solclient.Messaging;
using SolaceSystems.Solclient.Messaging.SDT;

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Helper class to convert Solace native messages to wrapper api messages
    /// and vice versa.
    /// </summary>
    internal class SolaceNativeMsgAdapter
    {
        public static IMessage ConvertToNativeMsg(Message message)
        {
            IMessage solaceMsg = ContextFactory.Instance.CreateMessage();

            // Payload - msg body
            if (message.BodyAsBytes.Count > 0)
            {
                sbyte[] body;
                if (message.BodyAsBytes.Offset == 0 && message.BodyAsBytes.Count == message.BodyAsBytes.Array.Length)
                    body = (sbyte[])((Array)message.BodyAsBytes.Array);
                else
                {
                    body = new sbyte[message.BodyAsBytes.Count];
                    Buffer.BlockCopy(message.BodyAsBytes.Array, message.BodyAsBytes.Offset, body, 0, message.BodyAsBytes.Count);
                }
                solaceMsg.SetBinaryAttachment(body);
            }

            // Destination
            solaceMsg.Destination = (message.Destination is Topic) ?
                (IDestination)ContextFactory.Instance.CreateTopic(message.Destination.Name) :
                ContextFactory.Instance.CreateQueue(message.Destination.Name);

            // Delivery Mode
            solaceMsg.DeliveryMode = (message.IsPersistent) ? MessageDeliveryMode.Persistent : MessageDeliveryMode.Direct;

            // Reply Message
            if (message.IsReplyMessage)
                solaceMsg.SetAsReplyMessage();

            // Request message return destination
            if (message.ReplyTo != null)
                solaceMsg.ReplyTo = ConvertToSolaceDest(message.ReplyTo);

            // Application Headers
            if (message.AppHeadersPresent)
                ConvertAppHeaders(message.AppHeaders, solaceMsg);

            // Other Solace message properties
            solaceMsg.AckImmediately = message.AckImmediately;
            solaceMsg.ApplicationMessageId = message.ApplicationMessageId;
            solaceMsg.ApplicationMessageType = message.ApplicationMessageType;
            solaceMsg.CorrelationId = message.CorrelationId;
            solaceMsg.DMQEligible = message.DMQEligible;
            solaceMsg.ElidingEligible = message.ElidingEligible;
            solaceMsg.SenderId = message.SenderId;
            solaceMsg.TimeToLive = message.TimeToLive;
            solaceMsg.UserData = message.UserData;

            return solaceMsg;
        }

        public static Message ConvertFromNativeMsg(IMessage solNativeMsg)
        {
            var message = (solNativeMsg.DeliveryMode == MessageDeliveryMode.Persistent ||
                solNativeMsg.DeliveryMode == MessageDeliveryMode.NonPersistent) ?
                (Message)new PersistentMessage() : (Message)new NonPersistentMessage();
            message.Destination = ConvertFromSolaceDest(solNativeMsg.Destination);

            // Copy binary contents
            sbyte[] bodyBytes = solNativeMsg.GetBinaryAttachment();
            if (bodyBytes != null)
                message.BodyAsBytes = new ArraySegment<byte>((byte[])((Array)bodyBytes));

            // Request-reply
            message.ReplyTo = ConvertFromSolaceDest(solNativeMsg.ReplyTo);

            // Other Solace message properties
            message.AckImmediately = solNativeMsg.AckImmediately;
            message.AdMessageId = solNativeMsg.ADMessageId;
            message.ApplicationMessageId = solNativeMsg.ApplicationMessageId;
            message.ApplicationMessageType = solNativeMsg.ApplicationMessageType;
            message.CacheRequestId = solNativeMsg.CacheRequestId;
            message.CorrelationId = solNativeMsg.CorrelationId;
            message.DiscardIndication = solNativeMsg.DiscardIndication;
            message.DMQEligible = solNativeMsg.DMQEligible;
            message.ElidingEligible = solNativeMsg.ElidingEligible;
            message.IsReplyMessage = solNativeMsg.IsReplyMessage;
            message.ReceiverTimestamp = solNativeMsg.ReceiverTimestamp;
            message.Redelivered = solNativeMsg.Redelivered;
            message.SenderId = solNativeMsg.SenderId;
            message.SenderTimestamp = solNativeMsg.SenderTimestamp;
            message.TimeToLive = solNativeMsg.TimeToLive;
            message.UserData = solNativeMsg.UserData;

            // Copy All headers (application specific)
            BuildHeaderMap(solNativeMsg, message.AppHeaders);

            return message;
        }

        private static IDestination ConvertToSolaceDest(Destination destination)
        {
            // This is expensive and wasteful, but we profiled and tried an 
            // in-memory cache here and it didn't really speed things up.

            if (destination == null)
                return null;
            if (destination is Queue)
                return ContextFactory.Instance.CreateQueue(destination.Name);
            if (destination is Topic)
                return ContextFactory.Instance.CreateTopic(destination.Name);
            throw new ArgumentException("Expected topic or queue.");
        }

        private static Destination ConvertFromSolaceDest(IDestination solaceDest)
        {
            // This is expensive and wasteful, but we profiled and tried an 
            // in-memory cache here and it didn't really speed things up.

            if (solaceDest == null)
                return null;
            if (solaceDest is ITopic)
                return new Topic(solaceDest.Name);
            if (solaceDest is IQueue)
                return new Queue(solaceDest.Name);
            throw new ArgumentException("Expected topic or queue.");
        }

        private static void WriteCustomHeader(string key, object val, IMapContainer map)
        {
            if (val is string)
                map.AddString(key, val as string);
            else if (val is Int16 || val is short)
                map.AddInt16(key, (short)val);
            else if (val is Int32 || val is int)
                map.AddInt32(key, (int)val);
            else if (val is Int64 || val is long)
                map.AddInt64(key, (long)val);
            else if (val is bool)
                map.AddBool(key, (bool)val);
            else if (val is float)
                map.AddFloat(key, (float)val);
            else if (val is double)
                map.AddDouble(key, (double)val);
            else if (val is UInt16 || val is ushort)
                map.AddUInt16(key, (short)val);
            else if (val is UInt32 || val is uint)
                map.AddUInt32(key, (uint)val);
            else if (val is UInt64 || val is ulong)
                map.AddUInt64(key, (long)val);
            else if (val is byte)
                map.AddUInt8(key, (byte)val);
            else if (val is sbyte)
                map.AddInt8(key, (sbyte)val);
            else if (val is byte[])
                map.AddByteArray(key, (byte[])val);
            else if (val is char)
                map.AddChar(key, (char)val);
            else
                throw new MessagingException("Unsupported app header type" + val.GetType());
        }

        private static void ConvertAppHeaders(IDictionary<string, object> appHeaders, IMessage solaceMsg)
        {
            IMapContainer map = solaceMsg.CreateUserPropertyMap();
            foreach(string key in appHeaders.Keys)
            {
                object val = appHeaders[key];
                WriteCustomHeader(key, val, map);
            }
        }

        private static void BuildHeaderMap(IMessage msg, IDictionary<string, object> outHeaders)
        {
            IMapContainer headers = msg.UserPropertyMap;
            if (headers == null)
                return;

            while (headers.HasNext())
            {
                KeyValuePair<string, ISDTField> kv = headers.GetNext();
                string key = kv.Key;
                outHeaders[key] = kv.Value.Value;
            }
            headers.Close();
        }
    }
}
