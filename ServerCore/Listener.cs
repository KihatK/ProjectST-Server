using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace ServerCore {
    public class Listener {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 128) {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _sessionFactory += sessionFactory;

            _listenSocket.Bind(endPoint);

            _listenSocket.Listen(backlog);

            for (int i=0; i<register; i++) {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args) {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false) {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args) {
            try {
                if (args.SocketError == SocketError.Success) {
                    Session session = _sessionFactory.Invoke();
                    args.AcceptSocket.NoDelay = true;
                    session.Start(args.AcceptSocket);
                    if (args.AcceptSocket == null || args.AcceptSocket.Connected == false) {
                        args.AcceptSocket?.Close();
                    }
                    else {
                        session.OnConnected(args.AcceptSocket.RemoteEndPoint);
                    }
                }
                else {
                    Console.WriteLine(args.SocketError.ToString());
                }
            }
            catch (SocketException e) {
                Console.WriteLine($"SocketException: OnAcceptCompleted Failed {e.ToString()}");
                args.AcceptSocket?.Close();
            }
            catch (Exception e) {
                Console.WriteLine($"OnAcceptCompleted Failed {e.ToString()}");
                args.AcceptSocket?.Close();
            }
            RegisterAccept(args);
        }
    }
}
