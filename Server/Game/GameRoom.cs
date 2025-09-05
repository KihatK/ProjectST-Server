using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;
using Server.Data;
using Server.Engine;
using Server.Items;
using Server.Job;
using Server.Object;

namespace Server.Game {
    public class GameRoom : JobSerializer {
        public int RoomId { get; set; }
        public RoomType RoomType { get; set; }
        public Collision Collision { get; set; }
        Dictionary<int, Character> _characters = new Dictionary<int, Character>();
        Dictionary<int, InteractiveObject> _interactives = new Dictionary<int, InteractiveObject>();
        Dictionary<int, Animal> _animals = new Dictionary<int, Animal>();
        Dictionary<int, NPC> _NPCs = new Dictionary<int, NPC>();
        int _interactiveID = 10000;
        int _farmObjectId = 1000;
        int _animalId = 100;
        int _npcId = 10;
        int _partyId = 1;
        Dictionary<int, int[]> _parties;

        public GameRoom(RoomType roomType) {
            RoomType = roomType;
            if (roomType != RoomType.Login) {
                Collision = new Collision();
            }
        }

        public virtual void Init() {
            //TODO: 초기 세팅
            _parties = new Dictionary<int, int[]>();
        }

        public void Update() {
            Flush();
        }

        #region InGame
        public void HandleEquipItem(Character character, int itemDbId, bool equipped) {
            character.HandleEquipItem(itemDbId, equipped);
        }

        public void EnterGame(Character character) {
            if (character == null) {
                return;
            }

            character.posX = 0;
            character.posY = 0;
            character.posZ = 0;
            AddCharacter(character);
            {
                //해당 플레이어 클라이언트에게 전송
                S_EnterGame enterPacket = new S_EnterGame() {

                    CharacterDBId = character.CharacterDBId,
                    PosX = character.posX,
                    PosY = character.posY,
                    PosZ = character.posZ,
                    Direction = PlayerDirection.North,
                    SkillLevel = character.skillLevel
                };

                character.Session.SendPacket(enterPacket);

                //기존에 존재하던 플레이어들 해당 플레이어에게 전송
                foreach (Character c in _characters.Values) {
                    if (c.CharacterDBId != character.CharacterDBId) {
                        S_Spawn spawnPacket = new S_Spawn() {
                            CharacterDBId = c.CharacterDBId,
                            Nickname = c.Nickname,
                            PosX = c.posX,
                            PosY = c.posY,
                            PosZ = c.posZ,
                        };
                        foreach (Item item in c.Inven.GetEquipItem()) {
                            spawnPacket.EquipItemInfoData.Add(new ItemInfoData() {
                                ItemDBId = item.ItemDBId,
                                ItemType = item.ItemType,
                                TemplateId = item.TemplateId,
                                Count = item.Count,
                                Slot = item.Slot,
                                Equip = item.Equipped
                            });
                        }
                        character.Session.SendPacket(spawnPacket);
                    }
                }
            }
            {
                //맵에 존재하는 다른 플레이어들에게 전송
                S_Spawn spawnPacket = new S_Spawn() {
                    CharacterDBId = character.CharacterDBId,
                    Nickname = character.Nickname,
                    PosX = character.posX,
                    PosY = character.posY,
                    PosZ = character.posZ,
                };
                foreach (Item item in character.Inven.GetEquipItem()) {
                    spawnPacket.EquipItemInfoData.Add(new ItemInfoData() {
                        ItemDBId = item.ItemDBId,
                        ItemType = item.ItemType,
                        TemplateId = item.TemplateId,
                        Count = item.Count,
                        Slot = item.Slot,
                        Equip = item.Equipped
                    });
                }
                Broadcast(character, spawnPacket);
            }
        }

        public void LeaveGame(Character character, ClientSession clientSession) {
            _characters.Remove(character.CharacterDBId);

            S_Despawn despawnPacket = new S_Despawn() {
                CharacterDBId = character.CharacterDBId
            };
            Broadcast(character, despawnPacket);
            S_Logout logoutPacket = new S_Logout() {
                CharacterDBId = character.CharacterDBId,
                Ok = true,
                ErrorCode = 0,
            };
            character.Session.SendPacket(logoutPacket);

            character.Room = null;
            clientSession.MyCharacter = null;
        }
        #endregion

