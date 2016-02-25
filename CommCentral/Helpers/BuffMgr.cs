using System.Text;

namespace CDD.CommCentral
{
    internal class BuffMgr
    {
        private const int DEFAULT_BUFFER_LENGTH = 8196;

        public byte[] rawBytes = new byte[DEFAULT_BUFFER_LENGTH];
        public StringBuilder builder = new StringBuilder();
    }
}
