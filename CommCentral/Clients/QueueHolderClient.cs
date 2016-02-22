using CDD.CommCentral.Connection;
using System;
using System.Net;
using System.Net.Sockets;

namespace CDD.CommCentral.Clients
{
    public class QueueHolderClient : ClientBase
    {
        public QueueHolderClient(ClientManager_QueueHolders manager, TcpClient tcpClient)
            : base(manager, tcpClient)
        { }

        public void OnReceive(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            int bytesReceived = socket.EndReceive(ar);
            if (bytesReceived < 1)
            {
                this.ClientManager.Log(String.Format("Received shutdown message, Socket.SHUTDOWN --> QueueHolder({0}:{1})", ((IPEndPoint)socket.RemoteEndPoint).Address.ToString(), ((IPEndPoint)socket.RemoteEndPoint).Port));
                this.ClientManager.RemoveClient(this.HostEP);
            }
            else
            {
                //Read buffer, act accordingly
                //switch ()
                //{
                //    default:
                //        break;
                //}
            }
        }
    }
}
