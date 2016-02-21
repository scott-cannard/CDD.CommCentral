using CommCentral.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CommCentral
{
    class Program
    {
        const int PORT_QUEUEHOLDER = 790;
        const int PORT_PUBLISHER   = 791;
        const int PORT_SUBSCRIBER  = 792;

        private static EventLogger qholderLogger = new EventLogger(new LogDisplayer(0, "QueueHolder Listener:").PrintLog, 10);
        private static EventLogger publisherLogger = new EventLogger(new LogDisplayer(13, "Publisher Listener:").PrintLog, 10);
        private static EventLogger subscriberLogger = new EventLogger(new LogDisplayer(26, "Subscriber Listener:").PrintLog, 10);

        static void Main(string[] args)
        {
            Console.BufferWidth = 100;
            Console.SetWindowSize(100, 40);
            Console.Title = "CDD.CommCentral Connection Manager";

            //Create listeners
            Listener qholderListener = new Listener(ListenerType.QueueHolder, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_QUEUEHOLDER), qholderLogger);
            Listener publisherListener = new Listener(ListenerType.Publisher, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_PUBLISHER), publisherLogger);
            Listener subscriberListener = new Listener(ListenerType.Subscriber, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SUBSCRIBER), subscriberLogger);

            //Spin
            while (!ConsoleExtensions.CheckForKeypress(ConsoleKey.Escape))
            {
                Thread.Sleep(100);
            }

            //Server shutdown
            publisherListener.RequestStop();
            subscriberListener.RequestStop();
            publisherListener.Join();
            subscriberListener.Join();
            qholderListener.RequestStop();
            qholderListener.Join();

            //User-friendly pause before exiting debug mode
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Out.Write("Press <Enter> to quit");
            Console.In.ReadLine();
        }
    }
}
