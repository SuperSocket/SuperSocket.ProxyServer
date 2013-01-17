using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SuperSocket.ClientEngine;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.ProxyServer
{
    public class ProxySession : AppSession<ProxySession, BinaryRequestInfo>
    {
        private TcpClientSession m_TargetSession;
        private Action<ProxySession, TcpClientSession> m_ConnectedAction;

        private ArraySegment<byte> m_BufferSegment;

        public new ProxyAppServer AppServer
        {
            get
            {
                return (ProxyAppServer)base.AppServer;
            }
        }

        public ProxyType Type { get; internal set; }

        public string UserID { get; internal set; }

        internal protected new void SetNextReceiveFilter(IReceiveFilter<BinaryRequestInfo> receiveFilter)
        {
            base.SetNextReceiveFilter(receiveFilter);
        }

        internal void ConnectTarget(EndPoint remoteEndPoint, Action<ProxySession, TcpClientSession> connectedAction)
        {
            m_ConnectedAction = connectedAction;
            var targetSession = new AsyncTcpSession(remoteEndPoint);
            targetSession.ReceiveBufferSize = 2000000;
            targetSession.Connected += targetSession_Connected;
            targetSession.Closed += targetSession_Closed;
            targetSession.DataReceived += targetSession_DataReceived;
            targetSession.Error += targetSession_Error;

            var buffer = AppServer.RequestProxyBuffer();

            if (buffer.Array == null)
            {
                this.Close(CloseReason.ServerClosing);
                return;
            }

            m_BufferSegment = buffer;
            targetSession.Connect();
        }

        void targetSession_Error(object sender, ErrorEventArgs e)
        {
            Logger.Error(e.Exception);

            var client = (AsyncTcpSession)sender;

            if (!client.IsConnected)
            {
                var connectedAction = m_ConnectedAction;
                m_ConnectedAction = null;
                connectedAction(this, null);
                this.Close();
            }
        }

        void targetSession_DataReceived(object sender, DataEventArgs e)
        {
            if (!this.Connected)
                return;

            try
            {
                this.Send(e.Data, e.Offset, e.Length);
            }
            catch
            {

            }
        }

        void targetSession_Closed(object sender, EventArgs e)
        {
            if (this.Connected)
            {
                m_TargetSession = null;
                this.Close();
                return;
            }
        }

        void targetSession_Connected(object sender, EventArgs e)
        {
            m_TargetSession = (AsyncTcpSession)sender;
            var connectedAction = m_ConnectedAction;
            m_ConnectedAction = null;
            connectedAction(this, m_TargetSession);
        }

        internal void RequestDataReceived(byte[] buffer, int offset, int length)
        {
            if (m_TargetSession == null)
            {
                Logger.Error("Cannot receive data before when target socket is connected");
                this.Close();
                return;
            }

            m_TargetSession.Send(buffer, offset, length);
        }

        protected override void OnSessionClosed(CloseReason reason)
        {
            if(m_BufferSegment.Array != null)
                AppServer.PushProxyBuffer(m_BufferSegment);

            if (m_TargetSession != null)
                m_TargetSession.Close();
        }
    }
}
