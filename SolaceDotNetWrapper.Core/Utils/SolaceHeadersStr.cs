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

namespace SolaceDotNetWrapper.Core.Utils
{
    /// <summary>
    ///     To avoid repeated calls to ToString() we cache string representations to use as dictionary keys.
    /// </summary>
    public class SolaceHeadersStr
    {
        /// <summary>
        ///     Publisher can set this property on a Guaranteed message to request the Solace the appliance to acknowledge the receipt of the message immediately, as opposed to waiting for the message to be spooled on the appliance and then acknowledge it.
        /// </summary>
        public static readonly string AckImmediately = SolaceHeaders.SOL_AckImmediately.ToString();

        /// <summary>
        ///     Applies to received messages only with DeliveryMode MessageDeliveryMode.PERSISTENT or MessageDeliveryMode.NON_PERSISTENT. Represents the message Id of a Guaranteed message and can be used when acknowledging such a message. If not available 0 is returned.
        /// </summary>
        public static readonly string AdMessageId = SolaceHeaders.SOL_AdMessageId.ToString();

        /// <summary>
        ///     Gets/sets the application messageId. Returns null if not present.
        /// </summary>
        public static readonly string ApplicationMessageId = SolaceHeaders.SOL_ApplicationMessageId.ToString();

        /// <summary>
        ///     Gets/sets the application message type. This value is used by applications only and is passed through the API untouched. If not present, null is returned.
        /// </summary>
        public static readonly string ApplicationMessageType = SolaceHeaders.SOL_ApplicationMessageType.ToString();

        /// <summary>
        ///     Returns the request ID set in the cache request. Always null if CacheStatus is MessageCacheStatus.Live If not present -1 is returned
        /// </summary>
        public static readonly string CacheRequestId = SolaceHeaders.SOL_CacheRequestId.ToString();

        /// <summary>
        ///     The correlation id is used for correlating a request to a reply. If not present, null is returned.
        /// </summary>
        public static readonly string CorrelationId = SolaceHeaders.SOL_CorrelationId.ToString();

        /// <summary>
        ///     A setter/getter for the Deliver-To-One (DTO) property on a message with a Delivery mode of Direct. When a message has the DTO property set, it can to be delivered only to one client. This property is only supported by routers running SolOS-TR.
        /// </summary>
        public static readonly string DeliverToOne = SolaceHeaders.SOL_DeliverToOne.ToString();

        /// <summary>
        ///     Returns true if one or more messages have been discarded prior to the current message, otherwise it returns false. Indicates congestion discards only and is not affected by message eliding.
        /// </summary>
        public static readonly string DiscardIndication = SolaceHeaders.SOL_DiscardIndication.ToString();

        /// <summary>
        ///     Expired messages with this property set to true are moved to a Dead Message Queue when one is provisioned on the router. Default: false
        /// </summary>
        public static readonly string DmqEligible = SolaceHeaders.SOL_DmqEligible.ToString();

        /// <summary>
        ///     Setting this property to true indicates that this message should be eligible for eliding.
        /// </summary>
        public static readonly string ElidingEligible = SolaceHeaders.SOL_ElidingEligible.ToString();

        /// <summary>
        ///     Returns true if it is a reply message (reply to a request).
        /// </summary>
        public static readonly string IsReplyMessage = SolaceHeaders.SOL_IsReplyMessage.ToString();

        /// <summary>
        ///     Gets the receive timestamp (in milliseconds, from midnight, January 1, 1970 UTC). Returns -1 if not present.
        /// </summary>
        public static readonly string ReceiverTimestamp = SolaceHeaders.SOL_ReceiverTimestamp.ToString();

        /// <summary>
        ///     Applies only when DeliveryMode is MessageDeliveryMode.PERSISTENT or MessageDeliveryMode.NON_PERSISTENT. Indicates if the message has been delivered by the router to the API before.
        /// </summary>
        public static readonly string Redelivered = SolaceHeaders.SOL_Redelivered.ToString();

        /// <summary>
        ///     Represents the sender id. Returns null if not present.
        /// </summary>
        public static readonly string SenderId = SolaceHeaders.SOL_SenderId.ToString();

        /// <summary>
        ///     Gets the sender timestamp (in milliseconds, from midnight, January 1, 1970 UTC). Returns -1 if not present or set.
        /// </summary>
        public static readonly string SenderTimestamp = SolaceHeaders.SOL_SenderTimestamp.ToString();

