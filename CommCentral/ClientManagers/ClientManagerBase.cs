using CDD.CommCentral.Clients;
using CDD.CommCentral.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CDD.CommCentral.Connection
{
    public enum Component { QueueHolder, Publisher, Subscriber };

    public abstract class ClientManagerBase : IClientManager
    {
        protected EventLogger m_Logger;
        protected EventLogger m_ClientLog;
        protected List<ClientBase> m_Clients = new List<ClientBase>();
        protected volatile object clientListModificationLock = new object();

        public static ClientManagerBase GetInstance(Component componentType, EventLogger logger, EventLogger clientLog)
        {
            switch (componentType)
            {
                case Component.QueueHolder:
                    return new ClientManager_QueueHolders(logger, clientLog);

                case Component.Publisher:
                    return new ClientManager_Publishers(logger, clientLog);

                case Component.Subscriber:
                    return new ClientManager_Subscribers(logger, clientLog);

                default:
                    throw new TypeAccessException("ClientManager.GetInstance(): Component type has not been implemented");
            }
        }

        protected void AddClient(ClientBase client)
        {
            lock(clientListModificationLock)
            {
                m_Clients.Add(client);
                UpdateClientLog();
            }
        }

        public void RemoveClient(IPEndPoint remoteEP)
        {
            lock(clientListModificationLock)
            {
                ClientBase client = m_Clients.FirstOrDefault(c => c.HostEP.Equals(remoteEP));
                if (client != null)
                    client.Socket.BeginDisconnect(true, new AsyncCallback(CompleteDisconnect), client.Socket);
            }
        }

        private void CompleteDisconnect(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;

            lock (clientListModificationLock)
            {
                ClientBase client = m_Clients.FirstOrDefault(c => c.HostEP.Equals(s.RemoteEndPoint));
                if (client != null)
                {
                    s.EndDisconnect(ar);
                    client.TcpClient.Close();
                    m_Clients.Remove(client);
                    UpdateClientLog();
                }
            }
        }

        public void Log(string eventStr)
        {
            m_Logger.Record(eventStr);
        }

        protected void UpdateClientLog()
        {
            StringBuilder sbClients = new StringBuilder();
            foreach (ClientBase client in m_Clients)
            {
                IPEndPoint localEP = (IPEndPoint)client.Socket.LocalEndPoint;
                IPEndPoint remoteEP = (IPEndPoint)client.Socket.RemoteEndPoint;
                sbClients.AppendLine(String.Format("Port {0} <---> QueueHolder({1}:{2}) serving {3} Publishers",
                                                   localEP.Port, remoteEP.Address, remoteEP.Port, client.PublishersServed));
            }
            m_ClientLog.Replace(sbClients.ToString());
        }

        public virtual void OnAccept(IAsyncResult ar)
        { throw new NotImplementedException("Interface ClientManager.OnAccept(IAsyncResult) is missing."); }

        public virtual void Shutdown()
        { throw new NotImplementedException("Interface ClientManager.Shutdown() is missing."); }
    }
}
