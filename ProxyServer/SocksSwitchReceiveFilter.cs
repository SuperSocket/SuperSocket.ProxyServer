using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.SocketBase;

namespace SuperSocket.ProxyServer
{
    class SocksSwitchReceiveFilter : IReceiveFilter<BinaryRequestInfo>
    {
        private ProxySession m_Session;

        public SocksSwitchReceiveFilter(ProxySession session)
        {
            m_Session = session;
        }

        public BinaryRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            var session = m_Session;
            left = length;

            var version = readBuffer[offset];

            if (version == 0x04)
                session.SetNextReceiveFilter(new Socks4ProxyReceiveFilter(session));
            else if (version == 0x05)
                session.SetNextReceiveFilter(new Socks5ProxyReceiveFilter());
            else
            {
                session.Logger.Error(session, string.Format("Invalid version: {0}", version));
                left = 0;
                State = FilterState.Error;
                return null;
            }

            return null;
        }

        public int LeftBufferSize
        {
            get { return 0; }
        }

        public IReceiveFilter<BinaryRequestInfo> NextReceiveFilter
        {
            get { return null; }
        }


        public void Reset()
        {
            throw new NotImplementedException();
        }

        public FilterState State { get; private set; }
    }
}
