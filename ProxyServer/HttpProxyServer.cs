using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    public class HttpProxyServer : ProxyAppServer
    {
        public HttpProxyServer()
            : base(new HttpProxyReceiveFilterFactory())
        {

        }

        protected override void OnNewSessionConnected(ProxySession session)
        {
            session.Type = ProxyType.Http;
            base.OnNewSessionConnected(session);
        }
    }
}
