using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    class ProxyDataRequestFilter : IRequestFilter<BinaryRequestInfo>
    {
        public BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;
            var proxySession = session as ProxySession;
            proxySession.RequestDataReceived(readBuffer, offset, length);
            return null;
        }

        public int LeftBufferSize
        {
            get { return 0; }
        }

        public IRequestFilter<BinaryRequestInfo> NextRequestFilter
        {
            get { return null; }
        }
    }
}
