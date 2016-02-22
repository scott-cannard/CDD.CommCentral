using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDD.CommCentral.Connection
{
    interface IClientManager
    {
        void OnAccept(IAsyncResult ar);
        void Shutdown();
    }
}