        #region Party
        public void MakeParty(Character character) {
            if (character == null) {
                Console.WriteLine("MakeParty(): character is null");
                return;
            }
            S_MakeParty makePartyPacket = new S_MakeParty();
            if (character.party != null) {
                makePartyPacket.Ok = false;
                makePartyPacket.ErrorMessage = "이미 속해있는 파티가 있습니다";
                makePartyPacket.ErrorCode = 1;
                character.Session.SendPacket(makePartyPacket);
                return;
            }

            int[] party = { -1, -1, -1, -1, -1 };
            party[0] = _partyId;
            party[1] = character.CharacterDBId;

            _parties[party[0]] = party;

            character.party = _parties[party[0]];

            makePartyPacket.Ok = true;
            makePartyPacket.ErrorMessage = "Success";
            makePartyPacket.ErrorCode = 0;
            makePartyPacket.PartyId = _partyId;
            _partyId++;
            character.Session.SendPacket(makePartyPacket);
        }

        public void InviteParty(Character character, int targetCharacterDBId, int partyId) {
            S_InviteParty invitePacket = new S_InviteParty();
            invitePacket.CharacterDBId = character.CharacterDBId;
            invitePacket.TargetCharacterDBId = targetCharacterDBId;

            if (character.party == null) {
                Console.WriteLine("현재 속해있는 파티가 없습니다");
                invitePacket.ErrorCode = 1;
                character.Session.SendPacket(invitePacket);
                return;
            }
            if (character.CharacterDBId == targetCharacterDBId) {
                invitePacket.ErrorCode = 2;
                character.Session.SendPacket(invitePacket);
                return;
            }
            if (character.party[0] != partyId) {
                Console.WriteLine("패킷의 파티 아이디와 서버 상의 캐릭터가 속한 파티 아이디가 다릅니다");
                invitePacket.ErrorCode = 5;
                character.Session.SendPacket(invitePacket);
                return;
            }

            Character targetCharacter = GetCharacter(targetCharacterDBId);
            if (targetCharacter == null) {
                Console.WriteLine("InviteParty(): TargetCharacter doesn't exist");
                invitePacket.ErrorCode = 3;
                character.Session.SendPacket(invitePacket);
                return;
            }
            if (targetCharacter.Session == null) {
                Console.WriteLine("InviteParty(): TargetCharacter Session doesn't exist");
                invitePacket.ErrorCode = 6;
                character.Session.SendPacket(invitePacket);
                return;
            }
            if (targetCharacter.party != null) {
                invitePacket.ErrorCode = 4;
                character.Session.SendPacket(invitePacket);
                return;
            }

            S_GotPartyRequest gotPartyRequestPacket = new S_GotPartyRequest();
            gotPartyRequestPacket.PartyId = partyId;
            gotPartyRequestPacket.CharacterDBId = targetCharacterDBId;
            gotPartyRequestPacket.TargetCharacterDBId = character.CharacterDBId;
            targetCharacter.Session.SendPacket(gotPartyRequestPacket);
            targetCharacter.Session.lastPartyRequestTick = Environment.TickCount64;

            invitePacket.ErrorCode = 0;
            character.Session.SendPacket(invitePacket);
        }

        public void AcceptParty(Character character, int partyId) {
            if (character == null) {
                Console.WriteLine("AcceptParty(): Character doesn't exist");
                return;
            }

            S_AcceptParty sAcceptPartyPacket = new S_AcceptParty();
            if (character.party != null) {
                sAcceptPartyPacket.Ok = false;
                sAcceptPartyPacket.ErrorMessage = "이미 파티에 가입되어 있습니다";
                sAcceptPartyPacket.ErrorCode = 2;
                character.Session.SendPacket(sAcceptPartyPacket);
                return;
            }
            int code = JoinParty(partyId, character);
            if (code == 0) {
                CharacterData[] characterDatas = new CharacterData[4];
                Character[] partyCharacters = GetCharactersInParty(partyId);
                for (int i=0; i<4; i++) {
                    if (partyCharacters[i] == null) {
                        continue;
                    }
                    characterDatas[i] = new CharacterData() {
                        CharacterDBId = partyCharacters[i].CharacterDBId,
                        Nickname = partyCharacters[i].Nickname,
                    };

                    //Debug
                    if (ConfigManager.Config.Deploy == false) {
                        Console.WriteLine($"Index: {i} - PlayerID: {partyCharacters[i].CharacterDBId}");
                    }
                }

                sAcceptPartyPacket.Ok = true;
                sAcceptPartyPacket.ErrorMessage = "Success";
                sAcceptPartyPacket.ErrorCode = 0;
                sAcceptPartyPacket.Party = characterDatas;
                sAcceptPartyPacket.PartyId = partyId;
                foreach (Character partyCharacter in partyCharacters) {
                    if (partyCharacter != null) {
                        sAcceptPartyPacket.CharacterDBId = partyCharacter.CharacterDBId;
                        partyCharacter.Session.SendPacket(sAcceptPartyPacket);
                    }
                }
            }
            else if (code == 1) {
                sAcceptPartyPacket.Ok = false;
                sAcceptPartyPacket.ErrorMessage = "파티가 존재하지 않습니다";
                sAcceptPartyPacket.ErrorCode = 3;
                character.Session.SendPacket(sAcceptPartyPacket);
            }
            else if (code == 2) {
                sAcceptPartyPacket.Ok = false;
                sAcceptPartyPacket.ErrorMessage = "파티가 이미 다 찼습니다";
                sAcceptPartyPacket.ErrorCode = 4;
                character.Session.SendPacket(sAcceptPartyPacket);
            }
            else if (code == 3) {
                Console.WriteLine("AcceptParty(): character is null");
            }
        }

