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

using NUnit.Framework;
using SolaceDotNetWrapper.Core;

namespace SolaceDotNetWrapper.Tests
{
    public class SolaceOptionsTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ToSessionPropertiesDefaultTest()
        {
            var solaceOptions = new SolaceOptions();
            var sessionProps = solaceOptions.ToSessionProperties();

            Assert.AreEqual(solaceOptions.Host, sessionProps.Host);
            Assert.AreEqual(solaceOptions.MsgVpnName, sessionProps.VPNName);
            Assert.AreEqual(solaceOptions.Username, sessionProps.UserName);
            Assert.AreEqual(solaceOptions.Password, sessionProps.Password);
            Assert.AreEqual(solaceOptions.AutoReconnectRetries, sessionProps.ReconnectRetries);
            Assert.AreEqual(solaceOptions.AutoReconnectWaitinMs, sessionProps.ReconnectRetriesWaitInMsecs);
        }

        [Test]
        public void ToSessionPropertiesTest()
        {
            var solaceOptions = CreateSolaceOptions(true);
            var sessionProps = solaceOptions.ToSessionProperties();

            Assert.AreEqual(solaceOptions.Host, sessionProps.Host);
            Assert.AreEqual(solaceOptions.MsgVpnName, sessionProps.VPNName);
            Assert.AreEqual(solaceOptions.Username, sessionProps.UserName);
            Assert.AreEqual(solaceOptions.Password, sessionProps.Password);
            Assert.AreEqual(solaceOptions.AutoReconnectRetries, sessionProps.ReconnectRetries);
            Assert.AreEqual(solaceOptions.AutoReconnectWaitinMs, sessionProps.ReconnectRetriesWaitInMsecs);
        }

        [Test]
        public void ToSessionProperties_AutoReconnectDisabledTest()
        {
            var solaceOptions = CreateSolaceOptions(false);
            var sessionProps = solaceOptions.ToSessionProperties();

            Assert.AreEqual(solaceOptions.Host, sessionProps.Host);
            Assert.AreEqual(solaceOptions.MsgVpnName, sessionProps.VPNName);
            Assert.AreEqual(solaceOptions.Username, sessionProps.UserName);
            Assert.AreEqual(solaceOptions.Password, sessionProps.Password);
            Assert.AreEqual(0, sessionProps.ConnectRetriesPerHost);
            Assert.AreEqual(0, sessionProps.ConnectRetries);
            Assert.AreEqual(0, sessionProps.ReconnectRetries);
            Assert.AreEqual(3000, sessionProps.ReconnectRetriesWaitInMsecs);
        }

        private SolaceOptions CreateSolaceOptions(bool autoReconnectOpt)
        {
            var solaceOptions = new SolaceOptions()
            {
                Host = "tcp://localhost:55555",
                MsgVpnName = "test",
                Username = "testUser",
                Password = "tesPassword",
                AutoReconnect = autoReconnectOpt
            };

            return solaceOptions;
        }
    }
}