using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    public abstract class ProxyAppServer : AppServer<ProxySession, BinaryRequestInfo>
    {
        private ConcurrentStack<ArraySegment<byte>> m_BufferPool;

        private ArraySegment<byte> m_NullArraySegment = new ArraySegment<byte>();

        public ProxyAppServer(IRequestFilterFactory<BinaryRequestInfo> requestFilterFactory)
            : base(requestFilterFactory)
        {

        }

        public override bool Setup(IRootConfig rootConfig, IServerConfig config, ISocketServerFactory socketServerFactory, IRequestFilterFactory<BinaryRequestInfo> requestFilterFactory)
        {
            if (!base.Setup(rootConfig, config, socketServerFactory, requestFilterFactory))
                return false;

            int proxyReceiveBufferSize = config.ReceiveBufferSize;

            var buffer = new byte[proxyReceiveBufferSize * config.MaxConnectionNumber];

            var bufferList = new List<ArraySegment<byte>>(config.MaxConnectionNumber);

            for (var i = 0; i < config.MaxConnectionNumber; i++)
            {
                bufferList.Add(new ArraySegment<byte>(buffer, i * proxyReceiveBufferSize, proxyReceiveBufferSize));
            }

            m_BufferPool = new ConcurrentStack<ArraySegment<byte>>(bufferList);

            return true;
        }

        internal ArraySegment<byte> RequestProxyBuffer()
        {
            ArraySegment<byte> buffer;
            if (m_BufferPool.TryPop(out buffer))
                return buffer;

            Logger.Error("No enougth proxy buffer segment!");
            return m_NullArraySegment;
        }

        internal void PushProxyBuffer(ArraySegment<byte> buffer)
        {
            m_BufferPool.Push(buffer);
        }
    }
}
