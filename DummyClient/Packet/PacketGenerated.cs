
using MessagePack;

public enum PacketID {
    C_CreateAccount = 0,
    S_CreateAccount = 1,
    C_CreateCharacter = 2,
    S_CreateCharacter = 3,
    C_Login = 4,
    S_Login = 5,
    C_Logout = 6,
    S_Logout = 7,
    C_EnterGame = 8,
    S_EnterGame = 9,
    C_Move = 10,
    C_MoveEnd = 11,
    S_Move = 12,
    S_MoveEnd = 13,
    S_Spawn = 14,
    S_SpawnObject = 15,
    S_Despawn = 16,
    S_DespawnObject = 17,
    C_Chat = 18,
    S_Chat = 19,
    C_MakeParty = 20,
    S_MakeParty = 21,
    C_InviteParty = 22,
    S_InviteParty = 23,
    S_GotPartyRequest = 24,
    C_AcceptParty = 25,
    S_AcceptParty = 26,
    C_BanishParty = 27,
    S_BanishParty = 28,
    C_ExitParty = 29,
    S_ExitParty = 30,
    C_EnterSessionGameRequest = 31,
    S_EnterSessionGameRequest = 32,
    C_EnterSessionGame = 33,
    S_EnterSessionGame = 34,
    C_StartInteract = 35,
    S_StartInteract = 36,
    C_FinishInteract = 37,
    S_FinishInteract = 38,
    S_TimeOver = 39,
    C_FinishSessionGame = 40,
    S_FinishSessionGame = 41,
    C_MoveRoom = 42,
    S_MoveRoom = 43,
    S_Ping = 44,
    C_Ping = 45,
    C_ActiveSkill = 46,
    S_ActiveSkill = 47,
    S_RemoveScarecrow = 48,
    S_ChangeSeason = 49,
    S_AnimalMove = 50,
    S_AnimalMoveEnd = 51,
    S_RemoveFieldObjectByAnimal = 52,
    S_CheckTime = 53,
    S_ChangeObjectHp = 54,
    S_ChangeObject = 55,
    S_ChangeHp = 56,
    S_Exhaust = 57,
    S_SenseSeeds = 58,
    C_ChangeSeasonByButton = 59,
    C_EquipItem = 60,
    S_EquipItem = 61,
    C_GetInventory = 62,
    S_GetInventory = 63,
    C_GetItem = 64,
    S_GetItem = 65,
    C_ThrowItem = 66,
    S_ThrowItem = 67,
    C_PurchaseItem = 68,
    S_PurchaseItem = 69,
    C_SellItem = 70,
    S_SellItem = 71,
    C_GachaItem = 72,
    S_GachaItem = 73,
    C_Emotion = 74,
    S_Emotion = 75,
    S_NPCMove = 76,
    C_PayOffDept = 77,
    S_PayOffDept = 78,
    C_EnhanceSkill = 79,
    S_EnhanceSkill = 80,
    C_ChooseSkill = 81,
    S_ChooseSkill = 82,
    S_PartyMemberReady = 83,
    C_GetRanking = 84,
    S_GetRanking = 85,
    C_LoadingComplete = 86,
    S_LoadingComplete = 87,
    MoveVector3 = 88,
    CharacterData = 89,
    ObjectData = 90,
    ItemInfoData = 91,
    PlayerRanking = 92,
}

