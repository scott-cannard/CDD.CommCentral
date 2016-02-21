using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommCentral.Logging
{
    public class LogDisplayer
    {
        private static object consoleWriteLock = new object();
        private int m_CursorStartRow;
        private string m_Header;

        public LogDisplayer(int row, string header)
        {
            m_CursorStartRow = row;
            m_Header = header;
        }

        public void PrintLog(Object sender, EventArgs e)
        {
            string[] eventRecords = (e as LoggingEventArgs).EventsArray;

            lock (consoleWriteLock)
            {
                Console.SetCursorPosition(0, m_CursorStartRow);
                Console.Out.WriteLine(m_Header);
                for (int i = 0; i < 10; i++)
                    Console.Out.WriteLine((eventRecords.Count() > i ? eventRecords[i] : String.Empty).PadRight(Console.BufferWidth - 1));
            }
        }

    }
}
