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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using SolaceDotNetWrapper.Core.Utils;
using SolaceSystems.Solclient.Messaging;

namespace SolaceDotNetWrapper.Core
{
    public class SolaceConnection : IConnection
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly SolaceOptions solaceOptions;
        private readonly FlowBindings flowBindings;
        private readonly RequestReplyStateManager requestMgr;

        /// <summary>
        ///     Object used for synchronizations.
        /// </summary>
        private static readonly object _syncLock = new Object();

        private bool isInitialized = false;
        private bool alreadyAddedLocalTopic = false;
        private Destination p2pInboxInUse = null;

        private volatile ConnectionState connectionState = ConnectionState.Created;

        private IContext context = null;
        private ISession session = null;

        // We use the TPL DataFlow Library's BufferBlocks to pass messages & events
        // from this wrapper api (producer) to the calling app (consumer)
        BufferBlock<Message> messageQueue = null;
        List<BufferBlock<ConnectionEvent>> connectionEvtObservers = new List<BufferBlock<ConnectionEvent>>();

        // Task completion source for asynchronously waiting for the connection UP event
        TaskCompletionSource<ConnectionEvent> tcsConnection = null;

        // Task completion source for topic subscriptions
        TaskCompletionSource<bool> tcsTopicSub = null;

        public Destination ClientP2PInbox
        {
            get { return p2pInboxInUse; }
        }