[MessagePack.Union(0, typeof(C_CreateAccount))]
[MessagePack.Union(1, typeof(S_CreateAccount))]
[MessagePack.Union(2, typeof(C_CreateCharacter))]
[MessagePack.Union(3, typeof(S_CreateCharacter))]
[MessagePack.Union(4, typeof(C_Login))]
[MessagePack.Union(5, typeof(S_Login))]
[MessagePack.Union(6, typeof(C_Logout))]
[MessagePack.Union(7, typeof(S_Logout))]
[MessagePack.Union(8, typeof(C_EnterGame))]
[MessagePack.Union(9, typeof(S_EnterGame))]
[MessagePack.Union(10, typeof(C_Move))]
[MessagePack.Union(11, typeof(C_MoveEnd))]
[MessagePack.Union(12, typeof(S_Move))]
[MessagePack.Union(13, typeof(S_MoveEnd))]
[MessagePack.Union(14, typeof(S_Spawn))]
[MessagePack.Union(15, typeof(S_SpawnObject))]
[MessagePack.Union(16, typeof(S_Despawn))]
[MessagePack.Union(17, typeof(S_DespawnObject))]
[MessagePack.Union(18, typeof(C_Chat))]
[MessagePack.Union(19, typeof(S_Chat))]
[MessagePack.Union(20, typeof(C_MakeParty))]
[MessagePack.Union(21, typeof(S_MakeParty))]
[MessagePack.Union(22, typeof(C_InviteParty))]
[MessagePack.Union(23, typeof(S_InviteParty))]
[MessagePack.Union(24, typeof(S_GotPartyRequest))]
[MessagePack.Union(25, typeof(C_AcceptParty))]
[MessagePack.Union(26, typeof(S_AcceptParty))]
[MessagePack.Union(27, typeof(C_BanishParty))]
[MessagePack.Union(28, typeof(S_BanishParty))]
[MessagePack.Union(29, typeof(C_ExitParty))]
[MessagePack.Union(30, typeof(S_ExitParty))]
[MessagePack.Union(31, typeof(C_EnterSessionGameRequest))]
[MessagePack.Union(32, typeof(S_EnterSessionGameRequest))]
[MessagePack.Union(33, typeof(C_EnterSessionGame))]
[MessagePack.Union(34, typeof(S_EnterSessionGame))]
[MessagePack.Union(35, typeof(C_StartInteract))]
[MessagePack.Union(36, typeof(S_StartInteract))]
[MessagePack.Union(37, typeof(C_FinishInteract))]
[MessagePack.Union(38, typeof(S_FinishInteract))]
[MessagePack.Union(39, typeof(S_TimeOver))]
[MessagePack.Union(40, typeof(C_FinishSessionGame))]
[MessagePack.Union(41, typeof(S_FinishSessionGame))]
[MessagePack.Union(42, typeof(C_MoveRoom))]
[MessagePack.Union(43, typeof(S_MoveRoom))]
[MessagePack.Union(44, typeof(S_Ping))]
[MessagePack.Union(45, typeof(C_Ping))]
[MessagePack.Union(46, typeof(C_ActiveSkill))]
[MessagePack.Union(47, typeof(S_ActiveSkill))]
[MessagePack.Union(48, typeof(S_RemoveScarecrow))]
[MessagePack.Union(49, typeof(S_ChangeSeason))]
[MessagePack.Union(50, typeof(S_AnimalMove))]
[MessagePack.Union(51, typeof(S_AnimalMoveEnd))]
[MessagePack.Union(52, typeof(S_RemoveFieldObjectByAnimal))]
[MessagePack.Union(53, typeof(S_CheckTime))]
[MessagePack.Union(54, typeof(S_ChangeObjectHp))]
[MessagePack.Union(55, typeof(S_ChangeObject))]
[MessagePack.Union(56, typeof(S_ChangeHp))]
[MessagePack.Union(57, typeof(S_Exhaust))]
[MessagePack.Union(58, typeof(S_SenseSeeds))]
[MessagePack.Union(59, typeof(C_ChangeSeasonByButton))]
[MessagePack.Union(60, typeof(C_EquipItem))]
[MessagePack.Union(61, typeof(S_EquipItem))]
[MessagePack.Union(62, typeof(C_GetInventory))]
[MessagePack.Union(63, typeof(S_GetInventory))]
[MessagePack.Union(64, typeof(C_GetItem))]
[MessagePack.Union(65, typeof(S_GetItem))]
[MessagePack.Union(66, typeof(C_ThrowItem))]
[MessagePack.Union(67, typeof(S_ThrowItem))]
[MessagePack.Union(68, typeof(C_PurchaseItem))]
[MessagePack.Union(69, typeof(S_PurchaseItem))]
[MessagePack.Union(70, typeof(C_SellItem))]
[MessagePack.Union(71, typeof(S_SellItem))]
[MessagePack.Union(72, typeof(C_GachaItem))]
[MessagePack.Union(73, typeof(S_GachaItem))]
[MessagePack.Union(74, typeof(C_Emotion))]
[MessagePack.Union(75, typeof(S_Emotion))]
[MessagePack.Union(76, typeof(S_NPCMove))]
[MessagePack.Union(77, typeof(C_PayOffDept))]
[MessagePack.Union(78, typeof(S_PayOffDept))]
[MessagePack.Union(79, typeof(C_EnhanceSkill))]
[MessagePack.Union(80, typeof(S_EnhanceSkill))]
[MessagePack.Union(81, typeof(C_ChooseSkill))]
[MessagePack.Union(82, typeof(S_ChooseSkill))]
[MessagePack.Union(83, typeof(S_PartyMemberReady))]
[MessagePack.Union(84, typeof(C_GetRanking))]
[MessagePack.Union(85, typeof(S_GetRanking))]
[MessagePack.Union(86, typeof(C_LoadingComplete))]
[MessagePack.Union(87, typeof(S_LoadingComplete))]
[MessagePack.Union(88, typeof(MoveVector3))]
[MessagePack.Union(89, typeof(CharacterData))]
[MessagePack.Union(90, typeof(ObjectData))]
[MessagePack.Union(91, typeof(ItemInfoData))]
[MessagePack.Union(92, typeof(PlayerRanking))]
public interface IPacket {

}

