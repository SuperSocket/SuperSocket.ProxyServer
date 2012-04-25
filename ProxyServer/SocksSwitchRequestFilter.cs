using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.ProxyServer
{
    class SocksSwitchRequestFilter : IRequestFilter<BinaryRequestInfo>
    {
        public BinaryRequestInfo Filter(IAppSession<BinaryRequestInfo> session, byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = length;

            var proxySession = session as ProxySession;

            var version = readBuffer[offset];

            if (version == 0x04)
                proxySession.SetNextRequestFilter(new Socks4ProxyRequestFilter());
            else if (version == 0x05)
                proxySession.SetNextRequestFilter(new Socks5ProxyRequestFilter());
            else
            {
                session.Logger.Error(session, string.Format("Invalid version: {0}", version));
                session.Close(CloseReason.ProtocolError);
                left = 0;
                return null;
            }

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