        public SolaceConnection(SolaceOptions solaceOptions, BufferBlock<Message> messageQueue, ILoggerFactory loggerFactory)
        {
            this.solaceOptions = solaceOptions;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<SolaceConnection>();
            this.messageQueue = messageQueue;
            this.flowBindings = new FlowBindings(loggerFactory.CreateLogger<FlowBindings>());
            this.requestMgr = new RequestReplyStateManager(loggerFactory.CreateLogger<RequestReplyStateManager>());

            // Initialize the API if not already done so
            lock (_syncLock)
            {
                if (!isInitialized)
                {
                    InitializeSolaceAPI(solaceOptions.SolaceApiLogLevel);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionEvtQueue"></param>
        public void RegisterConnectionEvents(BufferBlock<ConnectionEvent> connectionEvtQueue)
        {
            connectionEvtObservers.Add(connectionEvtQueue);
        }

        /// <summary>
        /// Asynchronously stablishes a connection to the Solace broker.
        /// Connection UP and Fail events are sent asynchronously via the BufferBlock
        /// configured for the connection.
        /// </summary>
        public Task<ConnectionEvent> ConnectAsync()
        {
            // Ignore & return if already connected
            if (connectionState == ConnectionState.Opened || connectionState == ConnectionState.Opening)
            {
                throw new MessagingException("Connection already opened or opening.");
            }

            return ConnectAsyncInternal();
        }

        private async Task<ConnectionEvent> ConnectAsyncInternal()
        {
            connectionState = ConnectionState.Opening;

            ConnectionEvent connectionEvent;
            tcsConnection = new TaskCompletionSource<ConnectionEvent>();

            // Create a new context
            var cp = new ContextProperties();
            context = ContextFactory.Instance.CreateContext(cp, null);

            // Ensure the connection & publish is done in a non-blocking fashion
            var sessionProps = solaceOptions.ToSessionProperties();
            sessionProps.ConnectBlocking = false;
            sessionProps.SendBlocking = false;
            // Required for internal topic dispatching
            sessionProps.TopicDispatch = true;

            // Create the session with the event handlers
            session = context.CreateSession(solaceOptions.ToSessionProperties(), MessageEventHandler, SessionEventHandler);

            // Connect the session - non-blocking
            var returnCode = session.Connect();
            if (returnCode == ReturnCode.SOLCLIENT_FAIL)
            {
                // Something bad happened before the connection attempt to Solace
                // broker
                var errorInfo = ContextFactory.Instance.GetLastSDKErrorInfo();
                throw new MessagingException(errorInfo.ErrorStr);
            }
            else
            {
                connectionEvent = await tcsConnection.Task.ConfigureAwait(false);
                if (connectionEvent.State == ConnectionState.Opened)
                {
                    var p2pinbox = session.GetProperty(SessionProperties.PROPERTY.P2PInboxInUse) as string;
                    p2pInboxInUse = new Topic(p2pinbox);

                    // Create a local topic subscription. This is required when Topic Dispatch is enabled
                    // in the Solace API and client adds subscriptions using a subscription manager
                    // (i.e. on-behalf of subscriptions). This is a local subscription only and request
                    // is not sent to the appliance.
                    // Also don't readd the local topic when reconnecting.
                    if (!alreadyAddedLocalTopic)
                    {
                        ITopic solTopic = ContextFactory.Instance.CreateTopic(">");
                        IDispatchTarget target = session.CreateDispatchTarget(solTopic,
                            async (sender, msgEv) => await AcceptMessageEventAsync(msgEv, null).ConfigureAwait(false));

                        session.Subscribe(target, SubscribeFlag.LocalDispatchOnly, null);
                        alreadyAddedLocalTopic = true;
                    }
                }
            }

            return connectionEvent;
        }

        /// <summary>
        /// Asynchronously disconnects from the Solace broker. 
        /// </summary>
        public Task DisconnectAsync()
        {
            if (connectionState == ConnectionState.Closed)
                return Task.FromResult(0);

            return DisconnectAsyncInternal();
        }

        private async Task DisconnectAsyncInternal()
        {
            try
            {
                await OnStateChangedAsync(ConnectionState.Closing, null).ConfigureAwait(false);
                session.Disconnect();
                context.Dispose();
                await OnStateChangedAsync(ConnectionState.Closed, null).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                throw new MessagingException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:SolaceDotNetWrapper.Core.SolaceConnection"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:SolaceDotNetWrapper.Core.SolaceConnection"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:SolaceDotNetWrapper.Core.SolaceConnection"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:SolaceDotNetWrapper.Core.SolaceConnection"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:SolaceDotNetWrapper.Core.SolaceConnection"/> was occupying.</remarks>
        public void Dispose()
        {
            DisconnectAsync().Wait();
            try
            {
                session.Dispose();
            }
            catch (Exception ex)
            {
                throw new MessagingException(ex.Message, ex);
            }
        }

        public Task<MessageCorrelationContext> SendAsync(Message message)
        {
            using (var solaceMsg = SolaceNativeMsgAdapter.ConvertToNativeMsg(message))
            {
                var msgCtx = new MessageCorrelationContext(message, message.CorrelationId);
                var tcs = new TaskCompletionSource<MessageCorrelationContext>();

                solaceMsg.CorrelationKey = new MessageTaskPair(msgCtx, tcs);

                ReturnCode rc;
                try
                {
                    rc = session.Send(solaceMsg);
                }
                catch (Exception e)
                {
                    solaceMsg.Dispose();
                    throw new MessagingException(e.Message, e);
                }
                switch (rc)
                {
                    case ReturnCode.SOLCLIENT_OK:
                    case ReturnCode.SOLCLIENT_IN_PROGRESS:
                        // OK
                        break;
                    default:
                        throw new MessagingException("Send failure: " + rc.ToString());
                }

                if (!message.IsPersistent)
                {
                    // There will be no ack, immediately consider it acknowledged (complete synchronously)
                    tcs.SetResult(msgCtx);
                }

                return tcs.Task;
            }
        }

        public Task<Message> SendRequestAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SubscribeAsync(Destination destination, bool flowStartState = false)
        {
            if(!(destination is Topic || destination is Queue))
                throw new ArgumentException("Destination must be a queue or topic.");
            if (destination != null)
                throw new ArgumentNullException("Destination cannot be null");

            if (destination is Topic)
                return SubscribeTopicAsyncInternal((Topic)destination);
            else if (destination is Queue)
                return SubscribeQueueAsyncInternal((Queue)destination, flowStartState);
        }

        public async Task<bool> SubscribeTopicAsyncInternal(Topic topic)
        {
            ITopic solTopic = ContextFactory.Instance.CreateTopic(topic.Name);
            FlowBindings.FlowBindingElement binding = null;
            bool newSubscription = _clientBindings.AddBinding(t, client, out binding);
            if (newSubscription && binding != null)
            {
                try
                {
                    // add to solace message bus
                    IDispatchTarget dTarget = _solSession.CreateDispatchTarget(solTopic,
                                                                               (sender, msgEv) =>
                                                                               {
                                                                                   AcceptMessageEvent(msgEv, binding);
                                                                               });
                    binding.TopicDispatchTarget = dTarget;
                    _solSession.Subscribe(dTarget, SubscribeFlag.RequestConfirm | SubscribeFlag.WaitForConfirm, null);
                    Debug.Assert((binding.Flow != null) ^ (binding.TopicDispatchTarget != null),
                                 "Binding should have one of Flow/Topic");
                }
                catch (Exception e)
                {
                    binding.TopicDispatchTarget = null;
                    _clientBindings.RemoveBinding(t, client, out binding);
                    throw WrapException(e);
                }
            }
        }

        public Task<bool> UnsubscribeAsync(Destination destination)
        {
            throw new NotImplementedException();
        }

        #region Helper Methods

        private void InitializeSolaceAPI(string logLevel)
        {
            // Initialize the API & set API logging
            var cfp = new ContextFactoryProperties();
            // TODO: Set log level
            cfp.SolClientLogLevel = GetSolaceLogLevel(logLevel);
            // TODO: Delegate logs to the wrapper's logging factory
            cfp.LogDelegate = OnSolaceApiLog;
            // Must init the API before using any of its artifacts.
            ContextFactory.Instance.Init(cfp);
        }

        private SolLogLevel GetSolaceLogLevel(string logLevel)
        {
            try
            {
                return (SolLogLevel) Enum.Parse(typeof(SolLogLevel), logLevel);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Invalid Solace API log level specified - Defaulting level to NOTICE");
                return SolLogLevel.Notice;
            }
        }

        private LogLevel GetLogLevel(SolLogLevel solLogLevel)
        {
            switch (solLogLevel)
            {
                case SolLogLevel.Critical:
                    return LogLevel.Critical;
                case SolLogLevel.Error:
                    return LogLevel.Error;
                case SolLogLevel.Warning:
                    return LogLevel.Warning;
                case SolLogLevel.Notice:
                    return LogLevel.Information;
                case SolLogLevel.Info:
                    return LogLevel.Trace;
                case SolLogLevel.Debug:
                    return LogLevel.Debug;
                case SolLogLevel.Emergency:
                case SolLogLevel.Alert:
                default:
                    return LogLevel.None;
            }
        }

        

        /// <summary>
        /// Log delegate for redirecting Solace .NET API logs to the wrapper's
        /// logging abstraction.
        /// </summary>
        /// <param name="solLogInfo">The Solace API log info containing the level, 
        /// exception, and message.</param>
        private void OnSolaceApiLog(SolLogInfo solLogInfo)
        {
            var logLevel = GetLogLevel(solLogInfo.LogLevel);
            if (logger.IsEnabled(logLevel))
            {
                logger.Log(logLevel, solLogInfo.LogException, solLogInfo.LogMessage);
            }
        }

        private async Task AcceptMessageEventAsync(MessageEventArgs msgEv, FlowBindings.FlowBindingElement binding)
        {
            try
            {
                Message rxMessage = SolaceNativeMsgAdapter.ConvertFromNativeMsg(msgEv.Message);

                bool continueDelivery = await requestMgr.HandleIncomingAsync(rxMessage).ConfigureAwait(false);

                // If continueDelivery is false, the message was handled by the requestMgr so we drop it.
                if (continueDelivery && binding != null)
                {
                    await binding.DispatchMessageAsync(rxMessage).ConfigureAwait(false);
                }
                else if (continueDelivery && binding == null)
                {
                    // There is no callback on which to forward the message to, so dispatch
                    // the message to the default session callback.
                    await messageQueue.SendAsync(rxMessage).ConfigureAwait(false);
                }
            }
            finally
            {
                msgEv.Message.Dispose();
            }
        }

        #endregion

        #region Event Handlers
        private async void MessageEventHandler(object sender, MessageEventArgs msgEv)
        {
            await AcceptMessageEventAsync(msgEv, null).ConfigureAwait(false);
        }

        private async void SessionEventHandler(object sender, SessionEventArgs e)
        {
            switch (e.Event)
            {
                case SessionEvent.Acknowledgement:
                case SessionEvent.RejectedMessageError:
                    var msgTaskPair = (e.CorrelationKey as MessageTaskPair);
                    if (msgTaskPair != null)
                    {
                        Exception pub_ex = null;
                        if (e.Event == SessionEvent.RejectedMessageError)
                            pub_ex = new MessagingException(string.Format(
                                "Message was rejected by the broker: Info({0}) ResponseCode({1})",
                                e.Info,
                                e.ResponseCode));
                        msgTaskPair.Complete(pub_ex);
                    }
                    break;
                case SessionEvent.ConnectFailedError:
                case SessionEvent.DownError:
                    // Change state
                    await OnStateChangedAsync(ConnectionState.Closed, e).ConfigureAwait(false);
                    break;
                case SessionEvent.Reconnecting:
                    await OnStateChangedAsync(ConnectionState.Reconnecting, e).ConfigureAwait(false);
                    break;
                case SessionEvent.Reconnected:
                    await OnStateChangedAsync(ConnectionState.Reconnected, e).ConfigureAwait(false);
                    break;
                case SessionEvent.UpNotice:
                    await OnStateChangedAsync(ConnectionState.Opened, e).ConfigureAwait(false);
                    break;
            }
        }

        private async Task OnStateChangedAsync(ConnectionState state, SessionEventArgs sessionEvtArgs)
        {
            connectionState = state;
            var connectionEvent = new ConnectionEvent()
            {
                State = connectionState,
                Info = sessionEvtArgs?.Info,
                ResponseCode = sessionEvtArgs?.ResponseCode ?? 0
            };

            if (tcsConnection != null)
                tcsConnection.TrySetResult(connectionEvent);

            foreach (var observer in connectionEvtObservers)
                await observer.SendAsync(connectionEvent).ConfigureAwait(false);
        }
        #endregion
    }
}
