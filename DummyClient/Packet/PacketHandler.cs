using DummyClient;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PacketHandler {
    static bool party = true;
    static bool enterSessionGame = true;
    static bool exitParty = false;
    static bool checkOnlyInteract = false;
    static int i = 0;
    public static void S_CreateAccountHandler(PacketSession session, IPacket packet) {
        S_CreateAccount createAccountPacket = packet as S_CreateAccount;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine(createAccountPacket.Ok);
        Console.WriteLine(createAccountPacket.ErrorMessage);
    }
    public static void S_CreateCharacterHandler(PacketSession session, IPacket packet) {
        S_CreateCharacter createCharacterPacket = packet as S_CreateCharacter;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine(createCharacterPacket.Ok);
        Console.WriteLine(createCharacterPacket.ErrorMessage);

        if (createCharacterPacket.Ok == true) {
            //게임입장
        }
    }
    public static void S_LoginHandler(PacketSession session, IPacket packet) {
        S_Login loginPacket = packet as S_Login;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine($"[S_Login]: Ok {loginPacket.Ok}");
        Console.WriteLine($"[S_Login]: CharacterDBId {loginPacket.CharacterDBId}");

        serverSession.AccountID = loginPacket.AccountDBId;
        serverSession.CharacterDBId = loginPacket.CharacterDBId;

        if (loginPacket.ErrorMessage == "캐릭터가 존재하지 않습니다") {
            C_CreateCharacter createCharacterPacket = new C_CreateCharacter();
            createCharacterPacket.Nickname = "admin6";
            serverSession.SendPacket(createCharacterPacket);
        }
        else if (loginPacket.Ok == true) {
            //게임 입장
            C_EnterGame enterPacket = new C_EnterGame();
            enterPacket.RoomType = RoomType.MainIsland;
            serverSession.SendPacket(enterPacket);
        }
    }

    public static void S_LogoutHandler(PacketSession session, IPacket packet) {

    }

    public async static void S_EnterGameHandler(PacketSession session, IPacket packet) {
        S_EnterGame enterPacket = packet as S_EnterGame;
        ServerSession serverSession = session as ServerSession;

        if (serverSession.DummmyID <= 499) {
            await Task.Delay(1000);

            C_MakeParty makePartyPacket = new C_MakeParty();
            makePartyPacket.CharacterDBId = serverSession.CharacterDBId;
            serverSession.SendPacket(makePartyPacket);
        }
        else if (serverSession.DummmyID <= 499) {
            //await Task.Delay(3000);

            //C_MakeParty makePartyPacket = new C_MakeParty();
            //makePartyPacket.CharacterDBId = serverSession.CharacterDBId;
            //serverSession.SendPacket(makePartyPacket);
        }

        //else if (serverSession.DummmyID <= 300) {
        //    await Task.Delay(3000);
        //}
        //else if (serverSession.DummmyID <= 400) {
        //    await Task.Delay(4000);
        //}


        //if (serverSession.DummmyID <= 100) {

        //}
        //else if (serverSession.DummmyID <= 200) {
        //    C_Move movePacket = new C_Move();
        //    movePacket.CharacterDBId = serverSession.CharacterDBId;
        //    if (serverSession.DummmyID % 4 == 0) {
        //        movePacket.MoveVector3 = new MoveVector3() { x = 1.0f, y = 0.0f, z = 0.0f };
        //    }
        //    else if (serverSession.DummmyID % 4 == 1) {
        //        movePacket.MoveVector3 = new MoveVector3() { x = -0.71f, y = 0.0f, z = -0.71f };

        //    }
        //    else if (serverSession.DummmyID % 4 == 2) {
        //        movePacket.MoveVector3 = new MoveVector3() { x = 0.0f, y = 0.0f, z = 1.0f };

        //    }
        //    else if (serverSession.DummmyID % 4 == 3) {
        //        movePacket.MoveVector3 = new MoveVector3() { x = 0.71f, y = 0.0f, z = 0.71f };
        //    }
        //    movePacket.PosX = serverSession.posX;
        //    movePacket.PosY = serverSession.posY;
        //    movePacket.PosZ = serverSession.posZ;
        //    serverSession.SendPacket(movePacket);

        //    await Task.Delay(1000);

        //    C_MoveEnd moveEndPacket = new C_MoveEnd();
        //    moveEndPacket.CharacterDBId = serverSession.CharacterDBId;
        //    moveEndPacket.PosX = movePacket.MoveVector3.x * 8.0f;
        //    moveEndPacket.PosY = 0;
        //    moveEndPacket.PosZ = movePacket.MoveVector3.z * 8.0f;
        //    serverSession.SendPacket(moveEndPacket);
        //}
        //else if (serverSession.DummmyID <= 300) {
        //    C_Chat chatPacket = new C_Chat();
        //    chatPacket.CharacterDBId = serverSession.CharacterDBId;
        //    chatPacket.Chat = $"Im {serverSession.DummmyID}";
        //    serverSession.SendPacket(chatPacket);
        //}
        //else if (serverSession.DummmyID <= 400) {
        //    C_MakeParty makePartyPacket = new C_MakeParty();
        //    makePartyPacket.CharacterDBId = serverSession.CharacterDBId;
        //    serverSession.SendPacket(makePartyPacket);
        //}
    }
    public static void S_MoveHandler(PacketSession session, IPacket packet) {

    }

    public static void S_SpawnHandler(PacketSession session, IPacket packet) {
        S_Spawn spawnPacket = packet as S_Spawn;
        ServerSession serverSession = session as ServerSession;

        //Console.WriteLine("Other Player Spawned");
        //Console.WriteLine(spawnPacket.CharacterDBId);
        //Console.WriteLine(spawnPacket.PosX);
        //Console.WriteLine(spawnPacket.PosY);
        //Console.WriteLine(spawnPacket.PosZ);
    }

    public static void S_SpawnObjectHandler(PacketSession session, IPacket packet) {

    }

    public static void S_MoveEndHandler(PacketSession session, IPacket packet) {
        S_MoveEnd movePacket = packet as S_MoveEnd;
        ServerSession serverSession = session as ServerSession;

        serverSession.posX = movePacket.PosX;
        serverSession.posY = movePacket.PosY;
        serverSession.posZ = movePacket.PosZ;
    }

    public static void S_DespawnHandler(PacketSession session, IPacket packet) {
        S_Despawn despawnPacket = packet as S_Despawn;
        ServerSession serverSession = session as ServerSession;

        //Console.WriteLine($"PlayerID : {despawnPacket.CharacterDBId} Despawned");
    }

    public static void S_DespawnObjectHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ChatHandler(PacketSession session, IPacket packet) {

    }

    public static void S_MakePartyHandler(PacketSession session, IPacket packet) {
        S_MakeParty makePartyPacket = packet as S_MakeParty;
        ServerSession serverSession = session as ServerSession;

        serverSession.partyId = makePartyPacket.PartyId;
        C_EnterSessionGameRequest sessionPacket = new C_EnterSessionGameRequest();
        sessionPacket.CharacterDBId = serverSession.CharacterDBId;
        sessionPacket.PartyId = makePartyPacket.PartyId;
        serverSession.SendPacket(sessionPacket);
    }

    public static void S_InvitePartyHandler(PacketSession session, IPacket packet) {

    }

    public static void S_GotPartyRequestHandler(PacketSession session, IPacket packet) {
        S_GotPartyRequest gotPartyRequestPacket = packet as S_GotPartyRequest;
        ServerSession serverSession = session as ServerSession;

        C_AcceptParty acceptPartyPacket = new C_AcceptParty() {
            PartyId = gotPartyRequestPacket.PartyId,
            CharacterDBId = serverSession.CharacterDBId,
            Answer = true,
        };
        serverSession.SendPacket(acceptPartyPacket);
    }

    public static void S_AcceptPartyHandler(PacketSession session, IPacket packet) {
        S_AcceptParty acceptPartyPacket = packet as S_AcceptParty;
        ServerSession serverSession = session as ServerSession;

        serverSession.partyId = acceptPartyPacket.PartyId;
    }

    public static void S_BanishPartyHandler(PacketSession session, IPacket packet) {
        S_BanishParty banishPacket = packet as S_BanishParty;
        ServerSession serverSession = session as ServerSession;

        if (banishPacket.Ok == false) {
            Console.WriteLine(banishPacket.ErrorMessage);
        }
        else {
            if (banishPacket.BanishedCharacterDBId == serverSession.CharacterDBId) {
                Console.WriteLine($"{banishPacket.PartyId}번 파티에서 추방되셨습니다");
            }
            else {
                Console.WriteLine($"{banishPacket.BanishedCharacterDBId}님이 추방되었습니다");
                for (int i = 0; i < banishPacket.Party.Length; i++) {
                    if (banishPacket.Party[i] == null) {
                        continue;
                    }
                    Console.WriteLine($"{i}번째 파티원 아이디: {banishPacket.Party[i].CharacterDBId}");
                    Console.WriteLine($"{i}번째 파티원 닉네임: {banishPacket.Party[i].Nickname}");
                }
            }
        }
    }

    public static void S_ExitPartyHandler(PacketSession session, IPacket packet) {
        S_ExitParty exitPartyPacket = packet as S_ExitParty;
        ServerSession serverSession = session as ServerSession;

        if (party) {
            Console.WriteLine($"[S_ExitPartyHandler]: {exitPartyPacket.Ok}");
            Console.WriteLine($"[S_ExitPartyHandler]: {exitPartyPacket.ErrorMessage}");
            Console.WriteLine($"[S_ExitPartyHandler]: {exitPartyPacket.CharacterDBId}님이 탈퇴하셨습니다");
            for (int i=0; i<exitPartyPacket.Party.Length; i++) {
                if (exitPartyPacket.Party[i] == null) {
                    continue;
                }
                Console.WriteLine($"{i}번째 파티원 아이디: {exitPartyPacket.Party[i].CharacterDBId}");
                Console.WriteLine($"{i}번째 파티원 닉네임: {exitPartyPacket.Party[i].Nickname}");
            }
        }
    }

    public async static void S_EnterSessionGameRequestHandler(PacketSession session, IPacket packet) {
        S_EnterSessionGameRequest enterRequestPacket = packet as S_EnterSessionGameRequest;
        ServerSession serverSession = session as ServerSession;

        if (enterRequestPacket.Ok) {
            await Task.Delay(1000);
            C_ChooseSkill skillPacket = new C_ChooseSkill();
            skillPacket.CharacterDBId = serverSession.CharacterDBId;
            skillPacket.SkillId = SkillId.Autumn;
            serverSession.SendPacket(skillPacket);

            await Task.Delay(1000);
            C_EnterSessionGame enterPacket = new C_EnterSessionGame();
            enterPacket.CharacterDBId = serverSession.CharacterDBId;
            //TEMP
            enterPacket.PartyId = serverSession.partyId;
            enterPacket.Answer = true;
            serverSession.SendPacket(enterPacket);
        }
        else {
            Console.WriteLine(enterRequestPacket.ErrorMessage);
        }
    }

    public async static void S_EnterSessionGameHandler(PacketSession session, IPacket packet) {
        S_EnterSessionGame sessionGamePacket = packet as S_EnterSessionGame;
        ServerSession serverSession = session as ServerSession;

        if (sessionGamePacket.Ok) {
            await Task.Delay(1000);
            C_LoadingComplete loadingPacket = new C_LoadingComplete();
            loadingPacket.CharacterDBId = serverSession.CharacterDBId;
            loadingPacket.LoadingComplete = true;
            serverSession.SendPacket(loadingPacket);

            await Task.Delay(1000);
            Console.WriteLine("Enter Session Game");
        }
    }

    public static void S_StartInteractHandler(PacketSession session, IPacket packet) {
        S_StartInteract startInteractPacket = packet as S_StartInteract;
        ServerSession serverSession = session as ServerSession;

        if (startInteractPacket.Ok) {
            if (startInteractPacket.CharacterDBId != serverSession.CharacterDBId) {
                Console.WriteLine($"{startInteractPacket.CharacterDBId}님이 상호작용 중입니다");
                return;
            }
            Console.WriteLine("상호작용할 시간을 입력해주세요");
            int command = Convert.ToInt32(Console.ReadLine());
            Thread.Sleep(command);
            C_FinishInteract finishInteractPacket = new C_FinishInteract();
            finishInteractPacket.CharacterDBId = serverSession.CharacterDBId;
            finishInteractPacket.ObjectId = startInteractPacket.ObjectId;
            finishInteractPacket.ObjectType = ObjectType.Stone;
            finishInteractPacket.InteractType = InteractType.DestroyStone;
            serverSession.SendPacket(finishInteractPacket);
        }
        else {
            Console.WriteLine(startInteractPacket.ErrorMessage);
        }
    }

    public static void S_FinishInteractHandler( PacketSession session, IPacket packet) {
        S_FinishInteract finishInteractPacket = packet as S_FinishInteract;
        ServerSession serverSession = session as ServerSession;

        if (finishInteractPacket.Ok) {
            Console.WriteLine("Interact finished");
        }
        else {
            Console.WriteLine(finishInteractPacket.ErrorMessage);
        }
    }

    public static void S_TimeOverHandler(PacketSession session, IPacket packet) {
        S_TimeOver timeOverPacket = packet as S_TimeOver;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine(timeOverPacket.CharacterDBId);
        Console.WriteLine(timeOverPacket.RewardCoin);
    }

    public static void S_FinishSessionGameHandler( PacketSession session, IPacket packet) {
        Console.WriteLine("게임 종료");
    }

    public static void S_MoveRoomHandler(PacketSession session, IPacket packet) {

    }

    public async static void S_PingHandler(PacketSession session, IPacket packet) {
        ServerSession serverSession = session as ServerSession;

        C_Ping pingPacket = new C_Ping();
        serverSession.SendPacket(pingPacket);

        if (serverSession.CharacterDBId <= 0) {
            return;
        }
        C_Move movePacket = new C_Move();
        movePacket.CharacterDBId = serverSession.CharacterDBId;
        if (serverSession.DummmyID % 4 == 0) {
            movePacket.MoveVector3 = new MoveVector3() { x = 1.0f, y = 0.0f, z = 0.0f };
        }
        else if (serverSession.DummmyID % 4 == 1) {
            movePacket.MoveVector3 = new MoveVector3() { x = -0.71f, y = 0.0f, z = -0.71f };

        }
        else if (serverSession.DummmyID % 4 == 2) {
            movePacket.MoveVector3 = new MoveVector3() { x = 0.0f, y = 0.0f, z = 1.0f };

        }
        else if (serverSession.DummmyID % 4 == 3) {
            movePacket.MoveVector3 = new MoveVector3() { x = 0.71f, y = 0.0f, z = 0.71f };
        }
        movePacket.PosX = serverSession.posX;
        movePacket.PosY = serverSession.posY;
        movePacket.PosZ = serverSession.posZ;
        serverSession.SendPacket(movePacket);

        await Task.Delay(1000);

        C_MoveEnd moveEndPacket = new C_MoveEnd();
        moveEndPacket.CharacterDBId = serverSession.CharacterDBId;
        moveEndPacket.PosX = serverSession.posX + movePacket.MoveVector3.x * 8.0f;
        moveEndPacket.PosY = 0;
        moveEndPacket.PosZ = serverSession.posZ + movePacket.MoveVector3.z * 8.0f;
        serverSession.SendPacket(moveEndPacket);
    }

    public static void S_ActiveSkillHandler(PacketSession session, IPacket packet) {
        S_ActiveSkill skillPacket = packet as S_ActiveSkill;
        ServerSession serverSession = session as ServerSession;

        if (skillPacket.Ok) {
            Console.WriteLine($"{skillPacket.SkillCharacterDBId}님이 스킬을 사용했습니다");
        }
        else {
            Console.WriteLine(skillPacket.ErrorMessage);
        }
    }

    public static void S_RemoveScarecrowHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ChangeSeasonHandler(PacketSession session, IPacket packet) {
        S_ChangeSeason seasonPacket = packet as S_ChangeSeason;
        ServerSession serverSession = session as ServerSession;

        Console.WriteLine(seasonPacket.Season);
        //for (int i=0; i<seasonPacket.Objects.Count; i++) {
        //    Console.WriteLine(seasonPacket.Objects[i].ObjectType);
        //}
    }

    public static void S_AnimalMoveHandler(PacketSession session, IPacket packet) {

    }

    public static void S_AnimalMoveEndHandler(PacketSession session, IPacket packet) {

    }

    public static void S_RemoveFieldObjectByAnimalHandler(PacketSession session, IPacket packet) {

    }

    public static void S_CheckTimeHandler(PacketSession session, IPacket packet) {
        S_CheckTime timePacket = packet as S_CheckTime;
        ServerSession serverSession = session as ServerSession;

        //Console.WriteLine($"Time interval : {timePacket.Interval}");
    }

    public static void S_ChangeHpHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ExhaustHandler(PacketSession session, IPacket packet) {

    }

    public static void S_SenseSeedsHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ChangeObjectHpHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ChangeObjectHandler(PacketSession session, IPacket packet) {

    }

    public static void S_EquipItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_GetInventoryHandler(PacketSession session, IPacket packet) {

    }

    public static void S_GetItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_ThrowItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_PurchaseItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_SellItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_GachaItemHandler(PacketSession session, IPacket packet) {

    }

    public static void S_EmotionHandler(PacketSession session, IPacket packet) {

    }

    public static void S_NPCMoveHandler(PacketSession session, IPacket packet) {

    }

    public static void S_PayOffDeptHandler(PacketSession session, IPacket packet) {

    }

    public static void S_EnhanceSkillHandler(PacketSession session, IPacket packet) {
        S_EnhanceSkill skillPacket = packet as S_EnhanceSkill;

    }

    public static void S_ChooseSkillHandler(PacketSession session, IPacket packet) {

    }

    public static void S_PartyMemberReadyHandler(PacketSession session, IPacket packet) {

    }

    public static void S_GetRankingHandler(PacketSession session, IPacket packet) {
        S_GetRanking rankingPacket = packet as S_GetRanking;
        ServerSession serverSession = session as ServerSession;
    }

    public static void S_LoadingCompleteHandler(PacketSession session, IPacket packet) {
        S_LoadingComplete loadingPacket = packet as S_LoadingComplete;
        ServerSession serverSession = session as ServerSession;


    }
}