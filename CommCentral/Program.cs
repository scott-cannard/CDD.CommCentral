using CommCentral.Connection;
using System;
using System.Net;

namespace CommCentral
{
    class Program
    {
        const int PORT_QUEUEHOLDER = 790;
        const int PORT_PUBLISHER   = 791;
        const int PORT_SUBSCRIBER  = 792;

        static void Main(string[] args)
        {
            Program.Print("Main: Beginning execution\n");

            //Listen for QueueHolders
            Connector qholderHandler = new Connector(ConnectorType.QueueHolder, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_QUEUEHOLDER));
            //Listen for Publishers
            Connector publisherHandler = new Connector(ConnectorType.Publisher, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_PUBLISHER));
            //Listen for Subscribers
            Connector subscriberHandler = new Connector(ConnectorType.Subscriber, new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SUBSCRIBER));

            while (!Console.KeyAvailable) ;
            while (Console.KeyAvailable)
                Console.ReadKey(true);

            publisherHandler.RequestStop();
            subscriberHandler.RequestStop();
            publisherHandler.Join();
            subscriberHandler.Join();

            qholderHandler.RequestStop();
            qholderHandler.Join();

            Program.Print("Main: Terminating\n\n**Press <ENTER> to quit**");
            Console.In.ReadLine();
        }

        public static void Print(string content)
        {
            Console.Out.Write(content);
        }
    }
}
