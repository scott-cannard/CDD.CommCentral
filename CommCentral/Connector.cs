using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommCentral.Connection
{
    public enum ConnectorType { QueueHolder, Publisher, Subscriber };

    public class Connector
    {
        private ConnectorType m_TypeEnum;
        private string m_TypeStr;
        private TcpListener m_Listener;
        private IPEndPoint m_EndPoint;
        private Thread m_Worker;

        public Connector(ConnectorType connectorType, System.Net.IPEndPoint endPoint, bool startThreadImmediately = true)
        {
            m_TypeEnum = connectorType;
            m_TypeStr = Enum.GetName(typeof(ConnectorType), connectorType);
            m_EndPoint = endPoint;

            m_Worker = new Thread(this.Run);

            if (startThreadImmediately)
                m_Worker.Start();
        }

        private void Run()
        {
            m_Listener = new TcpListener(m_EndPoint);
            m_Listener.Start();
            Program.Print(m_TypeStr + ": Listener started\n");

            while (m_StayAlive)
            {
                Program.Print(m_TypeStr + ": Waiting for connection request on port " + m_EndPoint.Port + "\n");
                try
                {
                    m_Listener.AcceptSocket();
                }
                catch (SocketException ex)
                {
                    Program.Print(m_TypeStr + ": " + ex.Message + '\n');
                    continue;
                }
                Program.Print(m_TypeStr + ": Received connection request\n");
                Program.Print(m_TypeStr + ": TODO - Process incoming connection request\n");
            }
            //Clean up
            m_Listener.Stop();
            Program.Print(m_TypeStr + ": Listener stopped\n");
        }

        private volatile bool m_StayAlive = true;
        public void RequestStop()
        {
            m_StayAlive = false;
            m_Listener.Stop();
        }
        public void Join()
        {
            m_Worker.Join();
            Program.Print(m_TypeStr + ": Worker has terminated\n");
        }
        public bool Join(int msTimeout)
        {
            if (m_Worker.Join(msTimeout))
            {
                Program.Print(m_TypeStr + ": Worker has terminated\n");
                return true;
            }
            //else
            Program.Print(m_TypeStr + ": Thread join timed out, worker has gone rogue!");
            return false;
        }
    }
}
