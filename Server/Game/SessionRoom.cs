using Server.Data;
using Server.DB;
using Server.Job;
using Server.Object;
using System.Numerics;

namespace Server.Game {
    class SessionRoom : GameRoom {
        public InteractiveObject[,] Field = new InteractiveObject[FieldSize, FieldSize];

        //TEMP
        public const int FieldSize = 31;
        public const int Offset = 5;
        public const int WinterWarmSize = 1;
        public const int FirePositionZ = FieldSize / 2;
        public const int FirePositionX = FieldSize / 2;
        const int SpawnStoneCount = 300;
        const int SeasonTime = 60 * 2 + 3;
        const int SendTime = 60 * 2;
        const int ForcedStartGame = 30;
        const float SeedRadius = 0.5f;
        const int SpawnAnimalInterval = 20;
        const int FinishTimeWhenAllCropHarvested = 5;
        const int MinSeedForCollect = 5;
        const int MaxSeedForCollect = 10;
        const int SeedSpawnTime = 15;
        static readonly float[,] SpawnPosition = new float[,] {
            { FirePositionX * Offset - (1 * (float)Offset / 2), 0, FirePositionZ * Offset - (1 * (float)Offset / 2) },
            { FirePositionX * Offset + (1 * (float)Offset * 3 / 2), 0, FirePositionZ * Offset - (1 * (float)Offset / 2) },
            { FirePositionX * Offset - (1 * (float)Offset / 2), 0, FirePositionZ * Offset + (1 *(float) Offset * 3 / 2) },
            { FirePositionX * Offset + (1 * (float)Offset * 3 / 2), 0, FirePositionZ * Offset + (1 *(float) Offset * 3 / 2) }
        };

        public Season season { get; private set; } = Season.Winter;

        HashSet<(int x, int y)> _usedPositions = new HashSet<(int, int)>();
        Random _random = new Random();
        List<int> _notChosenSkill = new List<int>();
        int _skillId = 20003;
        int _crop = 0;
        int _harvestedCrops = 0;
        int _partyId;
        bool _alreadyStarted = false;
        bool _changeSeasonByButton = false;
        IJob _changeSeasonJob;
        IJob _generateWeedJob;
        IJob _generateSeedJob;
        IJob _forcedStartGameJob;

        public SessionRoom() : base(RoomType.SessionGame) {
            foreach (int skillId in DataManager.SkillDict.Keys) {
                _notChosenSkill.Add(skillId);
            }
            for (int i=0; i< FieldSize; i++) {
                for (int j=0; j< FieldSize; j++) {
                    Field[i, j] = AddGround(i, j, this);
                }
            }

            Field[FirePositionZ, FirePositionX].ObjectType = ObjectType.Fire;
            for (int i = FirePositionZ - WinterWarmSize; i <= FirePositionZ + WinterWarmSize; i++) {
                for (int j = FirePositionX - WinterWarmSize; j <= FirePositionX + WinterWarmSize; j++) {
                    Field[i, j].isWarm = true;
                }
            }
        }

        public override void Init() {
            base.Init();
        }

        public InteractiveObject GetObjectInField(int objectId) {
            for (int i=0; i<FieldSize; i++) {
                for (int j=0; j<FieldSize; j++) {
                    if (Field[i, j].ObjectID == objectId) {
                        return Field[i, j];
                    }
                }
            }
            return null;
        }

        public List<InteractiveObject> GetObjectsInField() {
            List<InteractiveObject> farmObjects = new List<InteractiveObject>();
            for (int i=0; i<FieldSize; i++) {
                for (int j=0; j<FieldSize; j++) {
                    farmObjects.Add(Field[i, j]);
                }
            }
            return farmObjects;
        }

        public List<InteractiveObject> GetObjectsByTypeInField(ObjectType objectType) {
            List<InteractiveObject> objects = new List<InteractiveObject>();
            for (int i=0; i< FieldSize; ++i) {
                for (int j=0; j<FieldSize; j++) {
                    if (Field[i, j].ObjectType == objectType) {
                        objects.Add(Field[i, j]);
                    }
                }
            }
            return objects;
        }

        public void ChangeSeasonByButton() {
            Season newSeason = (Season)((int)season + 1);
            if ((int)newSeason > 3) {
                return;
            }
            if (_changeSeasonJob != null) {
                _changeSeasonJob.Cancel = true;
            }
            ChangeSeason(newSeason);
            _changeSeasonByButton = true;
        }

        bool IsAroundFire(int z, int x) {
            int[] dz = new int[8] {
                FirePositionZ + 1, FirePositionZ + 1, FirePositionZ, FirePositionZ -1, FirePositionZ - 1, FirePositionZ - 1, FirePositionZ, FirePositionZ + 1
            };
            int[] dx = new int[8] {
                FirePositionX, FirePositionX + 1, FirePositionX + 1, FirePositionX + 1, FirePositionX, FirePositionX - 1, FirePositionX - 1, FirePositionX - 1
            };
            bool isInRange = false;
            for (int i=0; i<8; i++) {
                if (dz[i] == z && dx[i] == x) {
                    isInRange = true;
                    break;
                }
            }
            return isInRange;
        }

