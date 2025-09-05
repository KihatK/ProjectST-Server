using Server;
using Server.DB;
using Server.Game;
using Server.Data;
using ServerCore;
using Server.Redis;

class PacketHandler {
    public static void C_CreateAccountHandler(PacketSession session, IPacket packet) {
        C_CreateAccount createUserPacket = packet as C_CreateAccount;
        ClientSession clientSession = session as ClientSession;

        if (createUserPacket.Username == null || createUserPacket.Password == null) {
            S_CreateAccount createResponsePacket = new S_CreateAccount() {
                Ok = false,
                ErrorCode = 3
            };
            clientSession.SendPacket(createResponsePacket);
            return;
        }
        if (createUserPacket.Username.Length <= 0 || createUserPacket.Username.Length > 10) {
            S_CreateAccount createResponsePacket = new S_CreateAccount() {
                Ok = false,
                ErrorCode = 3
            };
            clientSession.SendPacket(createResponsePacket);
            return;
        }
        if (createUserPacket.Password.Length <= 0 || createUserPacket.Password.Length > 20) {
            S_CreateAccount createResponsePacket = new S_CreateAccount() {
                Ok = false,
                ErrorCode = 3
            };
            clientSession.SendPacket(createResponsePacket);
            return;
        }
        DbTransaction.CreateAccount(createUserPacket.Username, createUserPacket.Password, clientSession);
    }
    public static void C_CreateCharacterHandler(PacketSession session, IPacket packet) {
        C_CreateCharacter createCharacterPacket = packet as C_CreateCharacter;
        ClientSession clientSession = session as ClientSession;

        if (createCharacterPacket.Nickname == null) {
            return;
        }
        if (createCharacterPacket.Nickname.Length <= 0 || createCharacterPacket.Nickname.Length > 10) {
            return;
        }
        DbTransaction.CreateCharacter(createCharacterPacket.Nickname, clientSession);
    }
    public static void C_LoginHandler(PacketSession session, IPacket packet) {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine(loginPacket.Username);
        Console.WriteLine(loginPacket.Password);

        LoginRoom loginRoom = GameLogic.Instance.GetRoom(RoomType.Login) as LoginRoom;
        if (loginRoom == null) {
            Console.WriteLine("Can't get LoginRoom");
            return;
        }
        if (loginPacket.Token != null) {
            
            DbTransaction.OAuthLogin(loginPacket.Token, loginPacket.LoginType, loginRoom, clientSession);
        }
        else {
            DbTransaction.LoginAccount(loginPacket.Username, loginPacket.Password, clientSession, loginRoom);
        }
    }

    public static void C_LogoutHandler(PacketSession session, IPacket packet) {
        C_Logout logoutPacket = packet as C_Logout;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter.CharacterDBId != logoutPacket.CharacterDBId) {
            Console.WriteLine("C_LogoutHandler: CharacterDBId different");
        }

