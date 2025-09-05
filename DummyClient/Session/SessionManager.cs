using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient {
    class SessionManager {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        object _lock = new object();
        int _dummyID = 0;
        Dictionary<int, ServerSession> _sessions = new Dictionary<int, ServerSession>();

        public ServerSession Generate() {
            lock (_lock) {
                ServerSession session = new ServerSession();
                session.DummmyID = ++_dummyID;

                _sessions.Add(_dummyID, session);
                //Console.WriteLine($"Generate: {_dummyID}");
                return session;
            }
        }

        public void Remove(ServerSession session) {
            lock (_lock) {
                _sessions.Remove(session.DummmyID);
            }
        }
    }
}