        void ChangeSeason(Season season) {
            this.season = season;

            List<Animal> animals = GetAnimals();
            S_DespawnObject despawnAnimalPacket = new S_DespawnObject() {
                ObjectType = ObjectType.Animal,
                All = true,
            };

            foreach (Animal animal in animals) {
                RemoveAnimal(animal);
                despawnAnimalPacket.ObjectIDs.Add(animal.ObjectID);
            }
            Broadcast(despawnAnimalPacket);

            if (season == Season.Winter) {
                for(int i=0; i<SpawnStoneCount;i++){
                    int randX = _random.Next(0, FieldSize);
                    int randZ = _random.Next(0, FieldSize);
                    if (Field[randZ, randX].ObjectType == ObjectType.Fire)
                        continue;
                    if (IsAroundFire(randZ, randX)) {
                        continue;
                    }
                    Field[randZ, randX].ObjectType = ObjectType.Stone;
                }

                //_changeSeasonJob = PushAfter(1000 * SeasonTime, ChangeSeason, Season.Spring);
            }
            else if (season == Season.Spring) {
                List<InteractiveObject> fields = GetObjectsByTypeInField(ObjectType.Field);
                if (fields.Count == 0) {
                    //종료
                    CantContinueGame();
                }
                List<InteractiveObject> seeds = new List<InteractiveObject> {
                    AddObject(ObjectType.SeedForCollect, this, 70, 0, 70, SeedRadius),
                    AddObject(ObjectType.SeedForCollect, this, 80, 0, 80, SeedRadius),
                    AddObject(ObjectType.SeedForCollect, this, 70, 0, 80, SeedRadius),
                    AddObject(ObjectType.SeedForCollect, this, 65, 0, 75, SeedRadius),
                    AddObject(ObjectType.SeedForCollect, this, 62, 0, 85, SeedRadius),
                    AddObject(ObjectType.SeedForCollect, this, 60, 0, 69, SeedRadius)
                };
                _usedPositions.Add((70, 70));
                _usedPositions.Add((80, 80));
                _usedPositions.Add((70, 80));
                _usedPositions.Add((65, 75));
                _usedPositions.Add((62, 85));
                _usedPositions.Add((60, 69));
                S_ChangeSeason seasonPacket = new S_ChangeSeason() {
                    Season = Season.Spring,
                };

                foreach (InteractiveObject seed in seeds) {
                    ObjectData data = new ObjectData() {
                        ObjectId = seed.ObjectID,
                        ObjectType = seed.ObjectType,
                        PosX = seed.posX,
                        PosY = seed.posY,
                        PosZ = seed.posZ,
                    };
                    seasonPacket.Objects.Add(data);
                }

                List<Character> characters = GetCharacters();
                foreach (Character character in characters) {
                    character.exhausted = false;
                    S_Exhaust exhaustPacket = new S_Exhaust() {
                        TargetCharacterDBId = character.CharacterDBId,
                        Exhaust = false,
                    };
                    Broadcast(exhaustPacket);
                }
                Broadcast(seasonPacket);
                _changeSeasonJob = PushAfter(1000 * SeasonTime, ChangeSeason, Season.Summer);
                _generateSeedJob = PushAfter(1000 * SeedSpawnTime, GenerateSeeds);

                PushAfter(1000 * SpawnAnimalInterval, SpawnAnimal, SpawnAnimalInterval, 1);
            }
            else if (season == Season.Summer) {
                List<InteractiveObject> seedForPlants = GetObjectsByTypeInField(ObjectType.SeedForPlant);
                if (seedForPlants.Count == 0) {
                    //종료
                    CantContinueGame();
                }
                if (_generateSeedJob != null) {
                    _generateSeedJob.Cancel = true;
                }
                //계절 변경
                S_ChangeSeason seasonPacket = new S_ChangeSeason() {
                    Season = Season.Summer,
                };

                //SeedForPlant -> Plant
                S_ChangeObject objectPacket = new S_ChangeObject() {
                    From = ObjectType.SeedForPlant,
                    To = ObjectType.Plant,
                };
                foreach (InteractiveObject seedForPlant in seedForPlants) {
                    ChangeFieldObjectType(seedForPlant, ObjectType.Plant);
                    objectPacket.ObjectIds.Add(seedForPlant.ObjectID);
                }

                //줍지 않은 씨앗은 제거
                S_DespawnObject despawnPacket = new S_DespawnObject() {
                    ObjectType = ObjectType.SeedForCollect,
                    All = true,
                };
                List<InteractiveObject> seedForCollects = GetObjectsByObjectType(ObjectType.SeedForCollect);
                foreach (InteractiveObject seedForCollect in seedForCollects) {
                    RemoveObject(seedForCollect.ObjectID);
                    despawnPacket.ObjectIDs.Add(seedForCollect.ObjectID);
                }

                Broadcast(seasonPacket);
                Broadcast(objectPacket);
                Broadcast(despawnPacket);
                _changeSeasonJob = PushAfter(1000 * SeasonTime, ChangeSeason, Season.Autumn);
                int generateWeedNum = (GetCharacters().Count == 2 || GetCharacters().Count == 4) ? 2 : 1;
                _generateWeedJob = PushAfter(1000 * 10, GenerateWeeds, 10, 0, generateWeedNum);
            }
            else if (season == Season.Autumn) {
                List<InteractiveObject> plants = GetObjectsByTypeInField(ObjectType.Plant);
                List<InteractiveObject> weeds = GetObjectsByTypeInField(ObjectType.Weed);
                if (plants.Count == 0 && weeds.Count == 0) {
                    //종료
                    CantContinueGame();
                }
                //Change Season
                if (_generateWeedJob != null) {
                    _generateWeedJob.Cancel = true;
                }
                S_ChangeSeason seasonPacket = new S_ChangeSeason() {
                    Season = Season.Autumn,
                };

                //Plant -> Crop
                //잡초를 제거하지 못한 Plant는 그대로
                S_ChangeObject objectPacket = new S_ChangeObject() {
                    From = ObjectType.Plant,
                    To = ObjectType.Crop,
                };
                foreach (InteractiveObject plant in plants) {
                    ChangeFieldObjectType(plant, ObjectType.Crop);
                    objectPacket.ObjectIds.Add(plant.ObjectID);
                    _crop++;
                }

                //Weed -> Crop
                S_ChangeObject changePacket = new S_ChangeObject() {
                    From = ObjectType.Weed,
                    To = ObjectType.Crop
                };
                foreach (InteractiveObject weed in weeds) {
                    ChangeFieldObjectType(weed, ObjectType.Crop);
                    changePacket.ObjectIds.Add(weed.ObjectID);
                    _crop++;
                }
                Broadcast(seasonPacket);
                Broadcast(objectPacket);
                Broadcast(changePacket);

                _changeSeasonJob = PushAfter(1000 * SeasonTime, RewardCharacters);

                PushAfter(1000 * SpawnAnimalInterval, SpawnAnimal, SpawnAnimalInterval, 1);
            }
        }

