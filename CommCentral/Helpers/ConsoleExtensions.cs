using System;

namespace CDD.CommCentral
{
    public static class ConsoleExtensions
    {
        /// <summary>
        /// WARNING: Consumes all data in the input stream up to and including the expected keypress,
        ///          or the entire stream if the expected key was not pressed.
        ///          In other words, this function enables ONLY ONE key to be recognized as input, any
        ///          other keypress will be consumed before you can check for it.
        /// </summary>
        public static bool CheckForKeypress(ConsoleKey expectedKey)
        {
            while (Console.KeyAvailable)
            {
                if (Console.ReadKey(true).Key.Equals(expectedKey))
                    return true;
            }
            return false;
        }
    }
}
