using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDD.CommCentral.Clients
{
    public interface IClient
    {
        void OnReceive(IAsyncResult ar);
    }
}