        void GenerateSeeds() {
            S_SpawnObject spawnPacket = new S_SpawnObject();
            for (int i = 0; i < _random.Next(MinSeedForCollect, MaxSeedForCollect); i++) {
                int randX;
                int randZ;
                // 중복되지 않는 좌표를 찾을 때까지 반복
                do {
                    randX = _random.Next((FirePositionX - 8) * Offset, (FirePositionX + 8) * Offset);
                    randZ = _random.Next((FirePositionZ - 8) * Offset, (FirePositionZ + 8) * Offset);
                } while (_usedPositions.Contains((randX, randZ)));

                // 중복되지 않는 좌표를 찾았으므로 저장
                _usedPositions.Add((randX, randZ));
                InteractiveObject seed = AddObject(ObjectType.SeedForCollect, this, randX, 0, randZ, SeedRadius);
                spawnPacket.Objects.Add(new ObjectData() {
                    ObjectType = ObjectType.SeedForCollect,
                    ObjectId = seed.ObjectID,
                    PosX = randX,
                    PosY = 0,
                    PosZ = randZ,
                });
            }
            Broadcast(spawnPacket);

            _generateSeedJob = PushAfter(1000 * SeedSpawnTime, GenerateSeeds);
        }

        void GenerateWeeds(int interval, int curTime, int num) {
            if (season != Season.Summer) {
                return;
            }
            curTime += interval;
            List<InteractiveObject> plants = GetObjectsByTypeInField(ObjectType.Plant);
            S_ChangeObject objectPacket = new S_ChangeObject() {
                From = ObjectType.Plant,
                To = ObjectType.Weed,
            };

            if (plants.Count < num) {
                foreach (InteractiveObject plant in plants) {
                    plant.ObjectType = ObjectType.Weed;
                    objectPacket.ObjectIds.Add(plant.ObjectID);
                    PushAfter(1000 * 3, plant.ChangeHp, -10, 2);
                }
                Broadcast(objectPacket);
                _generateWeedJob = PushAfter(1000 * 2, GenerateWeeds, interval, curTime, num);
                return;
            }

            int partyMemberCount = GetCharacters().Count;

            if (curTime >= 90) {
                interval = partyMemberCount >= 3 ? 2 : 3;
            }
            else if (curTime >= 60) {
                interval = partyMemberCount >= 3 ? 3 : 4;
            }
            else if (curTime >= 40) {
                interval = partyMemberCount >= 3 ? 4 : 5;
            }
            else if (curTime >= 20) {
                interval = partyMemberCount >= 3 ? 5 : 6;
            }
            else if (curTime >= 10) {
                interval = partyMemberCount >= 3 ? 6 : 8;
            }

            List<InteractiveObject> selected = new List<InteractiveObject>();
            List<int> usedIndices = new List<int>();

            while (selected.Count < num) {
                int index = _random.Next(plants.Count());

                if (!usedIndices.Contains(index)) {
                    selected.Add(plants[index]);
                    usedIndices.Add(index);
                }
            }

            foreach (InteractiveObject plant in selected) {
                plant.ObjectType = ObjectType.Weed;
                objectPacket.ObjectIds.Add(plant.ObjectID);
                PushAfter(1000 * 3, plant.ChangeHp, -10, 2);
            }
            Broadcast(objectPacket);
            _generateWeedJob = PushAfter(1000 * interval, GenerateWeeds, interval, curTime, num);
        }

