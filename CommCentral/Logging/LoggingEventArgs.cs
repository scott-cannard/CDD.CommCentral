using System;

namespace CDD.CommCentral.Logging
{
    public class LoggingEventArgs : EventArgs
    {
        public string[] StringArray { get; set; }
        public uint DisplayHeight { get; set; }
    }
}
