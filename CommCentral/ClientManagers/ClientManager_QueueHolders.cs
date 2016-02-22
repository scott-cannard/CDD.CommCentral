using CDD.CommCentral.Clients;
using CDD.CommCentral.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace CDD.CommCentral.Connection
{
    public class ClientManager_QueueHolders : ClientManagerBase
    {
        public ClientManager_QueueHolders(EventLogger logger, EventLogger clientLog)
        {
            m_Logger = logger;
            m_ClientLog = clientLog;
        }

        public override void OnAccept(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            QueueHolderClient newQueueHolder;

            if (listener.Server != null && listener.Server.IsBound)
            {
                try //Accept and get new TcpClient
                {
                    newQueueHolder = new QueueHolderClient(this, listener.EndAcceptTcpClient(ar));
                    m_Logger.Record(String.Format("Received connection request, Socket.ACCEPT <-- QueueHolder({0}:{1})", ((IPEndPoint)newQueueHolder.Socket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)newQueueHolder.Socket.RemoteEndPoint).Port));
                    //Reset listener to receive the next connection
                    listener.BeginAcceptTcpClient(OnAccept, listener);
                }
                catch (Exception ex)
                {
                    m_Logger.Record("ClientManager_QueueHolders.OnAccept(): " + ex.Message);
                    return;
                }

                //Store new client in the collection
                base.AddClient(newQueueHolder);
                
                //Set to receive
                newQueueHolder.Socket.BeginReceive(newQueueHolder.RxBuffer, 0, newQueueHolder.RxBuffer.Length, SocketFlags.None, new AsyncCallback(newQueueHolder.OnReceive), newQueueHolder.Socket);
            }
        }

        public override void Shutdown()
        {
            m_Logger.Record("Closing QueueHolder connections...");
            lock (clientListModificationLock)
            {
                foreach (QueueHolderClient qHolder in m_Clients)
                {
                    m_Logger.Record(String.Format("Shutting down {0}", qHolder.HostEP));
                    base.RemoveClient(qHolder.HostEP);
                }
            }
            m_Logger.Record("...all QueueHolder clients have been shut down");
        }
    }
}
