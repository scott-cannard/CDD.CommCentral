using CDD.CommCentral.Logging;
using System;
using System.Net.Sockets;

namespace CDD.CommCentral.Connection
{
    public class ClientManager_Subscribers : ClientManagerBase
    {
        public ClientManager_Subscribers(EventLogger logger, EventLogger clientLog)
        {
            m_Logger = logger;
            m_ClientLog = clientLog;
        }

        public override void OnAccept(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            //TcpClient newSubscriber;

            if (listener.Server != null && listener.Server.IsBound)
            {
                base.OnAccept(ar);
            }
        }

        public override void Shutdown()
        {
            
        }
    }
}
