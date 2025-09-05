using Microsoft.Extensions.Logging.Abstractions;
using Server.Job;
using Server.Redis;

namespace Server.Game {
    public class GameLogic : JobSerializer {
        public static GameLogic Instance { get; } = new GameLogic();
        LoginRoom _loginRoom;
        MainRoom _mainRoom;
        Dictionary<int, ContentRoom> _contentRooms;
        Dictionary<int, SessionRoom> _sessionRooms;

        int _contentRoomId = 0;
        int _sessionRoomId = 0;

        public void Init() {
            _loginRoom = new LoginRoom();
            _mainRoom = new MainRoom();
            _contentRooms = new Dictionary<int, ContentRoom>();
            _sessionRooms = new Dictionary<int, SessionRoom>();

            _loginRoom.Init();
            _mainRoom.Init();
        }

        public void Update() {
            Flush();

            _loginRoom.Update();
            _mainRoom.Update();
            foreach (ContentRoom contentRoom in _contentRooms.Values) {
                contentRoom.Update();
            }
            foreach (SessionRoom sessionRoom in _sessionRooms.Values) { 
                sessionRoom.Update();
            }
        }

        public List<GameRoom> GetRooms() {
            List<GameRoom> rooms = new List<GameRoom>();
            rooms.Add(_loginRoom);
            rooms.Add(_mainRoom);
            foreach (ContentRoom contentRoom in _contentRooms.Values) {
                rooms.Add(contentRoom);
            }
            foreach (SessionRoom sessionRoom in _sessionRooms.Values) {
                rooms.Add(sessionRoom);
            }
            return rooms;
        }

        public GameRoom GetRoom(RoomType roomType, int roomId = -1) {
            switch (roomType) {
                case RoomType.Login:
                    return _loginRoom;
                case RoomType.MainIsland:
                    return _mainRoom;
                case RoomType.ContentIsland:
                    if (_contentRooms.ContainsKey(roomId)) {
                        return _contentRooms[roomId];
                    }
                    else {
                        throw new ArgumentException("Invalid room ID.");
                    }
                case RoomType.SessionGame:
                    if (_sessionRooms.ContainsKey(roomId)) {
                        return _sessionRooms[roomId];
                    }
                    else {
                        throw new ArgumentException("Invalid room ID.");
                    }
                default:
                    throw new ArgumentException("Invalid room type.");
            }
        }

        public GameRoom Add(RoomType roomType) {
            if (roomType == RoomType.ContentIsland) {
                ContentRoom contentRoom = new ContentRoom();
                contentRoom.Push(contentRoom.Init);
                contentRoom.RoomId = _contentRoomId;
                _contentRooms.Add(_contentRoomId, contentRoom);
                _contentRoomId++;
                return contentRoom;
            }
            else if (roomType == RoomType.SessionGame) {
                SessionRoom sessionRoom = new SessionRoom();
                sessionRoom.Push(sessionRoom.Init);
                sessionRoom.RoomId = _sessionRoomId;
                _sessionRooms.Add(_sessionRoomId, sessionRoom);
                _sessionRoomId++;
                return sessionRoom;
            }
            return null;
        }

        public bool Remove(RoomType roomType, int roomId) {
            GameRoom room = GetRoom(roomType, roomId);
            if (roomType == RoomType.ContentIsland) {
                ContentRoom contentRoom = room as ContentRoom;
                contentRoom.Clear();
                return _contentRooms.Remove(roomId);
            }
            else if ( roomType == RoomType.SessionGame) {
                SessionRoom sessionRoom = room as SessionRoom;
                sessionRoom.Clear();
                return _sessionRooms.Remove(roomId);
            }
            return false;
        }
    }
}
