using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QueueHolder
{
    class Program
    {
        static IPEndPoint commCentralEP = new IPEndPoint(IPAddress.Loopback, 790);

        static void Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IPAddress.Loopback, 789));
            tcpClient.Connect(commCentralEP);

            if (tcpClient.Connected)
                Console.Out.WriteLine("Connection established.");

            Console.In.ReadLine();
        }
    }
}
