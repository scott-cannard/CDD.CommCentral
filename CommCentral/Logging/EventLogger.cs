using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CDD.CommCentral.Logging
{
    public class EventLogger
    {
        private StringBuilder m_EventLog;
        private event EventHandler m_OnChange;
        public uint DisplayHeight { get; set; }

        public EventLogger(EventHandler handler, uint displayHeight)
        {
            m_EventLog = new StringBuilder();
            m_OnChange += handler;
            DisplayHeight = displayHeight;
        }

        public void Record(string eventString)
        {
            m_EventLog.AppendLine(DateTime.Now.ToString("M/d/yy H:mm:ss.ffff") + " -- " + eventString);
            m_OnChange(this, new LoggingEventArgs { StringArray = GetLast(DisplayHeight), DisplayHeight = this.DisplayHeight });
        }

        public void Replace(string content)
        {
            m_EventLog.Clear();
            m_EventLog.Append(content);
            m_OnChange(this, new LoggingEventArgs { StringArray = GetEvents((int)DisplayHeight), DisplayHeight = this.DisplayHeight });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetEvents(int? n = null)
        {
            return (n != null && n >= 0) ? m_EventLog.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Take((int)n).ToArray()
                                         : m_EventLog.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string[] GetLast(uint numberOfEvents)
        {
            List<string> allLines = GetEvents().ToList();
            return allLines.Skip(Math.Max(0, allLines.Count - (int)numberOfEvents)).ToArray();
        }

        public override string ToString()
        {
            return m_EventLog.ToString();
        }
    }
}
