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
using System.Threading;
using SolaceDotNetWrapper.Core.Utils;

namespace SolaceDotNetWrapper.Core
{
    public class PersistentMessage : Message
    {
        public delegate void AckHandler(long id);

        private int _expectedAcks;

        public PersistentMessage() :
            base(true, false, true)
        {
        }

        public AckHandler AckHandlerFunction { get; set; }

        public override void Acknowledge()
        {
            int decrValue = Interlocked.Decrement(ref _expectedAcks);
            if (decrValue == 0 && AckHandlerFunction != null)
            {
                long adMessageId = (Headers.ContainsKey(SolaceHeadersStr.AdMessageId))
                                       ? (long)Headers[SolaceHeadersStr.AdMessageId]
                                       : 0;
                AckHandlerFunction(adMessageId);
            }
        }

        public void IncrementExpectedAcks()
        {
            int incrValue = Interlocked.Increment(ref _expectedAcks);
        }
    }
}