        public void AlreadyHaveParty(int partyId) {
            S_AcceptParty sAcceptPartyPacket = new S_AcceptParty();
            sAcceptPartyPacket.Ok = false;
            sAcceptPartyPacket.ErrorMessage = "이미 파티에 가입되어 있습니다";
            sAcceptPartyPacket.ErrorCode = 2;

            // Reset partyId
            partyId *= -1;
            Character partyOwnerCharacter = GetCharacter(_parties[partyId][1]);
            if (partyOwnerCharacter == null) {
                Console.WriteLine("Character doesn't exist");
            }
            partyOwnerCharacter.Session.SendPacket(sAcceptPartyPacket);
        }

        public void DeclineParty(int partyId) {
            if (_parties[partyId] == null) {
                return;
            }
            S_AcceptParty sAcceptPartyPacket = new S_AcceptParty();
            sAcceptPartyPacket.Ok = false;
            sAcceptPartyPacket.ErrorMessage = "상대방이 거절했습니다";
            sAcceptPartyPacket.ErrorCode = 1;
            Character partyOwnerCharacter = GetCharacter(_parties[partyId][1]);
            if (partyOwnerCharacter == null) {
                Console.WriteLine("Character doesn't exist");
            }
            partyOwnerCharacter.Session.SendPacket(sAcceptPartyPacket);
        }

        public void BanishParty(int partyId, int characterDBId, int targetCharacterDBId) {
            int[] party = GetParty(partyId);
            Character character = GetCharacter(characterDBId);

            S_BanishParty banishPacket = new S_BanishParty();
            if (party == null) {
                Console.WriteLine("파티가 존재하지 않습니다");
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "파티가 존재하지 않습니다";
                banishPacket.ErrorCode = 2;
                character.Session.SendPacket(banishPacket);
                return;
            }
            if (character.party == null) {
                Console.WriteLine("해당 캐릭터는 파티에 속해있지 않습니다");
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "파티에 속해있지 않습니다";
                banishPacket.ErrorCode = 1;
                character.Session.SendPacket(banishPacket);
                return;
            }
            if (character.party[0] != partyId) {
                Console.WriteLine("패킷의 파티 아이디와 서버 상의 캐릭터가 속한 파티 아이디가 다릅니다");
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "패킷의 파티 아이디와 서버 상의 캐릭터가 속한 파티 아이디가 다릅니다";
                banishPacket.ErrorCode = 5;
                character.Session.SendPacket(banishPacket);
                return;
            }

            if (party[1] != characterDBId) {
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "파티장이 아닙니다";
                banishPacket.ErrorCode = 3;
                character.Session.SendPacket(banishPacket);
                return;
            }
            int index = FindCharacterDBIdInParty(targetCharacterDBId, partyId);
            if (index == -1) {
                Console.WriteLine("파티가 존재하지 않습니다");
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "파티가 존재하지 않습니다";
                banishPacket.ErrorCode = 2;
                character.Session.SendPacket(banishPacket);
            }
            else if (index == 0) {
                Console.WriteLine("해당 파티에 속해있지 않습니다");
                banishPacket.CharacterDBId = characterDBId;
                banishPacket.Ok = false;
                banishPacket.ErrorMessage = "대상 캐릭터는 해당 파티에 속해있지 않습니다";
                banishPacket.ErrorCode = 4;
                character.Session.SendPacket(banishPacket);
            }
            else {
                for (int i=index; i<=3; i++) {
                    party[i] = party[i + 1];
                }
                party[4] = -1;
                banishPacket.BanishedCharacterDBId = targetCharacterDBId;
                banishPacket.Ok = true;
                banishPacket.ErrorMessage = "Success";
                banishPacket.ErrorCode = 0;
                banishPacket.PartyId = partyId;
                Character[] charactersInParty = GetCharactersInParty(partyId);
                CharacterData[] datas = new CharacterData[charactersInParty.Length];
                for (int i=0; i<charactersInParty.Length; i++) {
                    if (charactersInParty[i] == null) {
                        continue;
                    }
                    datas[i] = new CharacterData() {
                        CharacterDBId = charactersInParty[i].CharacterDBId,
                        Nickname = charactersInParty[i].Nickname,
                    };
                }
                banishPacket.Party = datas;
                for (int i=0; i<charactersInParty.Length; i++) {
                    if (charactersInParty[i] == null) {
                        continue;
                    }
                    banishPacket.CharacterDBId = charactersInParty[i].CharacterDBId;
                    charactersInParty[i].Session.SendPacket(banishPacket);
                }
                Character targetCharacter = GetCharacter(targetCharacterDBId);
                if (targetCharacter == null) {
                    Console.WriteLine("Not exist");
                    return;
                }
                targetCharacter.Session.SendPacket(banishPacket);
                targetCharacter.party = null;
                if (targetCharacter.Room.RoomType == RoomType.SessionGame) {
                    SessionRoom sessionRoom = targetCharacter.Room as SessionRoom;
                    sessionRoom.BackToMainIsland(targetCharacter);
                }
            }
        }

