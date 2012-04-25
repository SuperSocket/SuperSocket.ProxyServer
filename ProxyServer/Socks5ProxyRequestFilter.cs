using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.ProxyServer
{
    class Socks5ProxyRequestFilter : IRequestFilter<BinaryRequestInfo>
    {
        public BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            throw new NotImplementedException();
        }

        public int LeftBufferSize
        {
            get { throw new NotImplementedException(); }
        }

        public IRequestFilter<BinaryRequestInfo> NextRequestFilter
        {
            get { throw new NotImplementedException(); }
        }
    }
}
