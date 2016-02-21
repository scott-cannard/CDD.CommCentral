using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommCentral
{
    public static class ConsoleExtensions
    {
        public static bool CheckForKeypress(ConsoleKey expectedKey)
        {
            while (Console.KeyAvailable)
            {
                //Keys pressedKey = (Keys)Console.ReadKey(true).Key;
                if (Console.ReadKey(true).Key.Equals(expectedKey))
                    return true;
            }
            return false;
        }
    }
}
