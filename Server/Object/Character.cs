using Server.Items;
using Server.DB;
using Server.Job;
using Server.Game;

namespace Server.Object {
    /// <summary>
    /// GameRoom에 존재하는 플레이어
    /// </summary>
    public class Character : BaseObject {
        public int CharacterDBId { get; set; }
        public string Nickname { get; set; }
        public int Money { get; set; }
        public int Dept { get; set; }
        public int Location { get; set; }
        public int FarmCoin { get; set; }
        public int? FarmDBId { get; set; }
        public int AccountDBId { get; set; }
        public ClientSession Session { get; set; }
        public PlayerDirection Direction { get; set; }
        public MoveVector3 MoveVector3 { get; set; } = new MoveVector3() { x = 0, y = 0, z = 0 };
        public Inventory Inven { get; private set; } = new Inventory();
        public long startInteraction = 0;
        public int recentInteractObjectId = -1;
        public List<int> canInteraction = new List<int>();
        public List<int> catchAnimalList = new List<int>();
        public int[]? party = null;
        public bool acceptEnterGame = false;
        public bool sessionGameLoadingCompleted = false;
        public int skillId { get; set; } = -1;
        public Dictionary<int, int> skillLevel = new Dictionary<int, int>() {
            { 20000, 1 },
            { 20001, 1 },
            { 20002, 1 },
            { 20003, 1 }
        };
        public int seed = 0;
        public int destroyedStone = 0;
        public int createdField = 0;
        public int collectedSeed = 0;
        public int plantedSeed = 0;
        public int removedWeed = 0;
        public int catchedAnimal = 0;
        public int harvestedCrops = 0;
        public bool isWarm = true;
        public bool exhausted = false;
        public int maxHp { get; set; } = 100;
        public int hp { get; private set; } = 100;
        IJob _lastHpJob;

        public void ResetSessionGameCount() {
            skillId = -1;
            exhausted = false;
            isWarm = true;
            acceptEnterGame = false;
            sessionGameLoadingCompleted = false;
            recentInteractObjectId = -1;
            hp = 100;
            seed = 0;
            destroyedStone = 0;
            createdField = 0;
            collectedSeed = 0;
            plantedSeed = 0;
            removedWeed = 0;
            catchedAnimal = 0;
            harvestedCrops = 0;
            catchAnimalList.Clear();
        }

        public void ChangeWarmState(bool warm) {
            if (warm == isWarm) {
                return;
            }
            if (_lastHpJob != null) {
                _lastHpJob.Cancel = true;
            }
            isWarm = warm;
            if (warm) {
                _lastHpJob = Room.PushAfter(1000, ChangeHp, maxHp / 10);
            }
            else {
                _lastHpJob = Room.PushAfter(1000, ChangeHp, -10);
            }
        }

        public void ResetHp() {
            hp = maxHp;
        }

        void ChangeHp(int unit) {
            if (Room == null) {
                return;
            }
            if (Room.RoomType != RoomType.SessionGame) {
                return;
            }
            SessionRoom sessionRoom = Room as SessionRoom;
            if (sessionRoom.season != Season.Winter) {
                return;
            }

            int newHp = (int)Math.Clamp(hp + unit, 0, maxHp);
            if (hp == newHp) {
                //No Hp Change
                return;
            }

            S_ChangeHp hpPacket = new S_ChangeHp() {
                TargetCharacterDBId = CharacterDBId,
                Hp = newHp,
            };
            Room.Broadcast(hpPacket);
            if (exhausted && newHp > 0) {
                S_Exhaust exhaustPacket = new S_Exhaust() {
                    TargetCharacterDBId = CharacterDBId,
                    Exhaust = false,
                };
                Room.Broadcast(exhaustPacket);
                exhausted = false;
            }
            else if (newHp <= 0) {
                S_Exhaust exhaustPacket = new S_Exhaust() {
                    TargetCharacterDBId = CharacterDBId,
                    Exhaust = true,
                };
                Room.Broadcast(exhaustPacket);
                exhausted = true;
            }
            hp = newHp;

            _lastHpJob = Room.PushAfter(1000, ChangeHp, unit);
        }

        public void GetInventory() {
            S_GetInventory invenPacket = new S_GetInventory() { 
                CharacterDBId = CharacterDBId 
            };
            foreach (Item item in Inven.GetAll()) {
                ItemInfoData itemData = new ItemInfoData() {
                    ItemDBId = item.ItemDBId,
                    ItemType = item.ItemType,
                    TemplateId = item.TemplateId,
                    Count = item.Count,
                    Slot = item.Slot,
                    Equip = item.Equipped,
                };
                invenPacket.Inventory.Add(itemData);
            }

            Session.SendPacket(invenPacket);
        }

        public void HandleEquipItem(int itemDbId, bool equipped) {
            Item item = Inven.Get(itemDbId);
            if (item == null) {
                return;
            }

            if (equipped) {
                Item unequipItem = null;
                if (item.ItemType == ItemType.Hand) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Hand));
                }
                else if (item.ItemType == ItemType.Bag) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Bag));
                }
                else if (item.ItemType == ItemType.Bottom) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Bottom));
                }
                else if (item.ItemType == ItemType.Handwear) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Handwear));
                }
                else if (item.ItemType == ItemType.Earing) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Earing));
                }
                else if (item.ItemType == ItemType.Eyewear) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Eyewear));
                }
                else if (item.ItemType == ItemType.Headgear) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Headgear));
                }
                else if (item.ItemType == ItemType.Shoes) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Shoes));
                }
                else if (item.ItemType == ItemType.Top) {
                    unequipItem = Inven.Find(i => (i.Equipped && i.ItemType == ItemType.Top));
                }

                if (unequipItem != null) {
                    unequipItem.Equipped = false;

                    DbTransaction.EquipItem(this, unequipItem);

                    //기존에 착용했던 같은 카테고리 아이템은 착용해제하고 브로드캐스트
                    S_EquipItem unequipPacket = new S_EquipItem() {
                        CharacterDBId = CharacterDBId,
                        ItemDBId = unequipItem.ItemDBId,
                        ItemTemplateId = unequipItem.TemplateId,
                        Equip = unequipItem.Equipped,
                    };
                    Room.Broadcast(unequipPacket);
                }
            }

            item.Equipped = equipped;

            DbTransaction.EquipItem(this, item);
            //클라에 패킷 보내기
            S_EquipItem equipIPacket = new S_EquipItem() {
                CharacterDBId = CharacterDBId,
                ItemDBId = item.ItemDBId,
                ItemTemplateId = item.TemplateId,
                Equip = item.Equipped,
            };
            Room.Broadcast(equipIPacket);
        }
    }
}