        public void CheckPlayerReady(Character character, bool loadingComplete) {
            if (character == null) {
                return;
            }

            if (_forcedStartGameJob != null) {
                _forcedStartGameJob.Cancel = true;
            }

            if (loadingComplete) {
                character.sessionGameLoadingCompleted = true;
            }

            bool allPlayerLoadingCompleted = true;
            List<Character> characters = GetCharacters();
            foreach (Character c in characters) {
                if (c.sessionGameLoadingCompleted == false) {
                    allPlayerLoadingCompleted = false;
                    break;
                }
            }

            if (!_alreadyStarted) {
                if (allPlayerLoadingCompleted) {
                    //Broadcast
                    S_LoadingComplete loadingPacket = new S_LoadingComplete();
                    Broadcast(loadingPacket);
                    Tick(1, 0);
                    _changeSeasonJob = PushAfter(1000 * SeasonTime, ChangeSeason, Season.Spring);
                    _alreadyStarted = true;
                }
                else {
                    _forcedStartGameJob = PushAfter(1000 * ForcedStartGame, () => {
                        //Broadcast
                        S_LoadingComplete loadingPacket = new S_LoadingComplete();
                        Broadcast(loadingPacket);
                        Tick(1, 0);
                        _changeSeasonJob = PushAfter(1000 * SeasonTime, ChangeSeason, Season.Spring);
                        _alreadyStarted = true;
                    });
                }
            }
        }

        public void StartGame(Character[] charactersInParty, int partyId, List<int> chosenSkills) {
            foreach (int chosenSkillId  in chosenSkills) {
                if (_notChosenSkill.Contains(chosenSkillId)) {
                    _notChosenSkill.Remove(chosenSkillId);
                }
            }

            CharacterData[] characterDatas = new CharacterData[4];
            List<ObjectData> objectDatas = new List<ObjectData>();
            int[] party = { -1, -1, -1, -1, -1 };
            party[0] = partyId;
            _partyId = partyId;
            for (int i = 0; i < charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                party[i + 1] = charactersInParty[i].CharacterDBId;
                charactersInParty[i].Location = (int)RoomType.SessionGame;
                charactersInParty[i].posX = SpawnPosition[i, 0];
                charactersInParty[i].posY = 0;
                charactersInParty[i].posZ = SpawnPosition[i, 2];
                charactersInParty[i].Room = this;
                if (charactersInParty[i].skillId == -1) {
                    int randomIndex = _random.Next(_notChosenSkill.Count);
                    charactersInParty[i].skillId = _notChosenSkill[randomIndex];
                    Console.WriteLine($"{charactersInParty[i].Nickname} : {_notChosenSkill[randomIndex]}");
                    _notChosenSkill.RemoveAt(randomIndex);
                }
                SkillData skillData = null;
                if (DataManager.SkillDict.TryGetValue(charactersInParty[i].skillId, out skillData)) {
                    if (skillData.season == Season.Winter) {
                        charactersInParty[i].maxHp = (int)skillData.level[charactersInParty[i].skillLevel[charactersInParty[i].skillId]]["health"];
                        charactersInParty[i].ResetHp();
                    }
                }
                AddCharacter(charactersInParty[i]);
                characterDatas[i] = new CharacterData() {
                    CharacterDBId = charactersInParty[i].CharacterDBId,
                    PosX = charactersInParty[i].posX,
                    PosY = charactersInParty[i].posY,
                    PosZ = charactersInParty[i].posZ,
                    Nickname = charactersInParty[i].Nickname,
                    SkillId = charactersInParty[i].skillId == -1 ? _skillId : charactersInParty[i].skillId,
                    SkillLevel = charactersInParty[i].skillLevel[_skillId],
                };
            }
            MakePartyItelf(partyId, party);

            ChangeSeason(Season.Winter);

            //Field Object 넣기
            for (int i=0; i<FieldSize; i++) {
                for (int j=0; j<FieldSize; j++) {
                    if (Field[i, j].ObjectType == ObjectType.Ground) {
                        continue;
                    }
                    ObjectData data = new ObjectData() {
                        ObjectId = Field[i, j].ObjectID,
                        ObjectType = Field[i, j].ObjectType,
                        PosX = Field[i, j].posX,
                        PosY = Field[i, j].posY,
                        PosZ = Field[i, j].posZ,
                    };
                    objectDatas.Add(data);
                }
            }

            S_EnterSessionGame sessionGamePacket = new S_EnterSessionGame() {
                Ok = true,
                ErrorMessage = "Success",
                ErrorCode = 0,
                PartyId = partyId,
                Party = characterDatas,
                Objects = objectDatas,
            };
            for (int i = 0; i < charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                charactersInParty[i].Session.SendPacket(sessionGamePacket);
            }
        }

        void Tick(int interval, int tickAmount) {
            if (tickAmount == SendTime || _changeSeasonByButton) {
                tickAmount = 0;
                _changeSeasonByButton = false;
            }
            S_CheckTime timePacket = new S_CheckTime() {
                Interval = interval,
                TickAmount = tickAmount
            };
            Broadcast(timePacket);
            PushAfter(1000 * interval, Tick, interval, tickAmount + 1);
        }

        public void RewardCharacters() {
            int rewardCoin = _harvestedCrops * 100;
            List<Character> characters = GetCharacters();

            S_TimeOver timeOverPacket = new S_TimeOver() {
                RewardCoin = rewardCoin
            };
            foreach (Character character in characters) {
                timeOverPacket.CharacterDBId = character.CharacterDBId;
                timeOverPacket.DestroyedStone = character.destroyedStone;
                timeOverPacket.CreatedField = character.createdField;
                timeOverPacket.CollectedSeed = character.collectedSeed;
                timeOverPacket.PlantedSeed = character.plantedSeed;
                timeOverPacket.RemovedWeed = character.removedWeed;
                timeOverPacket.CatchedAnimal = character.catchedAnimal;
                timeOverPacket.HarvestedCrops = character.harvestedCrops;
                character.Session.SendPacket(timeOverPacket);
                character.Money += rewardCoin;
                DbTransaction.GetReward(character, rewardCoin);
            }

            PushAfter(5000, FinishGame);
        }

