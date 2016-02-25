using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDD.CommCentral
{
    public class CDDMessage
    {
        public static string Format(string[] tokens)
        {
            StringBuilder sb = new StringBuilder();

            if (tokens.Length > 1)
                sb.Append("*\r\n" + tokens.Length.ToString());

            foreach (string t in tokens)
            {
                sb.Append("\r\n$\r\n" + t.Length.ToString() + "\r\n" + t);
            }

            return sb.ToString();
        }
    }
}
