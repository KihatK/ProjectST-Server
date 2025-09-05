using Server.Game;
using Server.Job;

namespace Server.Object {
    public class InteractiveObject : BaseObject {
        const int GainHp = 5;
        const int GainHpInterval = 1;
        public ObjectType ObjectType { get; set; }
        public InteractType InteractType { get; set; }
        public float collider { get; set; } = 0.5f;
        public bool canPassThough { get; set; }
        public bool isWarm = false;
        public bool disturbedByAnimal = false;

        public int maxHp { get; set; } = 100;
        public int hp { get; private set; } = 100;
        public IJob _lastHpJob;

        List<int> _animals = new List<int>();

        public InteractiveObject(ObjectType objectType, GameRoom room, float coll) {
            ObjectType = objectType;
            Room = room;
            switch (ObjectType) {
                case ObjectType.Stone:
                    canPassThough = false;
                    break;
                default:
                    canPassThough = true;
                    break;
            }
            collider = coll;
        }

        public void AddAnimal(int animalId) {
            _animals.Add(animalId);
        }

        public void RemoveAnimal(int animalId) {
            if (_animals.Contains(animalId)) {
                _animals.Remove(animalId);
                if (_animals.Count == 0) {
                    if (_lastHpJob != null) {
                        _lastHpJob.Cancel = true;
                    }
                    if (ObjectType != ObjectType.Puppet) {
                        _lastHpJob = Room.PushAfter(1000 * GainHpInterval, ChangeHp, GainHp, GainHpInterval);
                    }
                }
            }
        }

        public List<Animal> GetAnimals() {
            List<Animal> animals = new List<Animal>();
            foreach (int id in _animals) {
                Animal animal = Room.GetAnimal(id);
                animals.Add(animal);
            }

            return animals;
        }

        public List<Animal> GetNearAnimals() {
            if (ObjectType != ObjectType.Puppet) {
                return null;
            }
            List<Animal> animals = new List<Animal>();
            foreach (int id in _animals) {
                Animal animal = Room.GetAnimal(id);
                if (!animal.isMoving()) {
                    animals.Add(animal);
                }
            }

            return animals;
        }

        public void ClearAnimals() {
            _animals.Clear();
        }

        public void ResetHp() {
            if (_lastHpJob != null) {
                _lastHpJob.Cancel = true;
            }
            hp = maxHp;
            if (ObjectType == ObjectType.Field || ObjectType == ObjectType.Puppet) {
                return;
            }
            S_ChangeObjectHp hpPacket = new S_ChangeObjectHp() {
                ObjectType = ObjectType,
                ObjectId = ObjectID,
                Hp = hp,
            };
            Room.Broadcast(hpPacket);
        }

        public void ChangeHp(int unit, int interval) {
            if (ObjectType != ObjectType.Weed) {
                if (unit < 0 || (ObjectType != ObjectType.SeedForPlant && ObjectType != ObjectType.Crop && ObjectType != ObjectType.Puppet)) {
                    hp = maxHp;
                    return;
                }
            }

            int newHp = (int)Math.Clamp(hp + unit, 0, maxHp);
            if (hp == newHp) {
                //No Hp Change
                return;
            }

            if (_lastHpJob != null) {
                _lastHpJob.Cancel = true;
            }

            S_ChangeObjectHp hpPacket = new S_ChangeObjectHp() {
                ObjectType = ObjectType,
                ObjectId = ObjectID,
                Hp = newHp,
            };
            Room.Broadcast(hpPacket);
            hp = newHp;
            if (newHp <= 0) {
                SessionRoom sessionRoom = Room as SessionRoom;
                if (ObjectType == ObjectType.Weed) {
                    sessionRoom.Push(sessionRoom.CheckCanContinueGame, ObjectType);
                }
                sessionRoom.ChangeFieldObjectType(this, ObjectType.None);
                S_ChangeObject changePacket = new S_ChangeObject() {
                    From = ObjectType,
                    To = ObjectType.None,
                };
                changePacket.ObjectIds.Add(ObjectID);
                Room.Broadcast(changePacket);
                return;
            }

            _lastHpJob = Room.PushAfter(1000 * interval, ChangeHp, unit, interval);
        }

        public void ChangeHp(int unit, int interval, Animal animal) {
            if (ObjectType != ObjectType.SeedForPlant && ObjectType != ObjectType.Crop && ObjectType != ObjectType.Puppet) {
                return;
            }
            int newHp = (int)Math.Clamp(hp + unit, 0, maxHp);
            
            if (hp == newHp) {
                //No Hp Change
                if (hp <= 0) {
                    animal.ResetTarget();
                }
                return;
            }

            if (_lastHpJob != null) {
                _lastHpJob.Cancel = true;
            }

            S_ChangeObjectHp hpPacket = new S_ChangeObjectHp() {
                ObjectType = ObjectType,
                ObjectId = ObjectID,
                Hp = newHp,
            };
            Room.Broadcast(hpPacket);
            hp = newHp;
            if (newHp <= 0) {
                if (ObjectType == ObjectType.Puppet) {
                    Room.RemoveObject(ObjectID, this);
                    S_RemoveScarecrow removeScarecrow = new S_RemoveScarecrow() {
                        ObjectId = ObjectID
                    };
                    Room.Broadcast(removeScarecrow);
                }
                else {
                    SessionRoom sessionRoom = Room as SessionRoom;
                    if (ObjectType == ObjectType.Crop) {
                        sessionRoom.Push(sessionRoom.CheckCanContinueGame, ObjectType);
                    }

                    S_ChangeObject changePacket = new S_ChangeObject() {
                        From = ObjectType,
                        To = ObjectType.None,
                    };
                    if (ObjectType == ObjectType.SeedForPlant) {
                        sessionRoom.ChangeFieldObjectType(this, ObjectType.Field);
                        changePacket.To = ObjectType.Field;
                    }
                    else {
                        sessionRoom.ChangeFieldObjectType(this, ObjectType.None);
                    }

                    changePacket.ObjectIds.Add(ObjectID);
                    Room.Broadcast(changePacket);
                }
                return;
            }

            animal.SetJob(Room.PushAfter(1000 * interval, ChangeHp, unit, interval, animal));
        }
    }
}