        public void FinishGame() {
            List<Character> characters = GetCharacters();
            GameRoom room = GameLogic.Instance.GetRoom(RoomType.MainIsland);
            S_FinishSessionGame finishPacket = new S_FinishSessionGame();

            int[] party = { -1, -1, -1, -1, -1 };
            party[0] = _partyId;
            for (int i=0; i<characters.Count; i++) {
                party[i + 1] = characters[i].CharacterDBId;
            }
            room.Push(room.MakePartyItelf, party[0], party);
            foreach (Character character in characters) {
                finishPacket.CharacterDBId = character.CharacterDBId;
                character.Session.SendPacket(finishPacket);
                BackToMainIsland(character);
            }
            ClearParty();
            room.Push(room.RemoveSessionGame, RoomId);
        }

        public void BackToMainIsland(Character character) {
            character.ResetSessionGameCount();
            character.Session.HandleEnterGame(RoomType.MainIsland, true);
            RemoveCharacter(character.CharacterDBId);
        }

        public void StartInteract(int characterDBId, int objectId, ObjectType objectType, InteractType interactType) {
            InteractiveObject interactive = null;
            if (objectType == ObjectType.Animal) {
                interactive = GetAnimal(objectId) as InteractiveObject;
            }
            else {
                interactive = GetObject(objectId);
                if (interactive == null) {
                    interactive = GetObjectInField(objectId);
                }
            }
            Character character = GetCharacter(characterDBId);
            S_StartInteract interactObjectPacket = new S_StartInteract();
            if (interactive == null) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "오브젝트가 존재하지 않습니다";
                interactObjectPacket.ErrorCode = 2;
                character.Session.SendPacket(interactObjectPacket);
                Console.WriteLine($"오브젝트가 존재하지 않음. 캐릭터: {characterDBId}, 오브젝트아이디: {objectId}, 오브젝트타입: {objectType}, 상호작용타입: {interactType}");
                return;
            }
            if (interactive.ObjectType == ObjectType.None) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "None인 오브젝트입니다";
                interactObjectPacket.ErrorCode = 9;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (interactive.ObjectType != objectType) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "오브젝트가 다릅니다";
                interactObjectPacket.ErrorCode = 4;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (interactive.interacting) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "누군가가 상호작용 중입니다";
                interactObjectPacket.ErrorCode = 1;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (interactive.ObjectType != ObjectType.Puppet && interactive.disturbedByAnimal) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "작물을 먹고 있는 동물부터 잡아야합니다";
                interactObjectPacket.ErrorCode = 5;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (interactive.hp <= 0) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "체력이 0입니다";
                interactObjectPacket.ErrorCode = 7;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (interactType == InteractType.CatchAnimal) {
                Animal animal = interactive as Animal;
                if (animal != null) {
                    if (animal.isMoving()) {
                        interactObjectPacket.CharacterDBId = characterDBId;
                        interactObjectPacket.Ok = false;
                        interactObjectPacket.ErrorMessage = "동물이 농작물에 도달한 후에 잡을 수 있습니다";
                        interactObjectPacket.ErrorCode = 11;
                        character.Session.SendPacket(interactObjectPacket);
                        return;
                    }
                }
            }
            if (interactType == InteractType.PlantingSeed) {
                if (character.seed == 0) {
                    interactObjectPacket.CharacterDBId = characterDBId;
                    interactObjectPacket.Ok = false;
                    interactObjectPacket.ErrorMessage = "씨앗이 없습니다";
                    interactObjectPacket.ErrorCode = 8;
                    character.Session.SendPacket(interactObjectPacket);
                    return;
                }
            }
            if (interactive.ObjectType == ObjectType.Puppet && interactive.GetNearAnimals().Count == 0) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "허수아비 주변에 동물이 없습니다";
                interactObjectPacket.ErrorCode = 10;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            if (character.exhausted) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "탈진한 상태입니다";
                interactObjectPacket.ErrorCode = 6;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            bool exist = false;
            for (int i = 0; i < character.canInteraction.Count; i++) {
                if (character.canInteraction[i] == objectId) {
                    exist = true;
                    break;
                }
            }
            if (!exist) {
                interactObjectPacket.CharacterDBId = characterDBId;
                interactObjectPacket.Ok = false;
                interactObjectPacket.ErrorMessage = "해당 오브젝트가 플레이어 주변에 존재하지 않습니다";
                interactObjectPacket.ErrorCode = 3;
                character.Session.SendPacket(interactObjectPacket);
                return;
            }
            interactive.interacting = true;
            character.startInteraction = Environment.TickCount64;
            character.recentInteractObjectId = objectId;
            character.interacting = true;
            interactObjectPacket.CharacterDBId = characterDBId;
            interactObjectPacket.ObjectId = objectId;
            interactObjectPacket.ObjectType = objectType;
            interactObjectPacket.InteractType = interactType;
            interactObjectPacket.Ok = true;
            interactObjectPacket.ErrorMessage = "Success";
            interactObjectPacket.ErrorCode = 0;
            Broadcast(interactObjectPacket);
        }