        public void ExitParty(Character character, int partyId) {
            S_ExitParty exitPartyPacket = new S_ExitParty();
            exitPartyPacket.CharacterDBId = character.CharacterDBId;

            int index = FindCharacterDBIdInParty(character.CharacterDBId, partyId);
            if (index == -1) {
                exitPartyPacket.Ok = false;
                exitPartyPacket.ErrorMessage = "파티가 존재하지 않습니다";
                exitPartyPacket.ErrorCode = 1;
                character.Session.SendPacket(exitPartyPacket);
            }
            else if (index == 0) {
                exitPartyPacket.Ok = false;
                exitPartyPacket.ErrorMessage = "해당 파티에 속해있지 않습니다";
                exitPartyPacket.ErrorCode = 2;
                character.Session.SendPacket(exitPartyPacket);
            }
            else {
                int[] party = _parties[partyId];
                for (int i=index; i<=3; i++) {
                    party[i] = party[i + 1];
                }
                party[4] = -1;
                exitPartyPacket.Ok = true;
                exitPartyPacket.ErrorMessage = "Success";
                exitPartyPacket.ErrorCode = 0;
                exitPartyPacket.ExitCharacterDBId = character.CharacterDBId;
                Character[] charactersInParty = GetCharactersInParty(partyId);
                CharacterData[] datas = new CharacterData[charactersInParty.Length];
                for (int i=0; i<charactersInParty.Length; i++) {
                    if (charactersInParty[i] == null) {
                        continue;
                    }
                    datas[i] = new CharacterData() {
                        CharacterDBId = charactersInParty[i].CharacterDBId,
                        Nickname = charactersInParty[i].Nickname,
                    };
                }
                exitPartyPacket.Party = datas;
                for (int i=0; i<charactersInParty.Length; i++) {
                    if (charactersInParty[i] == null) {
                        continue;
                    }
                    charactersInParty[i].Session.SendPacket(exitPartyPacket);
                }
                character.Session.SendPacket(exitPartyPacket);
                character.party = null;
                if (party[1] == -1) {
                    RemoveParty(partyId);
                }
                if (character.Room.RoomType == RoomType.SessionGame) {
                    SessionRoom sessionRoom = character.Room as SessionRoom;
                    sessionRoom.BackToMainIsland(character);
                }
            }
        }

        public void MakePartyItelf(int partyId, int[] party) {
            _parties.Add(partyId, party);
        }

        protected int[] GetParty(int partyId) {
            int[] party = null;
            if (_parties.TryGetValue(partyId, out party)) {
                return party;
            }
            return null;
        }

        protected bool RemoveParty(int partyId) {
            int[] party = null;
            if (_parties.TryGetValue(partyId, out party)) {
                return _parties.Remove(partyId);
            }
            return false;
        }

