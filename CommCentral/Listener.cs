using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CommCentral.SocketAccept
{
    public enum ListenerType { QueueHolder, Publisher, Subscriber };

    public class Listener
    {
        private ListenerType m_TypeEnum;
        private string m_TypeStr;
        private TcpListener m_Listener;
        private IPEndPoint m_EndPoint;
        private Thread m_Worker;
        private StringBuilder m_StatusStream;

        public Listener(ListenerType listenerType, System.Net.IPEndPoint endPoint, StringBuilder statusStream, bool startThreadImmediately = true)
        {
            m_TypeEnum = listenerType;
            m_TypeStr = Enum.GetName(typeof(ListenerType), listenerType);
            m_EndPoint = endPoint;
            m_StatusStream = statusStream;

            m_Worker = new Thread(this.Run);
            if (startThreadImmediately)
                m_Worker.Start();
        }

        private void Run()
        {
            UpdateStatus("Worker thread is now running");

            m_Listener = new TcpListener(m_EndPoint);
            m_Listener.Start();
            UpdateStatus("Listener started");

            while (m_StayAlive)
            {
                UpdateStatus("Waiting for connection request on port " + m_EndPoint.Port);
                try
                {
                    m_Listener.AcceptSocket();
                }
                catch (SocketException ex)
                {
                    UpdateStatus(ex.Message);
                    continue;
                }
                UpdateStatus("Received connection request");

                //TODO: Process incoming connection request
                UpdateStatus("TODO - Process incoming connection request");
            }
            //Clean up
            m_Listener.Stop();
            UpdateStatus("Listener stopped");
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
            UpdateStatus("Worker has terminated");
        }
        public bool Join(int msTimeout)
        {
            if (m_Worker.Join(msTimeout))
            {
                UpdateStatus("Worker has terminated");
                return true;
            }
            //else
            UpdateStatus("Thread join timed out, worker has gone rogue!");
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateStatus(string message)
        {
            m_StatusStream.AppendLine(DateTime.Now.ToString("M/d/yy H:mm:ss.ffff") + " -- " + message);
        }
    }
}
