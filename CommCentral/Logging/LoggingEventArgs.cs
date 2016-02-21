using System;

namespace CommCentral.Logging
{
    public class LoggingEventArgs : EventArgs
    {
        public string[] EventsArray { get; set; }
    }
}