        public void FinishInteract(C_FinishInteract inputPacket) {
            Character character = GetCharacter(inputPacket.CharacterDBId);
            InteractiveObject interactive = null;
            if (inputPacket.ObjectType == ObjectType.Animal) {
                interactive = GetAnimal(inputPacket.ObjectId) as InteractiveObject;
            }
            else {
                interactive = GetObject(inputPacket.ObjectId);
                if (interactive == null) {
                    interactive = GetObjectInField(inputPacket.ObjectId);
                }
            }
            S_FinishInteract finishInteractPacket = new S_FinishInteract();

            if (interactive == null) {
                finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                finishInteractPacket.Ok = false;
                finishInteractPacket.ErrorMessage = "오브젝트가 존재하지 않습니다";
                finishInteractPacket.ErrorCode = 5;
                finishInteractPacket.ObjectId = inputPacket.ObjectId;
                finishInteractPacket.InteractType = inputPacket.InteractType;
                finishInteractPacket.ObjectType = inputPacket.ObjectType;
                Broadcast(finishInteractPacket);
                MakeRecentObjectInteractingFalse(character, inputPacket.ObjectType);
                character.interacting = false;
                Console.WriteLine($"오브젝트가 존재하지 않음 캐릭터: {inputPacket.CharacterDBId}, 오브젝트아이디: {inputPacket.ObjectId}, " +
                    $"오브젝트타입: {inputPacket.ObjectType}, 상호작용타입: {inputPacket.InteractType}");
                return;
            }

            if (inputPacket.ObjectId != character.recentInteractObjectId) {
                Console.WriteLine($"상호작용 시작할 때 보낸 ObjectId와 지금 보내진 ObjectId가 다릅니다 " +
                    $"캐릭터: {inputPacket.CharacterDBId}, 오브젝트아이디: {inputPacket.ObjectId}, 오브젝트타입: {inputPacket.ObjectType}, 상호작용타입: {inputPacket.InteractType}" +
                    $"이전 오브젝트: {character.recentInteractObjectId}");
                finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                finishInteractPacket.Ok = false;
                finishInteractPacket.ErrorMessage = "상호작용 시작할 때 보낸 ObjectId와 지금 보내진 ObjectId가 다릅니다";
                finishInteractPacket.ErrorCode = 6;
                Broadcast(finishInteractPacket);
                MakeRecentObjectInteractingFalse(character, inputPacket.ObjectType);
                interactive.interacting = false;
                character.interacting = false;
                return;
            }

            if (interactive.ObjectType != inputPacket.ObjectType) {
                finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                finishInteractPacket.Ok = false;
                finishInteractPacket.ErrorMessage = "오브젝트가 다릅니다";
                finishInteractPacket.ErrorCode = 6;
                Broadcast(character, finishInteractPacket);
                character.Session.SendPacket(finishInteractPacket);
                Console.WriteLine($"오브젝트가 다름 캐릭터: {inputPacket.CharacterDBId}, 오브젝트아이디: {inputPacket.ObjectId}, " +
                    $"오브젝트타입: {inputPacket.ObjectType}, 상호작용타입: {inputPacket.InteractType}");
                interactive.interacting = false;
                character.interacting = false;
                return;
            }

            long now = Environment.TickCount64;
            if (interactive.ObjectType == ObjectType.None) {
                finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                finishInteractPacket.Ok = false;
                finishInteractPacket.ErrorMessage = "None인 오브젝트입니다";
                finishInteractPacket.ErrorCode = 7;
                character.Session.SendPacket(finishInteractPacket);
                interactive.interacting = false;
                character.interacting = false;
                return;
            }
            long minimumInteractTime = 2 * 1000;
            if (inputPacket.InteractType != InteractType.CatchAnimal && inputPacket.InteractType != InteractType.CatchAnimals && inputPacket.InteractType != InteractType.RemoveWeed) {
                if (now - character.startInteraction < minimumInteractTime) {
                    finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                    finishInteractPacket.ObjectId = inputPacket.ObjectId;
                    finishInteractPacket.InteractType = inputPacket.InteractType;
                    finishInteractPacket.ObjectType = inputPacket.ObjectType;
                    finishInteractPacket.Ok = false;
                    finishInteractPacket.ErrorMessage = "상호작용 시간이 너무 짧습니다";
                    finishInteractPacket.ErrorCode = 1;
                    Broadcast(character, finishInteractPacket);
                    character.Session.SendPacket(finishInteractPacket);
                    interactive.interacting = false;
                    character.interacting = false;
                    return;
                }
            }
            if (ExecuteInteraction(inputPacket.InteractType, inputPacket.ObjectType, interactive, character, inputPacket.MinigameSuccess, inputPacket.ObjectId)) {
                finishInteractPacket.CharacterDBId = inputPacket.CharacterDBId;
                finishInteractPacket.ObjectId = inputPacket.ObjectId;
                finishInteractPacket.InteractType = inputPacket.InteractType;
                finishInteractPacket.ObjectType = inputPacket.ObjectType;
                finishInteractPacket.Ok = true;
                finishInteractPacket.ErrorMessage = "Success";
                finishInteractPacket.ErrorCode = 0;
                Broadcast(finishInteractPacket);
                if (inputPacket.InteractType == InteractType.CollectCrop) {
                    CheckCanContinueGame(ObjectType.Crop);
                }
            }
            interactive.interacting = false;
            character.interacting = false;
            character.recentInteractObjectId = -2;
        }

