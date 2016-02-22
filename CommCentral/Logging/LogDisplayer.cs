using System;
using System.Linq;

namespace CDD.CommCentral.Logging
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
            string[] lines = (e as LoggingEventArgs).StringArray;
            uint maxLines = (e as LoggingEventArgs).DisplayHeight;

            lock (consoleWriteLock)
            {
                Console.SetCursorPosition(0, m_CursorStartRow);
                Console.Out.WriteLine(m_Header);
                for (int i = 0; i < maxLines; i++)
                {
                    if (i < lines.Count())
                        Console.Out.WriteLine(lines[i].PadRight(Console.BufferWidth - 1));
                    else
                        Console.Out.WriteLine(new String(' ', Console.BufferWidth - 1));
                }
            }
        }

    }
}
