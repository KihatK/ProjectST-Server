using KdTree;
using KdTree.Math;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Object;

namespace Server.Engine {
    /// <summary>
    /// Collision 체크를 위해 KD Tree 구조를 이용하여 가장 가까운 오브젝트를 찾아 충돌하는지 확인함
    /// </summary>
    public class Collision {
        KdTree<float, int> kdTree;
        //추후 상수 처리
        const float _characterCollisionRadius = 3.0f;
        const float _characterInteractionRadius = 4.0f;
        const float _maxObjectColliderRadius = 1.0f;
        const float _senseRange = 5.0f;
        int[,] _directions = {
            {-1,  0},  // 상
            { 1,  0},  // 하
            { 0, -1},  // 좌
            { 0,  1},  // 우
            {-1, -1},  // 좌상
            {-1,  1},  // 우상
            { 1, -1},  // 좌하
            { 1,  1}   // 우하
        };

        /// <summary>
        /// Collision 생성을 할 때 KD Tree를 자동으로 생성
        /// </summary>
        public Collision() {
            kdTree = new KdTree<float, int>(2, new FloatMath());
        }

        /// <summary>
        /// KD Tree에 충돌처리할 오브젝트 추가
        /// </summary>
        /// <param name="x">추가할 오브젝트의 현재 x좌표</param>
        /// <param name="z">추가할 오브젝트의 현재 z좌표</param>
        /// <param name="objectId">추가할 오브젝트의 playerId</param>
        public void AddObject(float x, float z, int objectId) {
            kdTree.Add(new float[] { x, z }, objectId);
        }

        /// <summary>
        /// 더이상 충돌처리할 필요없는 오브젝트 제거
        /// </summary>
        /// <param name="objectId">제거할 오브젝트의 playerId</param>
        public void RemoveObject(int objectId) {
            float[] pos = null;
            if (kdTree.TryFindValue(objectId, out pos)) {
                kdTree.RemoveAt(pos);
            }
        }

        bool isInRange(int z, int x) {
            if (z < SessionRoom.FieldSize && z >= 0 && x < SessionRoom.FieldSize && x >= 0) {
                return true;
            }
            return false;
        }

        public bool CheckCollision(float x, float z, Character character, GameRoom room) {
            KdTreeNode<float, int>[] nodes = kdTree.RadialSearch(new float[] { x, z }, _characterInteractionRadius + _maxObjectColliderRadius);
            character.canInteraction.Clear();

            bool hasCollision = false;

            if (room.RoomType == RoomType.SessionGame) {
                SessionRoom sessionRoom = room as SessionRoom;
                int curZ = (int)(z) / SessionRoom.Offset;
                int curX = (int)(x) / SessionRoom.Offset;
                if (sessionRoom.season == Season.Winter) {
                    bool isWarm = isInRange(curZ, curX) ? sessionRoom.Field[curZ, curX].isWarm : false;
                    character.ChangeWarmState(isWarm);
                }
                if (isInRange(curZ, curX)) {
                    character.canInteraction.Add(sessionRoom.Field[curZ, curX].ObjectID);
                    if (!sessionRoom.Field[curZ, curX].canPassThough) {
                        hasCollision = true;
                    }
                    for (int i = 0; i < 8; i++) {
                        int dz = curZ + _directions[i, 0];
                        int dx = curX + _directions[i, 1];
                        if (isInRange(dz, dx)) {
                            character.canInteraction.Add(sessionRoom.Field[dz, dx].ObjectID);
                        }
                    }
                }
            }

            if (nodes.Length == 0) {
                return hasCollision;
            }

            foreach (KdTreeNode<float, int> node in nodes) {
                bool canPassThrough = room.GetObjectCanPassThrough(node.Value);
                float distance = (node.Point[0] - x) * (node.Point[0] - x) + (node.Point[1] - z) * (node.Point[1] - z);
                float objRadius = room.GetObjectCollider(node.Value);

                if (!canPassThrough && distance < (_characterCollisionRadius + objRadius) * (_characterCollisionRadius + objRadius)) {
                    hasCollision = true;
                }
                if (distance > (_characterInteractionRadius + objRadius) * (_characterInteractionRadius + objRadius)) {
                    break;
                }
                character.canInteraction.Add(node.Value);
            }

            return hasCollision;
        }
    }
}
