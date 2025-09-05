using Server.Game;
using Server.Job;
using System.Numerics;

namespace Server.Object {
    public class Animal : InteractiveObject {
        const int MaxSpawnFromObject = 20;
        const int MinSpawnFromObject = 10;
        const float tempOffset = 1.5f;
        float HalfOffsetX = 2.5f;
        float HalfOffsetZ = 2.5f;
        //어떤 종류의 동물인지
        public int _animalId = 0;
        public IJob job { get; private set; }
        InteractiveObject _targetObject { get; set; }
        Vector3 _moveVector = Vector3.Zero;
        float _speed = 10.0f;
        bool _moveStop = false;
        PlayerDirection _animalDirection = PlayerDirection.South;
        static Vector3[] _directionVector = { new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), 
            new Vector3((float)0.71, 0, (float)0.71), new Vector3((float)0.71, 0, (float)-0.71), new Vector3((float)-0.71, 0, (float)-0.71), new Vector3((float)-0.71, 0, (float)0.71) };
        bool _spawned = false;
        Random _random = new Random();

        public Animal(GameRoom room) : base(ObjectType.Animal, room, 0.5f) {

        }

        public bool isMoving() {
            return !_moveStop;
        }

        public void Update(float tick) {
            if (interacting) {
                return;
            }
            if (_targetObject == null) {
                //대상 탐색
                if (!FindAndSetTarget()) {
                    //타겟으로 지정할 대상이 없을 경우 해당 동물 삭제
                    Room.Push(Room.RemoveAnimal, this);
                    Room.Push(() => {
                        S_DespawnObject despawnPacket = new S_DespawnObject() {
                            ObjectType = ObjectType.Animal,
                            All = false
                        };
                        despawnPacket.ObjectIDs.Add(ObjectID);
                        Room.Broadcast(despawnPacket);
                    });
                    return;
                }
                _moveStop = false;
            }
            if (_moveStop) {
                return;
            }
            if (_targetObject.hp <= 0) {
                ResetTarget();
                return;
            }
            //대상과의 거리 계산. 거의 근접했으면 이동정지
            float directionX = (_targetObject.posX + HalfOffsetX) - posX;
            float directionY = _targetObject.posY - posY;
            float directionZ = (_targetObject.posZ + HalfOffsetZ) - posZ;
            if (MathF.Abs(directionX) < 0.4) {
                SetPosition(_targetObject.posX + HalfOffsetX, posY, posZ);
            }
            if (MathF.Abs(directionZ) < 0.4) {
                SetPosition(posX, posY, _targetObject.posZ + HalfOffsetZ);
            }
            //컴퓨터에서 실수를 저장하는 방법의 문제로 -10 - (-10)을 해도 0.3 등으로 저장되는 문제가 발생
            if (MathF.Abs(directionX) < 0.4 && MathF.Abs(directionZ) < 0.4) {
                SetPosition(_targetObject.posX + HalfOffsetX, _targetObject.posY, _targetObject.posZ + HalfOffsetZ);
                MoveEnd();
                return;
            }
            //방향벡터 계산
            Vector3 actualDirection = GetNormalizeDirectionVector(directionX, directionY, directionZ);

            int nearestIndex = GetNearest8DirectionIndex(actualDirection);

            if (MathF.Abs(Vector3.DistanceSquared(actualDirection, _moveVector) - Vector3.DistanceSquared(actualDirection, _directionVector[nearestIndex])) > 0.4) {
                _moveVector = _directionVector[nearestIndex];
                _animalDirection = (PlayerDirection)nearestIndex;
                MoveStart(_moveVector, _animalDirection);
            }
            
            //Move
            posX += _moveVector.X * _speed * tick;
            posY += _moveVector.Y * _speed * tick;
            posZ += _moveVector.Z * _speed * tick;

            Room.Collision.RemoveObject(ObjectID);
            Room.Collision.AddObject(posX, posZ, ObjectID);
        }

        public void MoveStart(Vector3 moveDirection, PlayerDirection animalDirection) {
            //Animal의 현재 위치, 방향, 속도 클라로 보내기

            S_AnimalMove movePacket = new S_AnimalMove() {
                AnimalId = ObjectID,
                Direction = animalDirection,
                PosX = posX,
                PosY = posY,
                PosZ = posZ
            };
            Room.Broadcast(movePacket, 1);
        }

        public void MoveEnd() {
            _moveStop = true;
            _moveVector = Vector3.Zero;

            S_AnimalMoveEnd moveEndPacket = new S_AnimalMoveEnd() {
                AnimalId = ObjectID,
                PosX = posX,
                PosY = posY,
                PosZ = posZ
            };
            Room.Broadcast(moveEndPacket, 1);
            TryEatTarget(_targetObject.ObjectID);
        }

        void TryEatTarget(int firstTargetId) {
            if (_moveStop && _targetObject != null && firstTargetId == _targetObject.ObjectID) {
                if (!_targetObject.interacting) {
                    if (_targetObject.ObjectType != ObjectType.Puppet) {
                        _targetObject.disturbedByAnimal = true;
                    }
                    job = Room.PushAfter(1000 * 2, _targetObject.ChangeHp, -10, 2, this);
                }
                else {
                    Room.PushAfter(1000, TryEatTarget, firstTargetId);

                }
            }
        }

