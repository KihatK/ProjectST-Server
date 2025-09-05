using Server.Object;

namespace Server.Game {
    class MainRoom : GameRoom {
        public MainRoom() : base(RoomType.MainIsland) {

        }

        public override void Init() {
            base.Init();
            //AddObject(5, 5, 0.5f);

            //TEST
            //AddAnimal(this, -5, 0, -5, 0.5f);
            AddNPC(this, 0, 0, 0);
        }

        public void ChooseSkill(Character character, SkillId skillId) {
            if (character == null || character.party == null) {
                return;
            }

            Character[] partyMember = GetCharactersInParty(character.party[0]);

            bool alreadychosen = false;
            if (skillId != SkillId.Default) {
                for (int i = 0; i < partyMember.Length; i++) {
                    if (partyMember[i] == null || partyMember[i].CharacterDBId == character.CharacterDBId) {
                        continue;
                    }

                    if (partyMember[i].skillId == (int)skillId) {
                        alreadychosen = true;
                        break;
                    }
                }
            }

            // 스킬 선택을 안해서 날아온 패킷
            // 선택 안된 스킬 중 맨 앞의 스킬 선택
            if (skillId == SkillId.Default) {
                bool[] selectedSkill = new bool[4];
                for (int i = 0; i < partyMember.Length; i++) {
                    if (partyMember[i] == null || partyMember[i].skillId == -1) {
                        continue;
                    }
                    int index = partyMember[i].skillId - 20000;
                    selectedSkill[index] = true;
                }

                for (int i = 0; i < 4; i++) {
                    if (selectedSkill[i]) {
                        continue;
                    }
                    skillId = (SkillId)(i + 20000);
                    break;
                }
            }


            if (alreadychosen) {
                //누군가가 이미 선택한 스킬
                //Broadcast to party
                S_ChooseSkill choosePacket = new S_ChooseSkill() {
                    Ok = false,
                    ErrorCode = 1,
                    ChosenCharacterDBId = character.CharacterDBId,
                };
                character.Session.SendPacket(choosePacket);
            }
            
            else {
                //성공
                character.skillId = (int)skillId;
                //Broadcast to party
                S_ChooseSkill choosePacket = new S_ChooseSkill() {
                    Ok = true,
                    ErrorCode = 0,
                    ChosenCharacterDBId = character.CharacterDBId,
                    SkillId = skillId,
                };
                for (int i = 0; i < partyMember.Length; i++) {
                    if (partyMember[i] == null) {
                        continue;
                    }

                    partyMember[i].Session.SendPacket(choosePacket);
                }
            }
        }

        public void EnterSessionGameRequest(int partyId, Character character) {
            if (partyId == -1 && character.party != null) {
                partyId = character.party[0];
            }
            Character[] charactersInParty = GetCharactersInParty(partyId);
            S_EnterSessionGameRequest sessionGamePacket = new S_EnterSessionGameRequest();
            if (charactersInParty == null) {
                sessionGamePacket.Ok = false;
                sessionGamePacket.ErrorMessage = "파티가 없거나 해당 파티에 속해있지 않습니다";
                sessionGamePacket.ErrorCode = 2;
                character.Session.SendPacket(sessionGamePacket);
                Push(MakeParty, character);
                Push(EnterSessionGameRequest, -1, character);
                return;
            }
            sessionGamePacket.PartyId = partyId;
            if (character.CharacterDBId != charactersInParty[0].CharacterDBId) {
                sessionGamePacket.Ok = false;
                sessionGamePacket.ErrorMessage = "파티장이 아닙니다";
                sessionGamePacket.ErrorCode = 1;
                character.Session.SendPacket(sessionGamePacket);
                return;
            }
            sessionGamePacket.Ok = true;
            sessionGamePacket.ErrorMessage = "Success";
            sessionGamePacket.ErrorCode = 0;
            for (int i = 0; i < charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                charactersInParty[i].Session.SendPacket(sessionGamePacket);
            }
        }

        public void EnterSessionGameCheck(Character character, int partyId, bool answer) {
            if (character == null) {
                return;
            }

            Character[] charactersInParty = GetCharactersInParty(partyId);
            if (charactersInParty == null) {
                Console.WriteLine("EnterSessionGameAccept()");
                return;
            }

            character.acceptEnterGame = answer;

            S_PartyMemberReady readyPacket = new S_PartyMemberReady() {
                ReadyCharacterDBId = character.CharacterDBId,
            };

            bool partyAgree = true;
            for (int i = 0; i < charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                if (charactersInParty[i].acceptEnterGame == false) {
                    partyAgree = false;
                }
            }

            for (int i=0; i<charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                readyPacket.AllMemberReady = partyAgree;
                charactersInParty[i].Session.SendPacket(readyPacket);
            }
            
            if (partyAgree) {
                PushAfter(1000 * 5, EnterSessionGame, charactersInParty, partyId);
            }
        }

        void EnterSessionGame(Character[] charactersInParty, int partyId) {
            GameRoom room = GameLogic.Instance.Add(RoomType.SessionGame);
            //해당 룸의 Init은 아직 실행 전임
            SessionRoom sessionRoom = room as SessionRoom;

            if (sessionRoom == null) {
                return;
            }
            List<int> chosenSkills = new List<int>();
            for (int i=0; i<charactersInParty.Length; i++) {
                if (charactersInParty[i] == null) {
                    continue;
                }
                RemoveCharacter(charactersInParty[i].CharacterDBId);
                S_Despawn despawnPacket = new S_Despawn {
                    CharacterDBId = charactersInParty[i].CharacterDBId,
                };
                BroadcastWithoutParty(despawnPacket, charactersInParty);
                charactersInParty[i].acceptEnterGame = false;
                chosenSkills.Add(charactersInParty[i].skillId);
            }

            RemoveParty(partyId);
            sessionRoom.Push(sessionRoom.StartGame, charactersInParty, partyId, chosenSkills);
        }
    }
}
