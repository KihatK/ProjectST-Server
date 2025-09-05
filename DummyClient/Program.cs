using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient {
    class Program {
        static void Main(string[] args) {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            //IPAddress ipAddr = ipHost.AddressList[1];
            IPAddress ipAddr = Dns.GetHostAddresses("a1047e594daccc0ec.awsglobalaccelerator.com")[0];
            //IPAddress ipAddr = IPAddress.Parse("13.124.31.243");
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); });

            while (true) {
                
            }
        }
    }
}