        void SetPosition(float x, float y, float z) {
            posX = x;
            posY = y;
            posZ = z;
        }

        public void ResetTarget() {
            if (_targetObject == null) {
                return;
            }
            if (job != null) {
                job.Cancel = true;
            }
            _targetObject.RemoveAnimal(ObjectID);
            _targetObject.disturbedByAnimal = false;
            _targetObject = null;
        }

        public void SetTarget(InteractiveObject targetObject) {
            _targetObject = targetObject;
            targetObject.AddAnimal(ObjectID);
            if (!_spawned) {
                _spawned = true;
                int randomDir = _random.Next(_directionVector.Length);
                int randomDistance = _random.Next(MinSpawnFromObject, MaxSpawnFromObject + 1);
                posX = targetObject.posX + HalfOffsetX - (_directionVector[randomDir].X * randomDistance);
                posZ = targetObject.posZ + HalfOffsetZ - (_directionVector[randomDir].Z * randomDistance);
                S_SpawnObject spawnPacket = new S_SpawnObject();
                spawnPacket.Objects.Add(new ObjectData() {
                    ObjectType = ObjectType.Animal,
                    ObjectId = ObjectID,
                    PosX = posX,
                    PosY = posY,
                    PosZ = posZ
                });
                Room.Broadcast(spawnPacket);
                if (targetObject.ObjectType == ObjectType.Puppet) {
                    GetNearestScarecrowOffset(targetObject.posX, targetObject.posZ);
                }
            }
        }

        public void SetJob(IJob hpJob) {
            job = hpJob;
        }

        bool FindAndSetTarget() {
            if (Room.RoomType == RoomType.SessionGame) {
                SessionRoom sessionRoom = Room as SessionRoom;
                List<InteractiveObject> scarecrow = sessionRoom.GetObjectsByObjectType(ObjectType.Puppet);
                if (scarecrow.Count != 0 && scarecrow[0].hp > 0) {
                    if (_spawned) {
                        GetNearestScarecrowOffset(scarecrow[0].posX, scarecrow[0].posZ);
                    }
                    SetTarget(scarecrow[0]);
                    return true;
                }
                List<InteractiveObject> crops = sessionRoom.GetObjectsByTypeInField(ObjectType.Crop);
                while (true) {
                    if (crops.Count == 0) {
                        break;
                    }
                    int randomIndex = _random.Next(crops.Count);
                    if (crops[randomIndex].GetAnimals().Count == 0 && crops[randomIndex].hp > 0) {
                        HalfOffsetX = 2.5f;
                        HalfOffsetZ = 2.5f;
                        SetTarget(crops[randomIndex]);
                        return true;
                    }
                    crops.RemoveAt(randomIndex);
                }
                List<InteractiveObject> seeds = sessionRoom.GetObjectsByTypeInField(ObjectType.SeedForPlant);
                while (true) {
                    if (seeds.Count  == 0) {
                        break;
                    }
                    int randomIndex = _random.Next(seeds.Count);
                    if (seeds[randomIndex].GetAnimals().Count == 0 && seeds[randomIndex].hp > 0) {
                        HalfOffsetX = 2.5f;
                        HalfOffsetZ = 2.5f;
                        SetTarget(seeds[randomIndex]);
                        return true;
                    }
                    seeds.RemoveAt(randomIndex);
                }
            }
            return false;
        }

        void GetNearestScarecrowOffset(float scarecrowPosX, float scarecrowPosZ) {
            Vector3 scarecrowPos = new Vector3(0, 0, 0);
            Vector3 animalPos = new Vector3(posX, posY, posZ);
            float minDist = 10000000.0f;
            for (int i = 0; i < _directionVector.Length; i++) {
                scarecrowPos.X = scarecrowPosX + _directionVector[i].X * tempOffset;
                scarecrowPos.Z = scarecrowPosZ + _directionVector[i].Z * tempOffset;
                float dist = Vector3.DistanceSquared(animalPos, scarecrowPos);
                if (minDist > dist) {
                    minDist = dist;
                    if (_directionVector[i].X > 0) {
                        HalfOffsetX = tempOffset;
                    }
                    else {
                        HalfOffsetX = -tempOffset;
                    }
                    if (_directionVector[i].Z > 0) {
                        HalfOffsetZ = tempOffset;
                    }
                    else {
                        HalfOffsetZ = -tempOffset;
                    }
                }
            }
        }

        Vector3 GetNormalizeDirectionVector(float directionX, float directionY, float directionZ) {
            Vector3 directionVector = new Vector3(directionX, directionY, directionZ);
            directionVector = Vector3.Normalize(directionVector);
            return directionVector;
        }

        int GetNearest8DirectionIndex(Vector3 direction) {
            int nearestIndex = -1;
            float nearestDIstance = 1000000.0f;
            for (int i = 0; i < _directionVector.Length; i++) {
                float getDistance = Vector3.DistanceSquared(_directionVector[i], direction);
                if (nearestDIstance > getDistance) {
                    nearestDIstance = getDistance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }
    }
}