        public void CheckCanContinueGame(ObjectType objectType) {
            if (objectType == ObjectType.SeedForPlant) {
                if (IsFieldEmpty(ObjectType.SeedForPlant)) {
                    CantContinueGame();
                }
            }
            else if (objectType == ObjectType.Weed) {
                if (IsFieldEmpty(ObjectType.Weed)) {
                    CantContinueGame();
                }
            }
            else if (objectType == ObjectType.Crop) {
                if (IsFieldEmpty(ObjectType.Crop)) {
                    CantContinueGame();
                }
            }
        }

        bool IsFieldEmpty(ObjectType objectType) {
            return GetObjectsByTypeInField(objectType).Count == 0;
        }

        void CantContinueGame() {
            if (_changeSeasonJob != null) {
                _changeSeasonJob.Cancel = true;
            }
            _changeSeasonJob = PushAfter(1000 * FinishTimeWhenAllCropHarvested, RewardCharacters);
        }

        bool ExecuteInteraction(InteractType interactType, ObjectType objectType, InteractiveObject interactive, Character character, bool minigameSuccess, int objectId) {
            S_FinishInteract finishInteractPacket = new S_FinishInteract();
            finishInteractPacket.ObjectId = objectId;
            finishInteractPacket.InteractType = interactType;
            finishInteractPacket.ObjectType = objectType;

            switch (interactType) {
                case InteractType.DestroyStone:
                    if (objectType == ObjectType.Stone) {
                        ChangeFieldObjectType(interactive, ObjectType.Ground);
                        character.destroyedStone++;
                    }
                    break;
                case InteractType.CreateField:
                    if (objectType == ObjectType.Ground) {
                        ChangeFieldObjectType(interactive, ObjectType.Field);
                        character.createdField++;
                    }
                    break;
                case InteractType.CollectSeed:
                    if (objectType == ObjectType.SeedForCollect) {
                        RemoveObject(interactive.ObjectID);
                        character.seed++;
                        character.collectedSeed++;
                    }
                    break;
                case InteractType.PlantingSeed:
                    if (objectType == ObjectType.Field) {
                        if (character.seed <= 0) {
                            finishInteractPacket.CharacterDBId = character.CharacterDBId;
                            finishInteractPacket.Ok = false;
                            finishInteractPacket.ErrorMessage = "갖고 있는 씨앗이 없습니다";
                            finishInteractPacket.ErrorCode = 2;
                            Broadcast(finishInteractPacket);
                            return false;
                        }
                        ChangeFieldObjectType(interactive, ObjectType.SeedForPlant);
                        character.seed--;
                        character.plantedSeed++;
                    }
                    break;
                case InteractType.RemoveWeed:
                    if (objectType != ObjectType.Weed) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "해당 오브젝트는 잡초가 아닙니다";
                        finishInteractPacket.ErrorCode = 6;
                        Broadcast(finishInteractPacket);
                        return false;
                    }
                    if (!minigameSuccess) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "미니게임에 실패했습니다";
                        finishInteractPacket.ErrorCode = 9;
                        Broadcast(finishInteractPacket);
                        return false;
                    }
                    ChangeFieldObjectType(interactive, ObjectType.Plant);
                    character.removedWeed++;
                    break;
                case InteractType.CatchAnimal:
                    //동물 잡기
                    if (objectType != ObjectType.Animal) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "해당 오브젝트는 동물이 아닙니다";
                        finishInteractPacket.ErrorCode = 6;
                        Broadcast(finishInteractPacket);
                        return false;
                    }

                    if (!minigameSuccess) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "미니게임에 실패했습니다";
                        finishInteractPacket.ErrorCode = 9;
                        Broadcast(finishInteractPacket);
                        return false;
                    }

                    Animal animal = interactive as Animal;
                    RemoveAnimal(animal);
                    S_DespawnObject despawnPacket = new S_DespawnObject(){
                        ObjectType = ObjectType.Animal,
                        All = false,
                    };
                    despawnPacket.ObjectIDs.Add(animal.ObjectID);
                    Broadcast(despawnPacket);
                    character.catchAnimalList.Add(animal._animalId);
                    character.catchedAnimal++;
                    break;
                case InteractType.CatchAnimals:
                    if (objectType != ObjectType.Puppet) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "해당 오브젝트는 허수아비가 아닙니다";
                        finishInteractPacket.ErrorCode = 6;
                        Broadcast(finishInteractPacket);
                        return false;
                    }
                    if (!minigameSuccess) {
                        finishInteractPacket.CharacterDBId = character.CharacterDBId;
                        finishInteractPacket.Ok = false;
                        finishInteractPacket.ErrorMessage = "미니게임에 실패했습니다";
                        finishInteractPacket.ErrorCode = 9;
                        Broadcast(finishInteractPacket);
                        return false;
                    }

                    List<Animal> animals = interactive.GetNearAnimals();
                    S_DespawnObject despawnAnimalPacket = new S_DespawnObject();
                    despawnAnimalPacket.ObjectType = ObjectType.Animal;
                    foreach (Animal anim in animals) {
                        RemoveAnimal(anim);
                        despawnAnimalPacket.ObjectIDs.Add(anim.ObjectID);
                        character.catchAnimalList.Add(anim._animalId);
                        character.catchedAnimal++;
                    }
                    Broadcast(despawnAnimalPacket);
                    break;
                case InteractType.CollectCrop:
                    if (objectType == ObjectType.Crop) {
                        ChangeFieldObjectType(interactive, ObjectType.None);
                        _harvestedCrops++;
                        character.harvestedCrops++;
                    }
                    break;
                default:
                    finishInteractPacket.CharacterDBId = character.CharacterDBId;
                    finishInteractPacket.Ok = false;
                    finishInteractPacket.ErrorMessage = "해당하는 상호작용이 존재하지 않습니다";
                    finishInteractPacket.ErrorCode = 4;
                    Broadcast(finishInteractPacket);
                    return false;
            }
            return true;
        }

        void MakeRecentObjectInteractingFalse(Character character, ObjectType objectType) {
            InteractiveObject recentObject = null;
            if (objectType == ObjectType.Animal) {
                recentObject = GetAnimal(character.recentInteractObjectId) as InteractiveObject;
            }
            else {
                recentObject = GetObject(character.recentInteractObjectId);
                if (recentObject == null) {
                    recentObject = GetObjectInField(character.recentInteractObjectId);
                }
            }
            if (recentObject != null) {
                recentObject.interacting = false;
            }
        }

        public void ChangeFieldObjectType(InteractiveObject farmObject, ObjectType objectType) {
            farmObject.ObjectType = objectType;
            farmObject.interacting = false;
            foreach (Animal animal in farmObject.GetAnimals()) {
                animal.ResetTarget();
            }
            //HP 100으로 초기화
            farmObject.ResetHp();
        }

        void SpawnAnimal(int interval, int count) {
            if (season != Season.Spring && season != Season.Autumn) {
                return;
            }
            int maxInterval = Math.Max(7, interval - 2);

            if (maxInterval == 10) {
                count = 2;
            }
            else if (maxInterval == 5) {
                count = 3;
            }

            int minCount = Math.Min(count, GetObjectsByTypeInField(ObjectType.Crop).Count + GetObjectsByTypeInField(ObjectType.SeedForPlant).Count);
            for (int i=0; i<count; i++) {
                int posX = -500;
                int posY = 0;
                int posZ = -500;
                int animalID = AddAnimal(this, posX, posY, posZ, 0.5f);
            }
            PushAfter(1000 * maxInterval, SpawnAnimal, maxInterval, count);
        }

        public void ActiveSkill(int characterDBId, int skillId, int objectId, ObjectType objectType) {
            Character character = GetCharacter(characterDBId);
            if (character == null) {
                Console.WriteLine("캐릭터가 해당 룸에 존재하지 않습니다");
                return;
            }
            S_ActiveSkill skillPacket = new S_ActiveSkill();

            if (character.skillId != skillId) {
                Console.WriteLine("해당 캐릭터가 가지고 있는 스킬이 아닙니다");
                skillPacket.SkillCharacterDBId = characterDBId;
                skillPacket.Ok = false;
                skillPacket.ErrorMessage = "해당 캐릭터가 가지고 있는 스킬이 아닙니다";
                skillPacket.ErrorCode = 1;
                character.Session.SendPacket(skillPacket);
                return;
            }

            SkillData skillData = null;
            if (DataManager.SkillDict.TryGetValue(skillId, out skillData)) {
                if (skillData.season == Season.Autumn) {
                    //가을
                    CreateScarecrow(character);
                    return;
                }
            }
            skillPacket.SkillCharacterDBId = characterDBId;
            skillPacket.Ok = false;
            skillPacket.ErrorMessage = "존재하지 않는 스킬 혹은 패시브 스킬입니다";
            skillPacket.ErrorCode = 2;
            character.Session.SendPacket(skillPacket);
        }

        void CreateScarecrow(Character character) {
            S_ActiveSkill skillPacket = new S_ActiveSkill() {
                SkillCharacterDBId = character.CharacterDBId,
            };
            List<InteractiveObject> scarecrows = GetObjectsByObjectType(ObjectType.Puppet);
            if (scarecrows.Count > 0) {
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 3;
                character.Session.SendPacket(skillPacket);
                return;
            }
            InteractiveObject scarecrow = AddObject(ObjectType.Puppet, this, character.posX, 0, character.posZ, 0.5f);
            foreach (var skill in DataManager.SkillDict.Values) {
                if (skill.season == Season.Autumn) {
                    scarecrow.maxHp = (int)skill.level[character.skillLevel[skill.id]]["scarecrow_health"];
                    scarecrow.ResetHp();
                    break;
                }
            }
            skillPacket.ObjectType = ObjectType.Puppet;
            skillPacket.ObjectId = scarecrow.ObjectID;
            skillPacket.Ok = true;
            skillPacket.ErrorCode = 0;
            skillPacket.PosX = character.posX;
            skillPacket.PosY = 0;
            skillPacket.PosZ = character.posZ;

            Broadcast(skillPacket);
        }

        void RemoveScarecrow(int objectId, Animal animal) {
            InteractiveObject scarecrow =  GetObject(objectId);
            if (scarecrow == null) {
                Console.WriteLine("존재하지 않는 오브젝트입니다");
                return;
            }
            if (scarecrow.ObjectType != ObjectType.Puppet) {
                Console.WriteLine("허수아비가 아닙니다");
                return;
            }

            RemoveObject(objectId, scarecrow);
            S_RemoveScarecrow removeScarecrow = new S_RemoveScarecrow() {
                ObjectId = objectId
            };
            Broadcast(removeScarecrow);
        }

        public override void Clear() {
            base.Clear();
            Field = null;
            _usedPositions = null;
        }
    }
}
