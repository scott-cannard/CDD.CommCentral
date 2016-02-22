using CDD.CommCentral.Connection;
using System;
using System.Net;
using System.Net.Sockets;

namespace CDD.CommCentral.Clients
{
    public abstract class ClientBase
    {
        protected const uint DEFAULT_BUFFER_SIZE = 65536;

        public ClientManagerBase ClientManager { get; protected set; }
        public TcpClient TcpClient { get; protected set; }
        public Socket Socket { get { return this.TcpClient.Client; } }
        public IPEndPoint HostEP { get { return (IPEndPoint)TcpClient.Client.RemoteEndPoint; } }
        public byte[] RxBuffer { get; protected set; }
        public byte[] TxBuffer { get; protected set; }
        public int PublishersServed { get; protected set; }

        public ClientBase(ClientManagerBase manager, TcpClient tcpClient)
        {
            this.ClientManager = manager;
            this.TcpClient = tcpClient;
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, new LingerOption(false, 1));
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            this.RxBuffer = new byte[DEFAULT_BUFFER_SIZE];
            this.TxBuffer = new byte[DEFAULT_BUFFER_SIZE];
            this.PublishersServed = 0;
        }

        protected void ResizeRxBuffer(uint size)
        {
            RxBuffer = new byte[size];
        }

        public virtual void OnReceive()
        { throw new NotImplementedException("Interface Client.OnReceive() is missing."); }
    }
}
