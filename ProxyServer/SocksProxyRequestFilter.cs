using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.ProxyServer
{
    class SocksProxyRequestFilter : RequestFilterBase<BinaryRequestInfo>
    {
        public override BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            throw new NotImplementedException();
        }
    }
}
