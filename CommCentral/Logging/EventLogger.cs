using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommCentral.Logging
{
    public class EventLogger
    {
        private StringBuilder m_EventLog;
        private event EventHandler m_RecordEventHandler;
        private int m_NumberOfEventsToReturn;

        public EventLogger(EventHandler handler, int numberOfEventsToReturn)
        {
            m_EventLog = new StringBuilder();
            m_RecordEventHandler += handler;
            m_NumberOfEventsToReturn = numberOfEventsToReturn;
        }

        public void Record(string eventString)
        {
            m_EventLog.AppendLine(eventString);
            m_RecordEventHandler(this, new LoggingEventArgs { EventsArray = GetLast(m_NumberOfEventsToReturn) });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetEvents()
        {
            return m_EventLog.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetLast(int numberOfEvents)
        {
            List<string> allLines = GetEvents().ToList();
            return allLines.Skip(Math.Max(0, allLines.Count - numberOfEvents)).ToArray();
        }

        public override string ToString()
        {
            return m_EventLog.ToString();
        }
    }
}
