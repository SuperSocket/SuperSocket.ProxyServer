using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    class ProxyDataReceiveFilter : IReceiveFilter<BinaryRequestInfo>
    {
        private ProxySession m_Session;

        public ProxyDataReceiveFilter(ProxySession session)
        {
            m_Session = session;
        }

        public BinaryRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            left = 0;
            m_Session.RequestDataReceived(readBuffer, offset, length);
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

        }

        public FilterState State { get; private set; }
    }
}
