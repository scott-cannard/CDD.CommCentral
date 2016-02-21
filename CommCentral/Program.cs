using CommCentral.SocketAccept;
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

        private static StringBuilder sbQueueHolder = new StringBuilder();
        private static StringBuilder sbPublisher = new StringBuilder();
        private static StringBuilder sbSubscriber = new StringBuilder();

        static void Main(string[] args)
        {
            Console.BufferWidth = 100;
            Console.SetWindowSize(100, 40);
            Console.Title = "CDD.CommCentral Connection Manager";

            //Listen for QueueHolders
            Listener qholderListener = new Listener(ListenerType.QueueHolder, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_QUEUEHOLDER), sbQueueHolder);
            //Listen for Publishers
            Listener publisherListener = new Listener(ListenerType.Publisher, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_PUBLISHER), sbPublisher);
            //Listen for Subscribers
            Listener subscriberListener = new Listener(ListenerType.Subscriber, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SUBSCRIBER), sbSubscriber);

            while (!ConsoleExtensions.CheckForKeypress(ConsoleKey.Escape))
            {
                DisplayListenerStatus();
                Thread.Sleep(250);
            }

            //Server shutdown
            publisherListener.RequestStop();
            subscriberListener.RequestStop();
            publisherListener.Join();
            subscriberListener.Join();
            qholderListener.RequestStop();
            qholderListener.Join();

            DisplayListenerStatus();
            Console.Out.Write("\n\nPress <Enter> to quit");
            Console.In.ReadLine();
        }

        private static void DisplayListenerStatus()
        {
            List<string> allLines = null;
            IEnumerable<string> lastNLines = null;

            Console.Clear();
            
            //Display qholderListener status
            Console.Out.WriteLine("QueueHolder Listener:");
            allLines = sbQueueHolder.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            lastNLines = allLines.Skip(Math.Max(0, allLines.Count - 10));
            for (int i = 0; i < 10; i++)
                Console.Out.WriteLine(lastNLines.Count() > i ? lastNLines.ElementAt(i) : String.Empty);
            
            //Display publisherListener status
            Console.Out.WriteLine("\n\nPublisher Listener:");
            allLines = sbPublisher.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            lastNLines = allLines.Skip(Math.Max(0, allLines.Count - 10));
            for (int i = 0; i < 10; i++)
                Console.Out.WriteLine(lastNLines.Count() > i ? lastNLines.ElementAt(i) : String.Empty);

            //Display subscriberListener status
            Console.Out.WriteLine("\n\nSubscriber Listener:");
            allLines = sbSubscriber.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            lastNLines = allLines.Skip(Math.Max(0, allLines.Count - 10));
            for (int i = 0; i < 10; i++)
                Console.Out.WriteLine(lastNLines.Count() > i ? lastNLines.ElementAt(i) : String.Empty);
        }
    }
}
