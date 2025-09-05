using Server.Game;

namespace Server.Object {
    /// <summary>
    /// 게임에 존재하는 플레이어, NPC, 장애물 등의 기본이 되는 틀
    /// </summary>
    public class BaseObject {
        public int ObjectID { get; set; }
        public GameRoom Room { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public bool interacting { get; set; }
    }
}
