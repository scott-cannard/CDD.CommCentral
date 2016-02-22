using CDD.CommCentral.Connection;
using CDD.CommCentral.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace CDD.CommCentral
{
    public class Listener
    {
        private TcpListener m_Listener;
        private ClientManagerBase m_ClientMgr;
        private EventLogger m_Logger;
        private Component m_ComponentType;

        public Listener(IPEndPoint endpoint, Component componentType, EventLogger logger, EventLogger clientLog)
        {
            m_Listener = new TcpListener(endpoint);
            m_ClientMgr = ClientManagerBase.GetInstance(componentType, logger, clientLog);
            m_Logger = logger;
            m_ComponentType = componentType;
            m_Logger.Record(String.Format("ListenerManager<{0}> initialized", componentType));

            m_Listener.Start();
            m_Logger.Record(String.Format("ListenerManager<{0}> TcpListener started", componentType));

            m_Listener.BeginAcceptTcpClient(new AsyncCallback(m_ClientMgr.OnAccept), m_Listener);
            m_Logger.Record(String.Format("ListenerManager<{0}> now accepting clients on port {1}", componentType, endpoint.Port));
        }

        public void Shutdown()
        {
            m_Listener.Stop();
            m_Logger.Record(String.Format("ListenerManager<{0}> TcpListener stopped", m_ComponentType));
            m_Listener.Server.Close();

            m_ClientMgr.Shutdown();
        }
    }
}
