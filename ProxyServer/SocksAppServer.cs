using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    public class SocksProxyServer : ProxyAppServer
    {
        public SocksProxyServer()
            : base(new SocksProxyRequestFilterFactory())
        {

        }
    }
}
