using Server.Game;

namespace Server.Object {
    public class NPC : BaseObject {
        float _time = 0.0f;
        Random _random = new Random();

        public NPC(GameRoom room) : base() {
            Room = room;
        }

        public void Update(float tick) {
            _time += tick;
            if (_time >= 5.0f) {
                GetRandomLocation();

                _time = 0.0f;
            }
        }

        void GetRandomLocation() {
            double min = Math.Max(-9, posX - 5);
            double max = Math.Min(14, posX + 5);
            double randomValue = _random.NextDouble();
            float randomX = (float)(randomValue * (max - min) + min);

            min = Math.Max(-14, posZ - 5);
            max = Math.Min(9, posZ + 5);
            randomValue = _random.NextDouble();
            float randomZ = (float)(randomValue * (max - min) + min);
            
            S_NPCMove npcMovePacket = new S_NPCMove() {
                ObjectId = ObjectID,
                PosX = randomX,
                PosY = 0,
                PosZ = randomZ,
            };
            Room.Broadcast(npcMovePacket);
            posX = randomX;
            posZ = randomZ;
        }
    }
}
