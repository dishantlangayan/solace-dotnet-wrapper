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

namespace SolaceDotNetWrapper.Core
{
    /// <summary>
    /// Abstract class for a Solace Topic or Queue destination.
    /// </summary>
    public abstract class Destination
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SolaceDotNetWrapper.Core.Destination"/> class.
        /// </summary>
        /// <param name="name">Name of the topic or queue destintation.</param>
        /// <param name="isTemporary">Define if the destination is temporary or not.</param>
        public Destination(string name, bool isTemporary = false)
        {
            this.Name = name;
            this.IsTemporary = false;
        }

        /// <summary>
        /// Name of the Solace destination.
        /// </summary>
        /// <value>The Solace destination name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:SolaceDotNetWrapper.Core.Destination"/> 
        /// is a temporary destination.
        /// </summary>
        /// <value><c>true</c> if is temporary; otherwise, <c>false</c>.</value>
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the name of 
        /// the destination.
        /// </summary>
        /// <returns>The name of the Solace Destination.</returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