[MessagePackObject]
public class C_CreateAccount : IPacket {
    [Key(0)]
    public string Username { get; set; }

    [Key(1)]
    public string Password { get; set; }

}

[MessagePackObject]
public class S_CreateAccount : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public string ErrorMessage { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_CreateCharacter : IPacket {
    [Key(0)]
    public string Nickname { get; set; }

}

[MessagePackObject]
public class S_CreateCharacter : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public string ErrorMessage { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_Login : IPacket {
    [Key(0)]
    public string Username { get; set; }

    [Key(1)]
    public string Password { get; set; }

    [Key(2)]
    public string Token { get; set; }

    [Key(3)]
    public LoginType LoginType { get; set; }

}

[MessagePackObject]
public class S_Login : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public string ErrorMessage { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

    [Key(3)]
    public int AccountDBId { get; set; }

    [Key(4)]
    public int CharacterDBId { get; set; }

    [Key(5)]
    public string Nickname { get; set; }

    [Key(6)]
    public int Money { get; set; }

    [Key(7)]
    public int Dept { get; set; }

    [Key(8)]
    public string Location { get; set; }

    [Key(9)]
    public int FarmCoin { get; set; }

}

[MessagePackObject]
public class C_Logout : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_Logout : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public bool Ok { get; set; }

    [Key(2)]
    public string ErrorMessage { get; set; }

    [Key(3)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_EnterGame : IPacket {
    [Key(0)]
    public RoomType RoomType { get; set; }

}

[MessagePackObject]
public class S_EnterGame : IPacket {
    [Key(0)]
    public RoomType RoomType { get; set; }

    [Key(1)]
    public int CharacterDBId { get; set; }

    [Key(2)]
    public float PosX { get; set; }

    [Key(3)]
    public float PosY { get; set; }

    [Key(4)]
    public float PosZ { get; set; }

    [Key(5)]
    public PlayerDirection Direction { get; set; }

    [Key(6)]
    public Dictionary<int, int> SkillLevel { get; set; } = new Dictionary<int, int>();

}

[MessagePackObject]
public class C_Move : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public MoveVector3 MoveVector3 { get; set; }

    [Key(2)]
    public PlayerDirection Direction { get; set; }

    [Key(3)]
    public float PosX { get; set; }

    [Key(4)]
    public float PosY { get; set; }

    [Key(5)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class C_MoveEnd : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public float PosX { get; set; }

    [Key(2)]
    public float PosY { get; set; }

    [Key(3)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_Move : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public MoveVector3 MoveVector3 { get; set; }

    [Key(2)]
    public PlayerDirection Direction { get; set; }

    [Key(3)]
    public float PosX { get; set; }

    [Key(4)]
    public float PosY { get; set; }

    [Key(5)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_MoveEnd : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public float PosX { get; set; }

    [Key(2)]
    public float PosY { get; set; }

    [Key(3)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_Spawn : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public string Nickname { get; set; }

    [Key(2)]
    public float PosX { get; set; }

    [Key(3)]
    public float PosY { get; set; }

    [Key(4)]
    public float PosZ { get; set; }

    [Key(5)]
    public List<ItemInfoData> EquipItemInfoData { get; set; } = new List<ItemInfoData>();

}

[MessagePackObject]
public class S_SpawnObject : IPacket {
    [Key(0)]
    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();

}

[MessagePackObject]
public class S_Despawn : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_DespawnObject : IPacket {
    [Key(0)]
    public ObjectType ObjectType { get; set; }

    [Key(1)]
    public bool All { get; set; } = false;

    [Key(2)]
    public List<int> ObjectIDs { get; set; } = new List<int>();

}

[MessagePackObject]
public class C_Chat : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public string Chat { get; set; }

}

[MessagePackObject]
public class S_Chat : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public string Chat { get; set; }

}

[MessagePackObject]
public class C_MakeParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_MakeParty : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public string ErrorMessage { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

    [Key(3)]
    public int PartyId { get; set; }

}

[MessagePackObject]
public class C_InviteParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int TargetCharacterDBId { get; set; }

    [Key(2)]
    public int PartyId { get; set; }

}

[MessagePackObject]
public class S_InviteParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int TargetCharacterDBId { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class S_GotPartyRequest : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int TargetCharacterDBId { get; set; }

    [Key(2)]
    public int PartyId { get; set; }

}

[MessagePackObject]
public class C_AcceptParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int PartyId { get; set; }

    [Key(2)]
    public bool Answer { get; set; }

}

[MessagePackObject]
public class S_AcceptParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public bool Ok { get; set; }

