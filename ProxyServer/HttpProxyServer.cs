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
            : base(new DefaultRequestFilterFactory<HttpProxyRequestFilter, BinaryRequestInfo>())
        {

        }

        public override IAppSession CreateAppSession(ISocketSession socketSession)
        {
            var proxySession = base.CreateAppSession(socketSession) as ProxySession;

            if (proxySession != null)
                proxySession.Type = ProxyType.Http;

            return proxySession;
        }
    }
}
