using CDD.CommCentral.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CDD.CommCentral.Clients
{
    public enum Component { MessageHost, Publisher, Subscriber, Error };

    public partial class ClientManager
    {
        private static readonly int IDENTITY_TOKEN_LENGTH = 3;
        private static readonly int GUID_LENGTH = 12;

        private volatile object listMessageHostsLock = new object();
        private List<TcpClient> m_MessageHosts = new List<TcpClient>();

        private volatile object listRxBuffersLock = new object();
        private Dictionary<EndPoint, BuffMgr> m_RxBuffers = new Dictionary<EndPoint, BuffMgr>();

        private volatile object listPubServersLock = new object();
        private Dictionary<Guid, EndPoint> m_PubServers = new Dictionary<Guid, EndPoint>();

        private ConsoleLogger m_Logger;
        private ConsoleLogger m_ClientLogger;

        public ClientManager(ConsoleLogger logger, ConsoleLogger clientLogger)
        {
            m_Logger = logger;
            m_ClientLogger = clientLogger;
        }

        public void OnAccept(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;

            if (listener.Server != null && listener.Server.IsBound)
            {
                //Listener can BeginAccept the next connection immediately after this...
                TcpClient tcpc = listener.EndAcceptTcpClient(ar);
                //...trigger manual reset event
                Program.connectionAccepted.Set();                       

                //Log
                IPEndPoint remoteEP = (IPEndPoint)tcpc.Client.RemoteEndPoint;
                m_Logger.Record(String.Format("Connection accepted [{0}:{1}]; Awaiting authentication", remoteEP.Address, remoteEP.Port));

                /* Identify client
                 * 
                 * TODO: Eventually this will be an actual secure authentication process(TLS?).
                 *       For now, just get an identification token from the connecting client.
                 */
                Component componentType = Component.Error;
                try
                {
                    byte[] rawIdentityToken = new byte[IDENTITY_TOKEN_LENGTH];

                    //Expect a 3-byte identity token. In any other case, drop the connection.
                    tcpc.Client.ReceiveTimeout = 500;
                    if (tcpc.Client.Receive(rawIdentityToken, IDENTITY_TOKEN_LENGTH, SocketFlags.None) != IDENTITY_TOKEN_LENGTH)
                        throw new KeyNotFoundException();

                    componentType = new Dictionary<string, Component>() { {"HST", Component.MessageHost},
                                                                          {"PUB", Component.Publisher},
                                                                          {"SUB", Component.Subscriber}
                                                                        }[Encoding.UTF8.GetString(rawIdentityToken)];
                }
                catch (KeyNotFoundException)
                {
                    //Drop the connection
                    if (tcpc.Connected)
                        tcpc.Close();
                    m_Logger.Record(String.Format("Authentication FAIL [{0}:{1}]; Dropping connection", remoteEP.Address, remoteEP.Port));
                }
                finally
                {
                    tcpc.Client.ReceiveTimeout = 0;
                }

                //Client has been identified. Handle case for each component type.
                BuffMgr buffer = new BuffMgr();

                switch (componentType)
                {
                    case Component.MessageHost:
                        //Add new MessageHost client info to collection
                        lock (listRxBuffersLock)
                        {
                            m_RxBuffers.Add(remoteEP, buffer);
                        }
                        lock (listMessageHostsLock)
                        {
                            m_MessageHosts.Add(tcpc);
                            UpdateConsole_ClientLog();
                        }
                        //Set to receive from MessageHost
                        tcpc.Client.BeginReceive(buffer.rawBytes, 0, buffer.rawBytes.Length, SocketFlags.None, new AsyncCallback(OnMessageHostReceive), tcpc.Client);
                        m_Logger.Record(String.Format("Authentication SUCCESS; Ready to receive from MessageHost[{0}:{1}]", remoteEP.Address, remoteEP.Port));
                        break;

                    case Component.Publisher:
                        //Expect to receive the Publisher's GUID
                        m_Logger.Record(String.Format("Authentication SUCCESS; Ready to receive GUID from Publisher[{0}:{1}]", remoteEP.Address, remoteEP.Port));
                        byte[] rawPubGuid = new byte[GUID_LENGTH];
                        if (tcpc.Client.Receive(rawPubGuid, GUID_LENGTH, SocketFlags.None) != GUID_LENGTH)
                        {
                            //Error occurred, drop the connection
                            if (tcpc.Connected)
                                tcpc.Close();
                            m_Logger.Record(String.Format("Invalid GUID received from Publisher[{0}:{1}]; Dropping connection", remoteEP.Address, remoteEP.Port));
                        }
                        else
                        {
                            //Validate GUID
                            Guid pubGuid;
                            try { pubGuid = new Guid(rawPubGuid); }
                            catch {
                                //Invalid GUID, drop the connection
                                if (tcpc.Connected)
                                    tcpc.Close();
                                m_Logger.Record(String.Format("Invalid GUID received from Publisher[{0}:{1}]; Dropping connection", remoteEP.Address, remoteEP.Port));
                                break;
                            }

                            //Determine which MessageHost is least busy, exchange session info between the two endpoints
                            IPEndPoint hostEP = null;
                            lock (listMessageHostsLock)
                            {
                                //TODO...
                                //if (m_MessageHosts.Count > 0)
                                //    hostEP = (IPEndPoint)m_MessageHosts.SortBy(mh => mh.NumberServed?)[0].Socket.RemoteEndPoint;
                            }

                            //TODO...
                            //if (TransferConnection(hostEP, remoteEP))
                            //{
                                //Record the publisher's GUID, and MessageHost endpoint
                                lock (listPubServersLock)
                                {
                                    m_PubServers.Add(pubGuid, hostEP);
                                }
                            //}
                            m_Logger.Record(String.Format("MessageHost[{0}:{1}] will serve Publisher[{2}:{3}]; Transferring connection", hostEP.Address, hostEP.Port, remoteEP.Address, remoteEP.Port));
                        }
                        break;

                    case Component.Subscriber:
                        //Expect to receive target Publisher's GUID from Subscriber
                        m_Logger.Record(String.Format("Authentication SUCCESS; Ready to receive target GUID from Subscriber[{0}:{1}]", remoteEP.Address, remoteEP.Port));
                        byte[] rawTargetGuid = new byte[GUID_LENGTH];
                        if (tcpc.Client.Receive(rawTargetGuid, GUID_LENGTH, SocketFlags.None) != GUID_LENGTH)
                        {
                            //Error occurred, drop the connection
                            if (tcpc.Connected)
                                tcpc.Close();
                            m_Logger.Record(String.Format("Invalid GUID received from Publisher[{0}:{1}]; Dropping connection", remoteEP.Address, remoteEP.Port));
                            break;
                        }
                        else
                        {
                            //Validate GUID
                            Guid targetGuid;
                            try { targetGuid = new Guid(rawTargetGuid); }
                            catch
                            {
                                //Invalid GUID, drop the connection
                                if (tcpc.Connected)
                                    tcpc.Close();
                                m_Logger.Record(String.Format("Invalid GUID received from Subscriber[{0}:{1}]; Dropping connection", remoteEP.Address, remoteEP.Port));
                                break;
                            }

                            //Determine which MessageHost is serving the target publisher, exchange session info
                            IPEndPoint hostEP = null;
                            lock (listPubServersLock)
                            {
                                hostEP = (IPEndPoint)m_PubServers[targetGuid];
                            }

                            //TODO...
                            //if (TransferConnection(hostEP, remoteEP))
                            //{
                            //    m_Logger.Record(String.Format("MessageHost[{0}:{1}] will serve Publisher[{2}:{3}]; Transferring connection", hostEP.Address, hostEP.Port, remoteEP.Address, remoteEP.Port));
                            //else
                            //    m_Logger.Record(String.Format("MessageHost[{0}:{1}] will serve Publisher[{2}:{3}]; Transferring connection", hostEP.Address, hostEP.Port, remoteEP.Address, remoteEP.Port));
                            //}
                        }
                        break;

                    default: break;
                }
            }
        }

        public void RemoveClient(EndPoint remoteEP)
        {
            lock (listMessageHostsLock)
            {
                TcpClient t = m_MessageHosts.FirstOrDefault(c => c.Client.RemoteEndPoint.Equals(remoteEP));
                if (t != null)
                {
                    t.Close();
                    m_MessageHosts.Remove(t);
                    UpdateConsole_ClientLog();
                }
            }
            lock (listRxBuffersLock)
            {
                m_RxBuffers.Remove(remoteEP);
            }
        }

        private void UpdateConsole_ClientLog()
        {
            StringBuilder sbClients = new StringBuilder();
            foreach (TcpClient client in m_MessageHosts)
            {
                IPEndPoint localEP = (IPEndPoint)client.Client.LocalEndPoint;
                IPEndPoint remoteEP = (IPEndPoint)client.Client.RemoteEndPoint;
                sbClients.AppendLine(String.Format("Port {0} <---> QueueHolder({1}:{2}) serving ?? Publishers",
                                                    localEP.Port, remoteEP.Address, remoteEP.Port));
            }
            m_ClientLogger.Replace(sbClients.ToString());
        }


        /// <summary>
        /// 
        /// </summary>
        public void GracefulShutdown()
        {
            //TODO: Close all MessageHost TCP connections
        }
    }
}