    [Key(2)]
    public string ErrorMessage { get; set; }

    [Key(3)]
    public int ErrorCode { get; set; }

    [Key(4)]
    public int PartyId { get; set; }

    [Key(5)]
    public CharacterData[] Party { get; set; }

}

[MessagePackObject]
public class C_BanishParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int TargetCharacterDBId { get; set; }

    [Key(2)]
    public int PartyId { get; set; }

}

[MessagePackObject]
public class S_BanishParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int BanishedCharacterDBId { get; set; }

    [Key(2)]
    public int PartyId { get; set; }

    [Key(3)]
    public CharacterData[] Party { get; set; }

    [Key(4)]
    public bool Ok { get; set; }

    [Key(5)]
    public string ErrorMessage { get; set; }

    [Key(6)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_ExitParty : IPacket {
    [Key(0)]
    public int PartyId { get; set; }

    [Key(1)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_ExitParty : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ExitCharacterDBId { get; set; }

    [Key(2)]
    public CharacterData[] Party { get; set; }

    [Key(3)]
    public bool Ok { get; set; }

    [Key(4)]
    public string ErrorMessage { get; set; }

    [Key(5)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_EnterSessionGameRequest : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int PartyId { get; set; }

}

[MessagePackObject]
public class S_EnterSessionGameRequest : IPacket {
    [Key(0)]
    public int PartyId { get; set; }

    [Key(1)]
    public bool Ok { get; set; }

    [Key(2)]
    public string ErrorMessage { get; set; }

    [Key(3)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_EnterSessionGame : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int PartyId { get; set; }

    [Key(2)]
    public bool Answer { get; set; }

}

[MessagePackObject]
public class S_EnterSessionGame : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public int PartyId { get; set; }

    [Key(2)]
    public string ErrorMessage { get; set; }

    [Key(3)]
    public int ErrorCode { get; set; }

    [Key(4)]
    public CharacterData[] Party { get; set; }

    [Key(5)]
    public List<ObjectData> Objects { get; set; }

}

[MessagePackObject]
public class C_StartInteract : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public ObjectType ObjectType { get; set; }

    [Key(3)]
    public InteractType InteractType { get; set; }

}

[MessagePackObject]
public class S_StartInteract : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public ObjectType ObjectType { get; set; }

    [Key(3)]
    public InteractType InteractType { get; set; }

    [Key(4)]
    public bool Ok { get; set; }

    [Key(5)]
    public string ErrorMessage { get; set; }

    [Key(6)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_FinishInteract : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public ObjectType ObjectType { get; set; }

    [Key(3)]
    public InteractType InteractType { get; set; }

    [Key(4)]
    public bool MinigameSuccess { get; set; }

}

[MessagePackObject]
public class S_FinishInteract : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public ObjectType ObjectType { get; set; }

    [Key(3)]
    public InteractType InteractType { get; set; }

    [Key(4)]
    public bool Ok { get; set; }

    [Key(5)]
    public string ErrorMessage { get; set; }

    [Key(6)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class S_TimeOver : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int RewardCoin { get; set; }

    [Key(2)]
    public int DestroyedStone { get; set; }

    [Key(3)]
    public int CreatedField { get; set; }

    [Key(4)]
    public int CollectedSeed { get; set; }

    [Key(5)]
    public int PlantedSeed { get; set; }

    [Key(6)]
    public int RemovedWeed { get; set; }

    [Key(7)]
    public int CatchedAnimal { get; set; }

    [Key(8)]
    public int HarvestedCrops { get; set; }

}

[MessagePackObject]
public class C_FinishSessionGame : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_FinishSessionGame : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class C_MoveRoom : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public RoomType StartRoomType { get; set; }

    [Key(2)]
    public RoomType EndRoomType { get; set; }

}

[MessagePackObject]
public class S_MoveRoom : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public RoomType StartRoomType { get; set; }

    [Key(2)]
    public RoomType EndRoomType { get; set; }

}

