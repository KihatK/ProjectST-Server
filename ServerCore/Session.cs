using System.Net.Sockets;
using System.Net;

namespace ServerCore {
    public abstract class PacketSession : Session {
        public static readonly int HeaderSize = 2;

        public sealed override int OnRecv(ArraySegment<byte> buffer, ref bool success) {
            int processLen = 0;

            while (true) {
                //Header를 읽을 수조차 없으면 break
                if (buffer.Count < HeaderSize) {
                    break;
                }

                //패킷을 온전히 읽을 수 있는지
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize) {
                    break;
                }

                //패킷 읽기
                if (!OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize))) {
                    processLen = buffer.Count;
                    success = false;
                    break;
                }

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }

        public abstract bool OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session {
        Socket _socket;
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(65535);

        object _lock = new object();
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer, ref bool success);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket) {
            _socket = socket;

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            RegisterRecv();
        }

        public void Send(List<ArraySegment<byte>> sendBuffList) {
            if (sendBuffList.Count == 0) {
                return;
            }

            lock (_lock) {
                foreach (ArraySegment<byte> sendBuff in sendBuffList) {
                    _sendQueue.Enqueue(sendBuff);
                }

                if (_pendingList.Count == 0) {
                    RegisterSend();
                }
            }
        }

        public void Send(ArraySegment<byte> sendBuff) {
            lock (_lock) {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0) {
                    RegisterSend();
                }
            }
        }

        void RegisterSend() {
            if (_disconnected == 1) {
                return;
            }

            while (_sendQueue.Count > 0) {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false) {
                    OnSendCompleted(null, _sendArgs);
                }
            }
            catch (SocketException e) {
                Console.WriteLine($"SocketException: RegisterSend Failed {e.ToString()}");
                Disconnect();

            }
            catch (Exception e) {
                Console.WriteLine($"RegisterSend Failed {e.ToString()}");
                Disconnect();
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args) {
            lock (_lock) {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                    try {
                        args.BufferList = null;
                        _pendingList.Clear();

                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0) {
                            RegisterSend();
                        }
                    }
                    catch (SocketException e) {
                        Console.WriteLine($"SocketException: OnSendCompleted Failed {e.ToString()}");
                        Disconnect();
                    }
                    catch (Exception e) {
                        Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                        Disconnect();
                    }
                }
                else {
                    Disconnect();
                }
            }
        }

        void RegisterRecv() {
            if (_disconnected == 1) {
                return;
            }

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false) {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (SocketException e) {
                Console.WriteLine($"SocketException: RegisterRecv Failed {e.ToString()}");
                Disconnect();
            }
            catch (Exception e) {
                Console.WriteLine($"RegisterRecv Failed {e.ToString()}");
                Disconnect();
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args) {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success) {
                try {
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false) {
                        Disconnect();
                        return;
                    }

                    bool success = true;
                    int processLen = OnRecv(_recvBuffer.ReadSegment, ref success);
                    if (processLen == 0 || _recvBuffer.DataSize < processLen) {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLen) == false) {
                        Disconnect();
                        return;
                    }

                    if (success == false) {
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (SocketException e) {
                    Console.WriteLine($"SocketException: OnRecvCompleted Failed {e.ToString()}");
                    Disconnect();
                }
                catch (Exception e) {
                    Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
                    Disconnect();
                }
            }
            else {
                Disconnect();
            }
        }

        void Clear() {
            lock (_lock) {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Disconnect(bool attackerExist = false) {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) {
                return;
            }

            if (attackerExist) {
                IPEndPoint ipEndPoint = _socket.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint != null) {
                    string ipAddress = ipEndPoint.Address.ToString();
                    int port = ipEndPoint.Port;

                    Console.WriteLine($"유효하지 않은 데이터를 보낸 상대방의 IP 주소: {ipAddress}, 포트: {port}");
                }
            }

            if (_socket.RemoteEndPoint != null) {
                OnDisconnected(_socket.RemoteEndPoint);
            }
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            Clear();
        }
    }
}
