namespace Server {
    /// <summary>
    /// ClientSession들을 저장하고 관리하는 객체
    /// </summary>
    class SessionManager {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionID = 0;
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        /// <summary>
        /// ClientSession을 생성하고 그에 맞는 SessionID 부여
        /// </summary>
        /// <returns>생성된 ClientSession 반환</returns>
        public ClientSession Generate() {
            lock (_lock) {
                int sessionID = ++_sessionID;

                ClientSession session = new ClientSession();
                session.SessionID = sessionID;
                _sessions.Add(sessionID, session);

                Console.WriteLine($"Generate: {sessionID}");

                return session;
            }
        }

        /// <summary>
        /// SessionManager가 가지고 있는 모든 ClientSession 반환
        /// </summary>
        /// <returns>모든 ClientSesison 반환</returns>
        public List<ClientSession> GetSessions() {
            List<ClientSession> sessions = new List<ClientSession>();

            lock (_lock) {
                sessions = _sessions.Values.ToList();
            }

            return sessions;
        }

        /// <summary>
        /// SessionID를 통해 ClientSession 찾고 반환
        /// </summary>
        /// <param name="id">찾고자 하는 ClientSession의 SessionID</param>
        /// <returns>찾고자 하는 ClientSession</returns>
        public ClientSession Find(int id) {
            lock (_lock) {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        /// <summary>
        /// ClientSession 제거
        /// </summary>
        /// <param name="session">제거하고자 하는 ClientSession</param>
        public void Remove(ClientSession session) {
            lock ( _lock) {
                _sessions.Remove(session.SessionID);
            }
        }
    }
}