[MessagePackObject]
public class S_Ping : IPacket {
}

[MessagePackObject]
public class C_Ping : IPacket {
}

[MessagePackObject]
public class C_ActiveSkill : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int SkillId { get; set; }

    [Key(2)]
    public int ObjectId { get; set; }

    [Key(3)]
    public ObjectType ObjectType { get; set; }

}

[MessagePackObject]
public class S_ActiveSkill : IPacket {
    [Key(0)]
    public int SkillCharacterDBId { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public ObjectType ObjectType { get; set; }

    [Key(3)]
    public bool Ok { get; set; }

    [Key(4)]
    public string ErrorMessage { get; set; }

    [Key(5)]
    public int ErrorCode { get; set; }

    [Key(6)]
    public float PosX { get; set; }

    [Key(7)]
    public float PosY { get; set; }

    [Key(8)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_RemoveScarecrow : IPacket {
    [Key(0)]
    public int ObjectId { get; set; }

}

[MessagePackObject]
public class S_ChangeSeason : IPacket {
    [Key(0)]
    public Season Season { get; set; }

    [Key(1)]
    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();

}

[MessagePackObject]
public class S_AnimalMove : IPacket {
    [Key(0)]
    public int AnimalId { get; set; }

    [Key(1)]
    public PlayerDirection Direction { get; set; }

    [Key(2)]
    public float PosX { get; set; }

    [Key(3)]
    public float PosY { get; set; }

    [Key(4)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_AnimalMoveEnd : IPacket {
    [Key(0)]
    public int AnimalId { get; set; }

    [Key(1)]
    public float PosX { get; set; }

    [Key(2)]
    public float PosY { get; set; }

    [Key(3)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class S_RemoveFieldObjectByAnimal : IPacket {
    [Key(0)]
    public int ObjectId { get; set; }

}

[MessagePackObject]
public class S_CheckTime : IPacket {
    [Key(0)]
    public int Interval { get; set; }

    [Key(1)]
    public int TickAmount { get; set; }

}

[MessagePackObject]
public class S_ChangeObjectHp : IPacket {
    [Key(0)]
    public ObjectType ObjectType { get; set; }

    [Key(1)]
    public int ObjectId { get; set; }

    [Key(2)]
    public int Hp { get; set; }

}

[MessagePackObject]
public class S_ChangeObject : IPacket {
    [Key(0)]
    public List<int> ObjectIds { get; set; } = new List<int>();

    [Key(1)]
    public ObjectType From { get; set; }

    [Key(2)]
    public ObjectType To { get; set; }

}

[MessagePackObject]
public class S_ChangeHp : IPacket {
    [Key(0)]
    public int TargetCharacterDBId { get; set; }

    [Key(1)]
    public int Hp { get; set; }

}

[MessagePackObject]
public class S_Exhaust : IPacket {
    [Key(0)]
    public int TargetCharacterDBId { get; set; }

    [Key(1)]
    public bool Exhaust { get; set; }

}

[MessagePackObject]
public class S_SenseSeeds : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public List<ObjectData> Seeds { get; set; } = new List<ObjectData>();

}

[MessagePackObject]
public class C_ChangeSeasonByButton : IPacket {
}

[MessagePackObject]
public class C_EquipItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ItemDBId { get; set; }

    [Key(2)]
    public bool Equip { get; set; }

}

[MessagePackObject]
public class S_EquipItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ItemDBId { get; set; }

    [Key(2)]
    public int ItemTemplateId { get; set; }

    [Key(3)]
    public bool Equip { get; set; }

}

[MessagePackObject]
public class C_GetInventory : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_GetInventory : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public List<ItemInfoData> Inventory { get; set; } = new List<ItemInfoData>();

}

[MessagePackObject]
public class C_GetItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public ItemType ItemType { get; set; }

    [Key(2)]
    public int ItemID { get; set; }

}

[MessagePackObject]
public class S_GetItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public bool Ok { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

    [Key(3)]
    public ItemInfoData ItemInfoData { get; set; }

}

[MessagePackObject]
public class C_ThrowItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ItemID { get; set; }

    [Key(2)]
    public int Count { get; set; }

}

[MessagePackObject]
public class S_ThrowItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public bool Ok { get; set; }

