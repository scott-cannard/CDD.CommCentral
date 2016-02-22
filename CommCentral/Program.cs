using CDD.CommCentral.Connection;
using CDD.CommCentral.Logging;
using System;
using System.Net;
using System.Threading;

namespace CDD.CommCentral
{
    class Program
    {
        const int LISTENPORT_QUEUEHOLDER = 790;
        const int LISTENPORT_PUBLISHER   = 791;
        const int LISTENPORT_SUBSCRIBER  = 792;

        private static EventLogger Logger_CnxnMgr = new EventLogger(new LogDisplayer(0, "Connection Manager:").PrintLog, 20);
        private static EventLogger Logger_QHClients = new EventLogger(new LogDisplayer(23, "QueueHolder Clients:").PrintLog, 10);
        //private static EventLogger Logger_PubClients = new EventLogger(new LogDisplayer(13, "Publisher Clients:").PrintLog, 10);
        //private static EventLogger Logger_SubClients = new EventLogger(new LogDisplayer(26, "Subscriber Clients:").PrintLog, 10);

        static void Main(string[] args)
        {
            Console.BufferWidth = 120;
            Console.SetWindowSize(120, 40);
            Console.Title = "CDD.CommCentral Connection Manager";

            //Create listeners
            Listener SM_QueueHolders = new Listener(new IPEndPoint(IPAddress.Loopback, LISTENPORT_QUEUEHOLDER),
                                                              Component.QueueHolder,
                                                              Logger_CnxnMgr, Logger_QHClients);
            Logger_QHClients.Replace("");

            Listener SM_Publishers = new Listener(new IPEndPoint(IPAddress.Loopback, LISTENPORT_PUBLISHER),
                                                            Component.Publisher,
                                                            Logger_CnxnMgr, null);

            Listener SM_Subscribers = new Listener(new IPEndPoint(IPAddress.Loopback, LISTENPORT_SUBSCRIBER),
                                                             Component.Subscriber,
                                                             Logger_CnxnMgr, null);
            
            //Spin
            //Thread.CurrentThread.Suspend();
            while (!ConsoleExtensions.CheckForKeypress(ConsoleKey.Escape))
            {
                Thread.Sleep(100);
            }

            //Server shutdown
            SM_Publishers.Shutdown();
            SM_Subscribers.Shutdown();
            SM_QueueHolders.Shutdown();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                //User-friendly pause before exiting debug mode
                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                Console.Out.Write("Press <Enter> to quit");
                Console.In.ReadLine();
            }
        }
    }
}