        /// <summary>
        ///     Represents the message sequence number. The set operation overrides the sequence number automatically generated by the session (if set). Returns null if not present.
        /// </summary>
        public static readonly string SequenceNumber = SolaceHeaders.SOL_SequenceNumber.ToString();

        /// <summary>
        ///     The number of milliseconds before the message is discarded or moved to a Dead Message Queue. A value of 0 means the message never expires. The default value is zero. Note that this property is only valid for Guaranteed Delivery messages (Persistent and Non-Persistent). It has no effect when used in conjunction with other message types unless the message is promoted by the router to a Guaranteed Delivery message.
        /// </summary>
        public static readonly string TimeToLive = SolaceHeaders.SOL_TimeToLive.ToString();

        /// <summary>
        ///     The user data part of the message (maximum: 36 bytes). Returns null if not present.
        /// </summary>
        public static readonly string UserData = SolaceHeaders.SOL_UserData.ToString();
    }

    internal class SolTypeAdapters
    {
        /// <summary>
        ///     Publisher can set this property on a Guaranteed message to request the Solace the appliance to acknowledge the receipt of the message immediately, as opposed to waiting for the message to be spooled on the appliance and then acknowledge it.
        /// </summary>
        public static readonly SolTypeAdapter<Boolean> AckImmediately =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_AckImmediately);

        public static readonly SolTypeAdapter<Int64> AdMessageId =
            new SolTypeAdapter<Int64>(SolaceHeaders.SOL_AdMessageId);

        public static readonly SolTypeAdapter<string> ApplicationMessageId =
            new SolTypeAdapter<string>(SolaceHeaders.SOL_ApplicationMessageId);

        public static readonly SolTypeAdapter<string> ApplicationMessageType =
            new SolTypeAdapter<string>(SolaceHeaders.SOL_ApplicationMessageType);

        public static readonly SolTypeAdapter<Int64> CacheRequestId =
            new SolTypeAdapter<Int64>(SolaceHeaders.SOL_CacheRequestId);

        //public static readonly SolTypeAdapter<string> CacheStatus = new SolTypeAdapter<string>(SolaceHeaders.CacheStatus);
        public static readonly SolTypeAdapter<string> CorrelationId =
            new SolTypeAdapter<string>(SolaceHeaders.SOL_CorrelationId);

        //public static readonly SolTypeAdapter<Boolean> CorrelationKey = new SolTypeAdapter<Boolean>(SolaceHeaders.CorrelationKey);
        public static readonly SolTypeAdapter<Boolean> DeliverToOne =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_DeliverToOne);

        public static readonly SolTypeAdapter<Boolean> DiscardIndication =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_DiscardIndication);

        public static readonly SolTypeAdapter<Boolean> DmqEligible =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_DmqEligible);

        public static readonly SolTypeAdapter<Boolean> ElidingEligible =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_ElidingEligible);

        public static readonly SolTypeAdapter<Boolean> IsReplyMessage =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_IsReplyMessage);

        public static readonly SolTypeAdapter<Int64> ReceiverTimestamp =
            new SolTypeAdapter<Int64>(SolaceHeaders.SOL_ReceiverTimestamp);

        public static readonly SolTypeAdapter<Boolean> Redelivered =
            new SolTypeAdapter<Boolean>(SolaceHeaders.SOL_Redelivered);

        public static readonly SolTypeAdapter<string> SenderId = new SolTypeAdapter<string>(SolaceHeaders.SOL_SenderId);

        public static readonly SolTypeAdapter<Int64> SenderTimestamp =
            new SolTypeAdapter<Int64>(SolaceHeaders.SOL_SenderTimestamp);

        public static readonly SolTypeAdapter<Int64> SequenceNumber =
            new SolTypeAdapter<Int64>(SolaceHeaders.SOL_SequenceNumber);

        public static readonly SolTypeAdapter<Int64> TimeToLive = new SolTypeAdapter<Int64>(SolaceHeaders.SOL_TimeToLive);
        public static readonly SolTypeAdapter<byte[]> UserData = new SolTypeAdapter<byte[]>(SolaceHeaders.SOL_UserData);
    }

    internal class SolTypeAdapter<T>
    {
        private readonly string keyName;
        private SolaceHeaders hdr;

        public SolTypeAdapter(SolaceHeaders h)
        {
            hdr = h;
            keyName = h.ToString();
        }

        public T CheckAdaptValue(object value)
        {
            if (value == null)
                return default(T);
            if (typeof(T).IsAssignableFrom(value.GetType()))
                return (T)value;
            else
                throw new ArgumentException("Invalid cast: " + keyName);
        }
    }
}