    [Key(2)]
    public int ErrorCode { get; set; }

    [Key(3)]
    public ItemInfoData ItemInfoData { get; set; }

}

[MessagePackObject]
public class C_PurchaseItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int TemplateId { get; set; }

    [Key(2)]
    public int Count { get; set; }

}

[MessagePackObject]
public class S_PurchaseItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public ItemInfoData ItemInfoData { get; set; }

    [Key(2)]
    public int Money { get; set; }

    [Key(3)]
    public bool Ok { get; set; }

    [Key(4)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_SellItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int ItemDBId { get; set; }

    [Key(2)]
    public int TemplateId { get; set; }

    [Key(3)]
    public int Count { get; set; }

}

[MessagePackObject]
public class S_SellItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public ItemInfoData ItemInfoData { get; set; }

    [Key(2)]
    public int Money { get; set; }

    [Key(3)]
    public bool Ok { get; set; }

    [Key(4)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_GachaItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_GachaItem : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public ItemInfoData ItemInfoData { get; set; }

    [Key(2)]
    public int Money { get; set; }

    [Key(3)]
    public bool Ok { get; set; }

    [Key(4)]
    public int ErrorCode { get; set; }

}

[MessagePackObject]
public class C_Emotion : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int EmotionId { get; set; }

}

[MessagePackObject]
public class S_Emotion : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int EmotionId { get; set; }

}

[MessagePackObject]
public class S_NPCMove : IPacket {
    [Key(0)]
    public int ObjectId { get; set; }

    [Key(1)]
    public float PosX { get; set; }

    [Key(2)]
    public float PosY { get; set; }

    [Key(3)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class C_PayOffDept : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public int MoneyForDept { get; set; }

}

[MessagePackObject]
public class S_PayOffDept : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public int ErrorCode { get; set; }

    [Key(2)]
    public int CharacterDBId { get; set; }

    [Key(3)]
    public int Money { get; set; }

    [Key(4)]
    public int Dept { get; set; }

}

[MessagePackObject]
public class C_EnhanceSkill : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public SkillId SkillId { get; set; }

}

[MessagePackObject]
public class S_EnhanceSkill : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public int ErrorCode { get; set; }

    [Key(2)]
    public int CharacterDBId { get; set; }

    [Key(3)]
    public int Money { get; set; }

    [Key(4)]
    public SkillId SkillId { get; set; }

    [Key(5)]
    public int SkillLevel { get; set; }

}

[MessagePackObject]
public class C_ChooseSkill : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public SkillId SkillId { get; set; }

}

[MessagePackObject]
public class S_ChooseSkill : IPacket {
    [Key(0)]
    public bool Ok { get; set; }

    [Key(1)]
    public int ErrorCode { get; set; }

    [Key(2)]
    public int ChosenCharacterDBId { get; set; }

    [Key(3)]
    public SkillId SkillId { get; set; }

}

[MessagePackObject]
public class S_PartyMemberReady : IPacket {
    [Key(0)]
    public int ReadyCharacterDBId { get; set; }

    [Key(1)]
    public bool AllMemberReady { get; set; }

}

[MessagePackObject]
public class C_GetRanking : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

}

[MessagePackObject]
public class S_GetRanking : IPacket {
    [Key(0)]
    public List<PlayerRanking> PlayerRankings { get; set; } = new List<PlayerRanking>();

    [Key(1)]
    public int MyCharacterDBId { get; set; }

    [Key(2)]
    public int MyRanking { get; set; }

    [Key(3)]
    public int MyMoney { get; set; }

    [Key(4)]
    public int MinutesUntilUpdate { get; set; }

}

[MessagePackObject]
public class C_LoadingComplete : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public bool LoadingComplete { get; set; }

}

[MessagePackObject]
public class S_LoadingComplete : IPacket {
}

[MessagePackObject]
public class MoveVector3 : IPacket {
    [Key(0)]
    public float x { get; set; }

