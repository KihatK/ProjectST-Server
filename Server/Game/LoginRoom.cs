namespace Server.Game {
    public class LoginRoom : GameRoom {
        List<int> _loginCharacterDBIds;
        public LoginRoom() : base(RoomType.Login) {
            
        }

        public override void Init() {
            base.Init();
            _loginCharacterDBIds = new List<int>();
        }

        public void LoginAccount(int characterDBId) {
            _loginCharacterDBIds.Add(characterDBId);
        }

        public void LogoutAccount(int characterDBId) {
            _loginCharacterDBIds.Remove(characterDBId);
        }

        public bool IsLoggedIn(int characterDBId) {
            return _loginCharacterDBIds.Contains(characterDBId);
        }
    }
}
