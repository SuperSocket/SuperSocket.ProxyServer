using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Facility.Protocol;

namespace SuperSocket .ProxyServer
{
    class Socks4ProxyReceiveFilter : FixedSizeReceiveFilter<BinaryRequestInfo>
    {
        private bool m_GotUserID = false;
        private bool m_FixedHeadLoaded = false;

        private int m_Port;
        private EndPoint m_TargetEndPoint;
        private string m_UserID;

        private ProxySession m_Session;
    
        public Socks4ProxyReceiveFilter(ProxySession session)
            : base(9)
        {
            m_Session = session;
        }

        private ArraySegmentList m_Buffer;

        private void AddSegment(byte[] segment, int offset, int length, bool toBeCopied)
        {
            if (m_Buffer == null)
                m_Buffer = new ArraySegmentList();

            m_Buffer.AddSegment(segment, offset, length, toBeCopied);
        }

        public override BinaryRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int left)
        {
            if (!m_FixedHeadLoaded)
                return base.Filter(readBuffer, offset, length, toBeCopied, out left);

            var session = m_Session;

            left = 0;
            int pos = FindEnd(readBuffer, offset, length);

            if (pos >= 0)
            {
                if (session.Type == ProxyType.Socks4a) //4a
                {
                    if (m_GotUserID)
                    {
                        if (pos != offset)
                            this.AddSegment(readBuffer, offset, pos - offset, toBeCopied);

                        if (m_Buffer == null || m_Buffer.Count <= 0)
                        {
                            session.Close(CloseReason.ProtocolError);
                            return null;
                        }

                        m_TargetEndPoint = new DnsEndPoint(m_Buffer.Decode(Encoding.ASCII), m_Port);
                        TryStartProxy(m_TargetEndPoint);
                        return null;
                    }
                    else
                    {
                        m_GotUserID = true;

                        if (pos != offset)
                            this.AddSegment(readBuffer, offset, pos - offset, toBeCopied);

                        if (m_Buffer == null || m_Buffer.Count <= 0)
                        {
                            m_UserID = string.Empty;
                        }
                        else
                        {
                            m_UserID = m_Buffer.Decode(Encoding.ASCII);
                        }

                        session.UserID = m_UserID;

                        int posHost = FindEnd(readBuffer, pos + 1, offset + length - pos - 1);

                        if (posHost >= 0)
                        {
                            m_TargetEndPoint = new DnsEndPoint(Encoding.ASCII.GetString(readBuffer, pos + 1, posHost - pos - 1), m_Port);
                            TryStartProxy(m_TargetEndPoint);
                            return null;
                        }

                        this.AddSegment(readBuffer, pos + 1, offset + length - pos - 1, toBeCopied);
                        return null;
                    }
                }
                else//4a
                {
                    if (pos != offset)
                        this.AddSegment(readBuffer, offset, pos - offset, toBeCopied);

                    if (m_Buffer == null || m_Buffer.Count <= 0)
                    {
                        m_UserID = string.Empty;
                    }
                    else
                    {
                        m_UserID = m_Buffer.Decode(Encoding.ASCII);
                    }

                    session.UserID = m_UserID;

                    TryStartProxy(m_TargetEndPoint);
                    return null;
                }
            }
            else
            {
                this.AddSegment(readBuffer, offset, length, toBeCopied);
                return null;
            }
        }

        /// <summary>
        /// Processes the fix size request.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="toBeCopied">if set to <c>true</c> [to be copied].</param>
        /// <returns></returns>
        protected override BinaryRequestInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
        {
            m_FixedHeadLoaded = true;

            var session = m_Session;

            if (buffer[offset + 1] != 0x01)
            {
                session.Logger.Error("request[1] is not 0x01");
                State = FilterState.Error;
                return null;
            }

            m_Port = (int)buffer[offset + 3] + (int)buffer[offset + 2] * 256;

            //Socks4
            if (ValidateIPAddress(buffer, offset + 4, 4))
            {
                session.Type = ProxyType.Socks4;

                m_TargetEndPoint = new IPEndPoint(new IPAddress(buffer.CloneRange(offset + 4, 4)), m_Port);

                int pos = FindEnd(buffer, offset + 9, length - offset - 9);

                if (pos > 0)
                {
                    m_GotUserID = true;
                    m_UserID = Encoding.ASCII.GetString(buffer, offset + 9, pos - offset - 9);
                    session.UserID = m_UserID;
                    TryStartProxy(m_TargetEndPoint);
                    return null;
                }

                this.AddSegment(buffer, offset + 9, length - 9, toBeCopied);
                return null;
            }
            else//Socks4a
            {
                session.Type = ProxyType.Socks4a;

                int pos = FindEnd(buffer, offset + 9, length - offset - 9);

                if (pos > 0)
                {
                    m_GotUserID = true;
                    m_UserID = Encoding.ASCII.GetString(buffer, offset + 9, pos - offset - 9);
                    session.UserID = m_UserID;

                    var posHost = FindEnd(buffer, pos + 1, offset + length - pos - 1);

                    if (posHost > 0)
                    {
                        m_TargetEndPoint = new DnsEndPoint(Encoding.ASCII.GetString(buffer, pos + 1, posHost - pos), m_Port);
                        TryStartProxy(m_TargetEndPoint);
                        return null;
                    }

                    this.AddSegment(buffer, pos + 1, offset + length - pos - 1, toBeCopied);
                    return null;
                }

                this.AddSegment(buffer, offset + 9, length - offset - 9, toBeCopied);
                return null;
            }
        }

        private bool ValidateIPAddress(IList<byte> ipAddress, int offset, int length)
        {
            int maxOffset = offset + length - 1;

            for (int i = 1; i < 4; i++)
            {
                if (ipAddress[maxOffset - i] != 0x00)
                    return true;
            }

            return false;
        }

        private int FindEnd(byte[] source, int offset, int length)
        {
            for (var i = offset; i < offset + length; i++)
            {
                if (source[i] == 0x00)
                {
                    return i;
                }
            }

            return -1;
        }

        private void TryStartProxy(EndPoint remoteEndPoint)
        {
            m_Session.ConnectTarget(remoteEndPoint, ProxyConnectedHandle);
        }

        private void ProxyConnectedHandle(ProxySession session, SuperSocket.ClientEngine.TcpClientSession targetSession)
        {
            //if (targetSession == null)
            //    session.SendResponse(m_FailedResponse, 0, m_FailedResponse.Length);
            //else
            //    session.SendResponse(m_OkResponse, 0, m_OkResponse.Length);
        }
    }
}
