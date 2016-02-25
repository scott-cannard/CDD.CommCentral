using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CDD.CommCentral.Clients
{
    public partial class ClientManager
    {
        public void OnMessageHostReceive(IAsyncResult ar)
        {
            Socket s = (Socket)ar.AsyncState;
            IPEndPoint remoteEP  = (IPEndPoint)s.RemoteEndPoint;

            if (s.Connected)
            {
                try
                {
                    if (s.EndReceive(ar) < 1)
                    {
                        m_Logger.Record(String.Format("Received shutdown signal from MessageHost[{0}:{1}]; Closing connection", remoteEP.Address, remoteEP.Port));
                        lock (listMessageHostsLock)
                        {
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                            RemoveClient(remoteEP);
                        }
                    }
                    else
                    {
                        //Feed incoming raw data into message-buffer stream
                        //If the stream contains <EOM> delimiter, signal the message processing thread
                        /*
                         * TODO: .NET package "Disruptor" looks like a good technology to use here
                         */
                    }
                }
                catch (SocketException sEx)
                {
                    if (sEx.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        m_Logger.Record(String.Format("Connection was forcibly closed by client MessageHost[{0}:{1}]", remoteEP.Address, remoteEP.Port));
                        RemoveClient(remoteEP);
                    }
                    else
                        m_Logger.Record(sEx.ErrorCode.ToString() + ": " + sEx.Message);
                }
                catch (Exception ex)
                {
                    m_Logger.Record(ex.Message);
                }        
            }
        }
    }
}