    [Key(1)]
    public float y { get; set; }

    [Key(2)]
    public float z { get; set; }

}

[MessagePackObject]
public class CharacterData : IPacket {
    [Key(0)]
    public int CharacterDBId { get; set; }

    [Key(1)]
    public float PosX { get; set; }

    [Key(2)]
    public float PosY { get; set; }

    [Key(3)]
    public float PosZ { get; set; }

    [Key(4)]
    public string Nickname { get; set; }

    [Key(5)]
    public int SkillId { get; set; }

    [Key(6)]
    public int SkillLevel { get; set; }

}

[MessagePackObject]
public class ObjectData : IPacket {
    [Key(0)]
    public int ObjectId { get; set; }

    [Key(1)]
    public ObjectType ObjectType { get; set; }

    [Key(2)]
    public float PosX { get; set; }

    [Key(3)]
    public float PosY { get; set; }

    [Key(4)]
    public float PosZ { get; set; }

}

[MessagePackObject]
public class ItemInfoData : IPacket {
    [Key(0)]
    public int ItemDBId { get; set; }

    [Key(1)]
    public ItemType ItemType { get; set; }

    [Key(2)]
    public int TemplateId { get; set; }

    [Key(3)]
    public int Count { get; set; }

    [Key(4)]
    public int Slot { get; set; }

    [Key(5)]
    public bool Equip { get; set; }

}

[MessagePackObject]
public class PlayerRanking : IPacket {
    [Key(0)]
    public string Nickname { get; set; }

    [Key(1)]
    public int Ranking { get; set; }

    [Key(2)]
    public int Money { get; set; }

}


public enum PlayerDirection {
    East,
    South,
    West,
    North,
    Northeast,
    Southeast,
    Southwest,
    Northwest,
}
public enum PlayerState {
    Idle,
    Moving,
}
public enum RoomType {
    Login,
    MainIsland,
    ContentIsland,
    SessionGame,
}
public enum ItemType {
    //농사 시 필요한 손 슬롯, 상호작용 시에만 클라이언트에서 사용.
    Hand,
    //치장을 위한 아이템 슬롯
    Bag,
    Bottom,
    Handwear,
    Earing,
    Eyewear,
    Headgear,
    Shoes,
    Top,
    //신체 부위 변경을 위한 슬롯
    Eye,
    Eyebrow,
    Hair,
    Lips,
    Mustache,
    
    //TODO:기존 코드, 삭제하면 오류 발생해서 냅둠. 지워주세요.
    Helmet,
    Armor,
    Head,
}
public enum ObjectType {
    //벽이나 충돌하는 Non-interaction 오브젝트
    Obstacle,
    //돌
    Stone,
    //허수아비
    Puppet,
    //새와 같은 동물
    Animal,
    //땅 중에서도 밭을 만들 수 있는 땅
    Ground,
    //밭
    Field,
    //주워야하는 씨앗
    SeedForCollect,
    //심어진 씨앗
    SeedForPlant,
    //씨앗에서 여름으로 넘어가 조금 성장한 작물
    Plant,
    //Plant 상태에서 잡초가 있는 상태
    Weed,
    //Plant 상태에서 가을로 넘어가 완전한 농작물
    Crop,
    //Crop을 제출할 창고나 트럭 등
    Storage,
    //밭 중앙에 있는 모닥불
    Fire,
    //작물을 수확한 후 빈 밭을 의미함
    None
}
public enum InteractType {
    //밭 갈기
    CreateField,
    //씨앗 뿌리기
    PlantingSeed,
    //씨앗 줍기
    CollectSeed,
    //농작물 재배
    CollectCrop,
    //농작물 Storage에 제출
    SubmitCrop,
    //Bird와 같은 동물 잡기
    CatchAnimal,
    //돌 부수기
    DestroyStone,
    //잡초 제거
    RemoveWeed,
    //허수아비 주변 동물 잡기
    CatchAnimals,
    //허수아비 설치
    SummonPuppet,
    //재화 얻기 - 결과창용
    GetRewardCoin,
    //임시
    Default
}
public enum Season {
    Winter,
    Spring,
    Summer,
    Autumn
}
public enum SkillId {
    Winter = 20000,
    Spring = 20001,
    Summer = 20002,
    Autumn = 20003,
    Default = 20004,
};
public enum LoginType {
    Local,
    Google,
    FaceBook,
    Naver,
    Stove,
}