        protected Character[] GetCharactersInParty(int partyId) {
            int[] party = null;
            if (_parties.TryGetValue(partyId, out party)) {
                Character[] partyCharacters = new Character[4];
                for (int i=1; i<=4; i++) {
                    if (party[i] == -1) {
                        continue;
                    }
                    partyCharacters[i-1] = GetCharacter(party[i]);
                }
                return partyCharacters;
            }
            else {
                Console.WriteLine("GetCharactersInParty(): Party doesn't exist");
                return null;
            }
        }

        protected void ClearParty() {
            _parties.Clear();
        }

        int FindCharacterDBIdInParty(int characterDBId, int partyId) {
            //-1: 파티가 존재하지 않음
            //0: 해당 파티에 캐릭터가 속해있지 않음
            //1~4: 해당 인덱스에 캐릭터가 존재
            int[] party = null;
            if (_parties.TryGetValue(partyId, out party)) {
                int index = 0;
                for (int i=1; i<=4; i++) {
                    if (party[i] == characterDBId) {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            else {
                Console.WriteLine("FindCharacterDBIdInParty(): Party doesn't exist");
                return -1;
            }
        }

        int FindEmptySlotInParty(int partyId) {
            int[] party = null;
            if (_parties.TryGetValue(partyId, out party)) {
                int emptySlot = 0;
                for (int i=1; i<=4; i++) {
                    if (party[i] == -1) {
                        emptySlot = i;
                        break;
                    }
                }
                if (emptySlot == 0) {
                    return 0;
                }
                return emptySlot;
            }
            else {
                Console.WriteLine("FindEmptySlotInParty(): Party doesn't exist");
                return -1;
            }
        }

        int JoinParty(int partyId, Character character) {
            if (character == null) {
                Console.WriteLine("JoinParty(): character is null");
                return 3;
            }
            int[] party = null;
            int index = FindEmptySlotInParty(partyId);
            if (index == -1) {
                //파티가 존재하지 않음
                return 1;
            }
            else if (index == 0) {
                //파티가 이미 참
                return 2;
            }
            else {
                _parties[partyId][index] = character.CharacterDBId;
                character.party = _parties[partyId];
                return 0;
            }
        }
        #endregion

        #region Manage
        public InteractiveObject AddObject(ObjectType objectType, GameRoom room, float x, float y, float z, float radius) {
            InteractiveObject interactive = new InteractiveObject(objectType, room, radius) {
                posX = x,
                posY = y,
                posZ = z,
                ObjectID = _interactiveID++,
            };
            _interactives.Add(interactive.ObjectID, interactive);
            Collision.AddObject(x, z, interactive.ObjectID);
            return interactive;
        }

        public int AddAnimal(GameRoom room, float x, float y, float z, float radius) {
            Animal animal = new Animal(room) {
                posX = x,
                posY = y,
                posZ = z,
                ObjectID = _animalId++,
            };
            _animals.Add(animal.ObjectID, animal);
            Collision.AddObject(x, z, animal.ObjectID);
            return animal.ObjectID;
        }

        public int AddNPC(GameRoom room, float x, float y, float z) {
            NPC npc = new NPC(room) {
                posX = x,
                posY = y,
                posZ = z,
                ObjectID = _npcId++,
            };
            _NPCs.Add(npc.ObjectID, npc);
            return npc.ObjectID;
        }

        public InteractiveObject AddGround(int z, int x, GameRoom room) {
            InteractiveObject interactive = new InteractiveObject(ObjectType.Ground, room, 0) {
                ObjectID = _farmObjectId++,
                posX = x * SessionRoom.Offset,
                posY = 0,
                posZ = z * SessionRoom.Offset,
                Room = this,
            };
            return interactive;
        }

        public void ChangeObjectTypeById(int objectId, ObjectType origin, ObjectType changeTo) {
            InteractiveObject interactive = GetObject(objectId);
            if (interactive == null) {
                return;
            }
            if (interactive.ObjectType != origin) {
                return;
            }
            _interactives[objectId].ObjectType = changeTo;
        }

        public InteractiveObject GetObject(int obejctID) {
            InteractiveObject interactive = null;
            if (_interactives.TryGetValue(obejctID, out interactive)) {
                return interactive;
            }
            return null;
        }

        public List<InteractiveObject> GetObjects() {
            List<InteractiveObject> interactives = new List<InteractiveObject>();
            foreach (InteractiveObject interactive in _interactives.Values) {
                interactives.Add(interactive);
            }
            return interactives;
        }

        public List<InteractiveObject> GetObjectsByObjectType(ObjectType objectType) {
            List<InteractiveObject> interactives = new List<InteractiveObject>();
            foreach (InteractiveObject interactive in _interactives.Values) {
                if (interactive.ObjectType == objectType) {
                    interactives.Add(interactive);
                }
            }
            return interactives;
        }

        public float GetObjectCollider(int objectId) {
            InteractiveObject interactive = null;
            if (_interactives.TryGetValue(objectId, out interactive)) {
                return interactive.collider;
            }
            Animal animal = null;
            if (_animals.TryGetValue(objectId, out animal)) {
                return animal.collider;
            }
            return 0;
        }

        public bool GetObjectCanPassThrough(int objectId) {
            InteractiveObject interactive = null;
            if (_interactives.TryGetValue(objectId, out interactive)) {
                return interactive.canPassThough;
            }
            return true;
        }

        public Animal GetAnimal(int animalId) {
            Animal animal = null;
            if (_animals.TryGetValue(animalId, out animal)) {
                return animal;
            }
            return null;
        }

        public List<Animal> GetAnimals() {
            List<Animal> animals = new List<Animal>();
            foreach (Animal animal in _animals.Values) {
                animals.Add(animal);
            }
            return animals;
        }

        public List<NPC> GetNPCs() {
            List<NPC> npcs = new List<NPC>();
            foreach (NPC npc in _NPCs.Values) {
                npcs.Add(npc);
            }
            return npcs;
        }

        public void RemoveObject(int objectID) {
            if (_interactives.ContainsKey(objectID)) {
                _interactives.Remove(objectID);
            }
            Collision.RemoveObject(objectID);
        }

        public void RemoveObject(int objectID, InteractiveObject scarecrow) {
            if (scarecrow != null) {
                foreach (Animal animal in scarecrow.GetAnimals()) {
                    animal.ResetTarget();
                }
            }
            _interactives.Remove(objectID);
            Collision.RemoveObject(objectID);
        }

        public void RemoveAnimal(Animal animal) {
            if (animal.job != null) {
                animal.job.Cancel = true;
            }
            animal.ResetTarget();
            _animals.Remove(animal.ObjectID);
            Collision.RemoveObject(animal.ObjectID);
        }

        public void AddCharacter(Character character) {
            if (!_characters.ContainsKey(character.CharacterDBId)) {
                _characters.Add(character.CharacterDBId, character);
            }
        }

        public void AddCharacters(List<Character> characters) {
            foreach (Character character in characters) {
                _characters.Add(character.CharacterDBId, character);
            }
        }

        public List<Character> GetCharacters() {
            List<Character> characters = new List<Character>();
            foreach (Character character in _characters.Values) {
                characters.Add(character);
            }
            return characters;
        }
        
        public Character GetCharacter(int characterDBId) {
            Character character = null;
            if (_characters.TryGetValue(characterDBId, out character)) {
                return character;
            }
            return null;
        }

        public bool RemoveCharacter(int characterDBId) {
            return _characters.Remove(characterDBId);
        }

        public void RemoveSessionGame(int sessionRoomId) {
            //주의
            GameLogic.Instance.Remove(RoomType.SessionGame, sessionRoomId);
        }

        public virtual void Clear() {
            _characters.Clear();
            _interactives.Clear();
            _parties.Clear();

            Collision = null;
        }

        public void Broadcast(Character character, IPacket packet, int immediate = 0) {
            foreach (Character c in _characters.Values) {
                if (c.CharacterDBId == character.CharacterDBId)
                    continue;
                if (c.Session == null)
                    continue;
                c.Session.SendPacket(packet, immediate);
            }
        }

        public void Broadcast(IPacket packet, int immediate = 0) {
            foreach (Character c in _characters.Values) {
                if (c.Session == null)
                    continue;
                c.Session.SendPacket(packet, immediate);
            }
        }

        public void BroadcastWithoutParty(IPacket packet, Character[] charactersInParty, int immediate =  0) {
            foreach (Character c in _characters.Values) {
                if (c.Session == null)
                    continue;

                bool isInParty = false;
                for (int i=0; i<charactersInParty.Length; i++) {
                    if (charactersInParty[i] == null) {
                        continue;
                    }
                    if (c.CharacterDBId == charactersInParty[i].CharacterDBId) {
                        isInParty = true;
                        break;
                    }
                }

                if (isInParty)
                    continue;

                c.Session.SendPacket(packet, immediate);
            }
        }
        #endregion
    }
}