using Server.Game;
using Server.Object;

namespace Server.Engine {
    public class Moving {
        static float playerSpeed = 8.0f;
        static float playerExhaustedSpeed = 4.0f;
        public static void CalculatePlayersMoving(TimeSpan tick) {
            List<GameRoom> rooms = GameLogic.Instance.GetRooms();
            foreach (GameRoom room in rooms) {
                List<Character> characters = room.GetCharacters();
                List<Animal> animals = room.GetAnimals();
                List<NPC> npcs = room.GetNPCs();
                foreach (Character character in characters) {
                    float posX;
                    float posY;
                    float posZ;
                    if (character.exhausted) {
                        posX = character.posX + character.MoveVector3.x * playerExhaustedSpeed * (float)tick.TotalSeconds;
                        posY = character.posY + character.MoveVector3.y * playerExhaustedSpeed * (float)tick.TotalSeconds;
                        posZ = character.posZ + character.MoveVector3.z * playerExhaustedSpeed * (float)tick.TotalSeconds;
                    }
                    else {
                        posX = character.posX + character.MoveVector3.x * playerSpeed * (float)tick.TotalSeconds;
                        posY = character.posY + character.MoveVector3.y * playerSpeed * (float)tick.TotalSeconds;
                        posZ = character.posZ + character.MoveVector3.z * playerSpeed * (float)tick.TotalSeconds;
                    }
                    if (!room.Collision.CheckCollision(posX, posZ, character, room) && CheckRangeIsValid(room, posX, posY, posZ)) {
                        if (!character.interacting) {
                            character.posX = posX;
                            character.posY = posY;
                            character.posZ = posZ;
                        }
                    }
                }
                foreach (Animal animal in animals) {
                    animal.Update((float)tick.TotalSeconds);
                }
                foreach (NPC npc in npcs) {
                    npc.Update((float)tick.TotalSeconds);
                }
            }
        }

        static bool CheckRangeIsValid(GameRoom room, float posX, float posY, float posZ) {
            if (room.RoomType == RoomType.MainIsland) {
                if (posX < -9 || posX > 14) {
                    return false;
                }
                if (posZ < -14 || posZ > 9) {
                    return false;
                }
            }
            else if (room.RoomType == RoomType.SessionGame) {
                if (posX < 0 || posX > (SessionRoom.FieldSize - 1) * SessionRoom.Offset) {
                    return false;
                }
                if (posZ < 0 || posZ > (SessionRoom.FieldSize - 1) * SessionRoom.Offset) {
                    return false;
                }
            }
            return true;
        }
    }
}
