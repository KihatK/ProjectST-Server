using Server.Game;
using ServerCore;
using Server.DB;
using System.Net;
using Server.Engine;
using Server.Data;
using Server.Redis;

namespace Server {
    static class Program {
        static Listener _listener = new Listener();

        //Gather packets. Send packets when the time come
        static void NetworkTask() {
            List<ClientSession> sessions = SessionManager.Instance.GetSessions();
            foreach (ClientSession session in sessions) {
                session.FlushSend();
            }
        }
        static void GameLogicTask() {
            //클라이언트에서 받은 패킷 컨텐츠 로직 처리
            GameLogic.Instance.Update();
        }

        static void DbTask() {
            while (true) {
                DbTransaction.Instance.Flush();
                Thread.Sleep(0);
            }
        }

        static void RedisTask() {
            while (true) {
                RedisServer.Instance.Flush();
                Thread.Sleep(0);
            }
        }

        static void Main(string[] args) {
            long lastTick = DateTime.UtcNow.Ticks;

            ConfigManager.LoadConfig();
            DataManager.LoadData();

            RedisServer.Instance.RunRedisServer();
            GameLogic.Instance.Init();

            int MsPerFrame = ConfigManager.Config.MsPerFrame;

            IPAddress ipAddr;
            if (ConfigManager.Config.Deploy) {
                ipAddr = IPAddress.Parse("0.0.0.0");
            }
            else {
                string host = Dns.GetHostName();
                IPHostEntry ipHost = Dns.GetHostEntry(host);
                ipAddr = ipHost.AddressList[1];
            }

            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Thread.CurrentThread.Name = "Main";

            {
                Thread t = new Thread(DbTask);
                t.Name = "DB";
                t.Start();
            }

            {
                Thread t = new Thread(RedisTask);
                t.Name = "Redis";
                t.Start();
            }

            DbTransaction.Dummy();
            DbTransaction.DbPing();

            //메모리 해제 확인용
            //{
            //    Thread t = new Thread(() => {
            //        while (true) {
            //            GC.Collect();
            //            Thread.Sleep(3000);
            //        }
            //    });
            //    t.Name = "GC";
            //    t.Start();
            //}

            while (true) {
                long now = DateTime.UtcNow.Ticks;
                long diff = now - lastTick;
                TimeSpan tick = new TimeSpan(diff);
                if (tick.TotalMilliseconds >= MsPerFrame) {
                    //추후 멀티스레드 처리
                    Moving.CalculatePlayersMoving(tick);
                    GameLogicTask();
                    NetworkTask();
                    lastTick = now;
                }
                Thread.Sleep(0);
            }
        }
    }
}