        Console.WriteLine($"{logoutPacket.CharacterDBId} Logout");
        clientSession.HandleLogout();
    }

    public static void C_EnterGameHandler(PacketSession session, IPacket packet) {
        C_EnterGame enterPacket = packet as C_EnterGame;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleEnterGame(enterPacket.RoomType);
    }
    public static void C_MoveHandler(PacketSession session, IPacket packet) {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter == null) {
            return;
        }
        if (clientSession.MyCharacter.Room == null) {
            return;
        }
        if (clientSession.MyCharacter.CharacterDBId != movePacket.CharacterDBId) {
            Console.WriteLine("Character ID Different");
            return;
        }

        clientSession.MyCharacter.Room.Push(() => {
            //TODO: TEMP
            clientSession.MyCharacter.MoveVector3 = movePacket.MoveVector3;
            clientSession.MyCharacter.Direction = movePacket.Direction;

            S_Move sMovePacket = new S_Move() {
                CharacterDBId = clientSession.MyCharacter.CharacterDBId,
                Direction = movePacket.Direction,
                MoveVector3 = movePacket.MoveVector3,
            };
            clientSession.MyCharacter.Room.Broadcast(clientSession.MyCharacter, sMovePacket, 1);
        });
    }

    public static void C_MoveEndHandler(PacketSession session, IPacket packet) {
        C_MoveEnd movePacket = packet as C_MoveEnd;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter.CharacterDBId != movePacket.CharacterDBId) {
            Console.WriteLine("Character ID Different");
            return;
        }

        clientSession.MyCharacter.Room.Push(() => {
            MoveVector3 moveVector3 = new MoveVector3() {
                x = 0,
                y = 0,
                z = 0,
            };
            S_MoveEnd sMovePacket = new S_MoveEnd() {
                CharacterDBId = movePacket.CharacterDBId,
                PosX = movePacket.PosX,
                PosY = movePacket.PosY,
                PosZ = movePacket.PosZ
            };
            clientSession.MyCharacter.MoveVector3 = moveVector3;
            if (Math.Abs(movePacket.PosX - clientSession.MyCharacter.posX) < 2.0f && Math.Abs(movePacket.PosZ - clientSession.MyCharacter.posZ) < 2.0f) {
                //오차 범위 내 허용
                clientSession.MyCharacter.posX = movePacket.PosX;
                clientSession.MyCharacter.posZ = movePacket.PosZ;
            }
            else {
                //오차범위 바깥
                sMovePacket.PosX = clientSession.MyCharacter.posX;
                sMovePacket.PosY = clientSession.MyCharacter.posY;
                sMovePacket.PosZ = clientSession.MyCharacter.posZ;
                clientSession.SendPacket(sMovePacket, 1);
            }
            clientSession.MyCharacter.Room.Broadcast(clientSession.MyCharacter, sMovePacket, 1);
        });
    }

    public static void C_ChatHandler(PacketSession session, IPacket packet) {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (chatPacket.Chat != String.Empty) {
            
            clientSession.MyCharacter.Room.Push(() => {
                S_Chat sChatPacket = new S_Chat() {
                    CharacterDBId = chatPacket.CharacterDBId,
                    Chat = chatPacket.Chat
                };
                clientSession.MyCharacter.Room.Broadcast(clientSession.MyCharacter, sChatPacket);
            });
        }
    }

    public static void C_MakePartyHandler(PacketSession session, IPacket packet) {
        C_MakeParty makePartyPacket = packet as C_MakeParty;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyCharacter.Room;
        room.Push(room.MakeParty, clientSession.MyCharacter);
    }

    public static void C_InvitePartyHandler(PacketSession session, IPacket packet) {
        C_InviteParty invitePartyPacket = packet as C_InviteParty;
        ClientSession clientSession = session as ClientSession;

        //Debug
        int invitePartyPlayer = invitePartyPacket.CharacterDBId;
        int invitePartyTarget = invitePartyPacket.TargetCharacterDBId;

        GameRoom room = clientSession.MyCharacter.Room;
        room.Push(room.InviteParty, clientSession.MyCharacter, invitePartyPacket.TargetCharacterDBId, invitePartyPacket.PartyId);
    }

    public static void C_AcceptPartyHandler(PacketSession session, IPacket packet) {
        C_AcceptParty acceptPartyPacket = packet as C_AcceptParty;
        ClientSession clientSession = session as ClientSession;

        if (clientSession == null) {
            Console.WriteLine("C_AcceptPartyHandler: ClientSession isn't exist");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        // Player already have party
        if(acceptPartyPacket.PartyId < 0) {
            room.Push(room.AlreadyHaveParty, acceptPartyPacket.PartyId);
        }
        else {
            clientSession.HandleAcceptParty(acceptPartyPacket);
        }
    }

    public static void C_BanishPartyHandler(PacketSession session, IPacket packet) {
        C_BanishParty banishPacket = packet as C_BanishParty;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter.CharacterDBId != banishPacket.CharacterDBId) {
            Console.WriteLine("C_BanishPartyHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        room.Push(room.BanishParty, banishPacket.PartyId, banishPacket.CharacterDBId, banishPacket.TargetCharacterDBId);
    }

    public static void C_ExitPartyHandler(PacketSession session, IPacket packet) {
        C_ExitParty exitPartyPacket = packet as C_ExitParty;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter.CharacterDBId != exitPartyPacket.CharacterDBId) {
            Console.WriteLine("C_ExitPartyHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        room.Push(room.ExitParty, clientSession.MyCharacter, exitPartyPacket.PartyId);
    }

    public static void C_EnterSessionGameRequestHandler(PacketSession session, IPacket packet) {
        C_EnterSessionGameRequest sessionGamePacket = packet as C_EnterSessionGameRequest;
        ClientSession clientSession = session as ClientSession;

        if (sessionGamePacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_EnterSessionGameRequestHandler: CharacterDBId different");
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.MainIsland) {
            MainRoom mainRoom = room as MainRoom;
            mainRoom.Push(mainRoom.EnterSessionGameRequest, sessionGamePacket.PartyId, clientSession.MyCharacter);
        }
    }

    public static void C_EnterSessionGameHandler(PacketSession session, IPacket packet) {
        C_EnterSessionGame sessionGamePacket = packet as C_EnterSessionGame;
        ClientSession clientSession = session as ClientSession;

        if (sessionGamePacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_EnterSessionGameHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.MainIsland) {
            MainRoom mainRoom = room as MainRoom;

            mainRoom.Push(mainRoom.EnterSessionGameCheck, clientSession.MyCharacter, sessionGamePacket.PartyId, sessionGamePacket.Answer);
        }
    }

    public static void C_StartInteractHandler(PacketSession session, IPacket packet) {
        C_StartInteract interactObjectPacket = packet as C_StartInteract;
        ClientSession clientSession = session as ClientSession;

        if (interactObjectPacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_StartInteractHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;

            sessionRoom.Push(sessionRoom.StartInteract, clientSession.MyCharacter.CharacterDBId, interactObjectPacket.ObjectId, interactObjectPacket.ObjectType, interactObjectPacket.InteractType);
        }
    }

    public static void C_FinishInteractHandler( PacketSession session, IPacket packet) {
        C_FinishInteract finishInteractPacket = packet as C_FinishInteract;
        ClientSession clientSession = session as ClientSession;

        if (finishInteractPacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_FinishInteractHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;

            sessionRoom.Push(sessionRoom.FinishInteract, finishInteractPacket);
        }
    }

    public static void C_FinishSessionGameHandler(PacketSession session, IPacket packet) {
        C_FinishSessionGame finishPacket = packet as C_FinishSessionGame;
        ClientSession clientSession = session as ClientSession;

        if (finishPacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_FinishSessionGameHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;
            sessionRoom.Push(sessionRoom.FinishGame);
        }
    }

    // Need test
    public static void C_MoveRoomHandler(PacketSession session, IPacket packet) {
        C_MoveRoom moveRoomPacket = packet as C_MoveRoom;
        ClientSession clientSession = session as ClientSession;

        // Test code
        clientSession.MyCharacter.Room.Push(() => {
            S_MoveRoom sMoveRoomPacket = new S_MoveRoom() {
                CharacterDBId = moveRoomPacket.CharacterDBId,
                StartRoomType = moveRoomPacket.StartRoomType,
                EndRoomType = moveRoomPacket.EndRoomType,
            };
            clientSession.MyCharacter.Room.Broadcast(clientSession.MyCharacter, sMoveRoomPacket);
        });

    }

    public static void C_PingHandler(PacketSession session, IPacket packet) {
        ClientSession clientSession = session as ClientSession;

        clientSession.HandlePing();
    }

    public static void C_ActiveSkillHandler(PacketSession session, IPacket packet) {
        C_ActiveSkill skillPacket = packet as C_ActiveSkill;
        ClientSession clientSession = session as ClientSession;

        if (skillPacket.CharacterDBId != clientSession.MyCharacter.CharacterDBId) {
            Console.WriteLine("C_ActiveSkillHandler: CharacterDBId different");
            return;
        }
        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;
            sessionRoom.Push(sessionRoom.ActiveSkill, skillPacket.CharacterDBId, skillPacket.SkillId, skillPacket.ObjectId, skillPacket.ObjectType);
        }
    }

    public static void C_ChangeSeasonByButtonHandler(PacketSession session, IPacket packet) {
        C_ChangeSeasonByButton seasonPacket = packet as C_ChangeSeasonByButton;
        ClientSession clientSession = session as ClientSession;

        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;
            sessionRoom.Push(sessionRoom.ChangeSeasonByButton);
        }
    }

    public static void C_EquipItemHandler(PacketSession session, IPacket packet) {
        C_EquipItem equipPacket = packet as C_EquipItem;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleEquipItem(equipPacket.CharacterDBId, equipPacket.ItemDBId, equipPacket.Equip);
    }

    public static void C_GetInventoryHandler(PacketSession session, IPacket packet) {
        C_GetInventory invenPacket = packet as C_GetInventory;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.MyCharacter == null || invenPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        clientSession.GetInven();
    }

    public static void C_GetItemHandler(PacketSession session, IPacket packet) {
        C_GetItem itemPacket = packet as C_GetItem;
        ClientSession clientSession = session as ClientSession;

        if (itemPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }
        DbTransaction.GetItem(clientSession.MyCharacter, itemPacket.ItemID);
    }

    public static void C_ThrowItemHandler(PacketSession session, IPacket packet) {
        C_ThrowItem itemPacket = packet as C_ThrowItem;
        ClientSession clientSession = session as ClientSession;

        if (itemPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        DbTransaction.ThrowItem(clientSession.MyCharacter, itemPacket.ItemID, itemPacket.Count);
    }

    public static void C_PurchaseItemHandler(PacketSession session, IPacket packet) {
        C_PurchaseItem purchasePacket = packet as C_PurchaseItem;
        ClientSession clientSession = session as ClientSession;

        if (purchasePacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        DbTransaction.PurchaseItem(clientSession.MyCharacter, purchasePacket.TemplateId, DataManager.ItemDict[purchasePacket.TemplateId].price, purchasePacket.Count);
    }

    public static void C_SellItemHandler(PacketSession session, IPacket packet) {
        C_SellItem sellPacket = packet as C_SellItem;
        ClientSession clientSession = session as ClientSession;

        if (sellPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        DbTransaction.SellItem(clientSession.MyCharacter, sellPacket.ItemDBId, DataManager.ItemDict[sellPacket.TemplateId].price, sellPacket.Count);
    }

    public static void C_GachaItemHandler(PacketSession session, IPacket packet) {
        C_GachaItem gachaPacket = packet as C_GachaItem;
        ClientSession clientSession = session as ClientSession;

        if (gachaPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        clientSession.GachaItem();
    }

    public static void C_EmotionHandler(PacketSession session, IPacket packet) {
        C_Emotion emotionPacket = packet as C_Emotion;
        ClientSession clientSession = session as ClientSession;

        if (emotionPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        S_Emotion sEmotionPacket = new S_Emotion() {
            CharacterDBId = emotionPacket.CharacterDBId,
            EmotionId = emotionPacket.EmotionId,
        };
        clientSession.MyCharacter.Room.Broadcast(sEmotionPacket);
    }

    public static void C_PayOffDeptHandler(PacketSession session, IPacket packet) {
        C_PayOffDept deptPacket = packet as C_PayOffDept;
        ClientSession clientSession = session as ClientSession;

        if (deptPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        clientSession.MyCharacter.Room.Push(DbTransaction.PayOffDept, clientSession.MyCharacter, deptPacket.MoneyForDept);
    }

    public static void C_EnhanceSkillHandler(PacketSession session, IPacket packet) {
        C_EnhanceSkill skillPacket = packet as C_EnhanceSkill;
        ClientSession clientSession = session as ClientSession;

        if (skillPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        clientSession.MyCharacter.Room.Push(DbTransaction.EnhanceSkill, clientSession.MyCharacter, skillPacket.SkillId);
    }

    public static void C_ChooseSkillHandler(PacketSession session, IPacket packet) {
        C_ChooseSkill choosePacket = packet as C_ChooseSkill;
        ClientSession clientSession = session as ClientSession;

        if (choosePacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        MainRoom mainRoom = clientSession.MyCharacter.Room as MainRoom;
        if (mainRoom != null) {
            mainRoom.ChooseSkill(clientSession.MyCharacter, choosePacket.SkillId);
        }
    }

    public static void C_GetRankingHandler(PacketSession session, IPacket packet) {
        C_GetRanking rankingPacket = packet as C_GetRanking;
        ClientSession clientSession = session as ClientSession;

        if (rankingPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null) {
            return;
        }

        RedisServer.Instance.Push(RedisServer.Instance.GetRanking, clientSession.MyCharacter);
    }

    public static void C_LoadingCompleteHandler(PacketSession session, IPacket packet) {
        C_LoadingComplete loadingPacket = packet as C_LoadingComplete;
        ClientSession clientSession = session as ClientSession;

        if (loadingPacket.CharacterDBId != clientSession.CharacterDBId) {
            return;
        }
        if (clientSession.MyCharacter == null || clientSession.MyCharacter.Room == null) {
            return;
        }

        GameRoom room = clientSession.MyCharacter.Room;
        if (room.RoomType == RoomType.SessionGame) {
            SessionRoom sessionRoom = room as SessionRoom;
            sessionRoom.Push(sessionRoom.CheckPlayerReady, clientSession.MyCharacter, loadingPacket.LoadingComplete);
        }
    }
}