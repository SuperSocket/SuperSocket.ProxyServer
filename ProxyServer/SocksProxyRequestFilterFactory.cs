using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.ProxyServer
{
    class SocksProxyRequestFilterFactory : IRequestFilterFactory<BinaryRequestInfo>
    {
        private IRequestFilter<BinaryRequestInfo> m_SwitchRequestFilter = new SocksSwitchRequestFilter();

        public IRequestFilter<BinaryRequestInfo> CreateFilter(IAppServer appServer, ISocketSession socketSession)
        {
            return m_SwitchRequestFilter;
        }
    }
}
