using CDD.CommCentral.Clients;
using CDD.CommCentral.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CDD.CommCentral
{
    class Program
    {
        private static readonly int LISTENPORT = 790;
        internal static ManualResetEvent connectionAccepted = new ManualResetEvent(false);

        private static ConsoleLogger Logger_CnxnMgr = new ConsoleLogger(0, "Connection Manager:", 20, true);
        private static ConsoleLogger Logger_MHClients = new ConsoleLogger(23, "MessageHost Clients:", 10);

        private static ClientManager clientManager = new ClientManager(Logger_CnxnMgr, Logger_MHClients);

        static void Main(string[] args)
        {
            Console.BufferWidth = 120;
            Console.SetWindowSize(120, 40);
            Console.Title = "CDD.CommCentral Connection Manager";

            Thread listenerThread = new Thread(() => {
                IPEndPoint listenerEP = new IPEndPoint(IPAddress.Loopback, LISTENPORT);
                TcpListener listener = new TcpListener(listenerEP);
                listener.Start();
                Logger_CnxnMgr.Record(String.Format("TcpListener started; Waiting for connections [{0}:{1}]>--", listenerEP.Address, listenerEP.Port));

                bool continueListening = true;
                while (continueListening)
                {
                    connectionAccepted.Reset();
                    listener.BeginAcceptTcpClient(new AsyncCallback(clientManager.OnAccept), listener);
                    
                    while (!connectionAccepted.WaitOne(100))
                    {
                        if (ConsoleExtensions.CheckForKeypress(ConsoleKey.Escape))
                        {
                            listener.Stop();
                            continueListening = false;
                            break;
                        }
                    }
                }
            });
            listenerThread.Start();
            Thread.Sleep(1);

            //Wait for listener thread to terminate, then shutdown
            listenerThread.Join();
            clientManager.GracefulShutdown();


            //User-friendly pause before exiting debug mode
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.Out.Write("Press <Enter> to quit");
                Console.In.ReadLine();
            }
        }
    }
}
