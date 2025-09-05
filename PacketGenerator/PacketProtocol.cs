public class C_CreateAccount {
    public string Username { get; set; }
    public string Password { get; set; }
}

public class S_CreateAccount {
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_CreateCharacter {
    public string Nickname { get; set; }
}

public class S_CreateCharacter {
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_Login {
    public string Username { get; set; }
    public string Password { get; set; }
    public string Token { get; set; }
    public LoginType LoginType { get; set; }
}

public class S_Login {
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public int AccountDBId { get; set; }
    public int CharacterDBId { get; set; }
    public string Nickname { get; set; }
    public int Money { get; set; }
    public int Dept { get; set; }
    public string Location { get; set; }
    public int FarmCoin { get; set; }
}

public class C_Logout {
    public int CharacterDBId { get; set; }
}

public class S_Logout {
    public int CharacterDBId { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_EnterGame {
    public RoomType RoomType { get; set; }
}

public class S_EnterGame {
    public RoomType RoomType { get; set; }
    public int CharacterDBId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public PlayerDirection Direction { get; set; }
    public Dictionary<int, int> SkillLevel { get; set; } = new Dictionary<int, int>();
}

public class C_Move {
    public int CharacterDBId { get; set; }
    public MoveVector3 MoveVector3 { get; set; }
    public PlayerDirection Direction { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class C_MoveEnd {
    public int CharacterDBId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_Move {
    public int CharacterDBId { get; set; }
    public MoveVector3 MoveVector3 { get; set; }
    public PlayerDirection Direction { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_MoveEnd {
    public int CharacterDBId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_Spawn {
    public int CharacterDBId { get; set; }
    public string Nickname { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public List<ItemInfoData> EquipItemInfoData { get; set; } = new List<ItemInfoData>();
}


public class S_SpawnObject {
    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
}


public class S_Despawn {
    public int CharacterDBId { get; set; }
}

public class S_DespawnObject {
    public ObjectType ObjectType { get; set; }
    public bool All { get; set; } = false;
    public List<int> ObjectIDs { get; set; } = new List<int>();
}

public class C_Chat {
    public int CharacterDBId { get; set; }
    public string Chat { get; set; }
}

public class S_Chat {
    public int CharacterDBId { get; set; }
    public string Chat { get; set; }
}

public class C_MakeParty {
    public int CharacterDBId { get; set; }
}

public class S_MakeParty {
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public int PartyId { get; set; }
}

public class C_InviteParty {
    public int CharacterDBId { get; set; }
    public int TargetCharacterDBId { get; set; }
    public int PartyId { get; set; }
}

public class S_InviteParty {
    public int CharacterDBId { get; set; }
    public int TargetCharacterDBId { get; set; }
    public int ErrorCode { get; set; }
}

public class S_GotPartyRequest {
    public int CharacterDBId { get; set; }
    public int TargetCharacterDBId { get; set; }
    public int PartyId { get; set; }
}

public class C_AcceptParty {
    public int CharacterDBId { get; set; }
    public int PartyId { get; set; }
    public bool Answer { get; set; }
}

public class S_AcceptParty {
    public int CharacterDBId { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public int PartyId { get; set; }
    public CharacterData[] Party { get; set; }
}

public class C_BanishParty {
    public int CharacterDBId { get; set; }
    public int TargetCharacterDBId { get; set; }
    public int PartyId { get; set; }
}

public class S_BanishParty {
    public int CharacterDBId { get; set; }
    public int BanishedCharacterDBId { get; set; }
    public int PartyId { get; set; }
    public CharacterData[] Party { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_ExitParty {
    public int PartyId { get; set; }
    public int CharacterDBId { get; set; }
}

public class S_ExitParty {
    public int CharacterDBId { get; set; }
    public int ExitCharacterDBId { get; set; }
    public CharacterData[] Party { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_EnterSessionGameRequest {
    public int CharacterDBId { get; set; }
    public int PartyId { get; set; }
}

public class S_EnterSessionGameRequest {
    public int PartyId { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_EnterSessionGame {
    public int CharacterDBId { get; set; }
    public int PartyId { get; set; }
    public bool Answer { get; set; }
}

public class S_EnterSessionGame {
    public bool Ok { get; set; }
    public int PartyId { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public CharacterData[] Party { get; set; }
    public List<ObjectData> Objects { get; set; }
}

public class C_StartInteract {
    public int CharacterDBId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public InteractType InteractType { get; set; }
}

public class S_StartInteract {
    public int CharacterDBId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public InteractType InteractType { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class C_FinishInteract {
    public int CharacterDBId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public InteractType InteractType { get; set; }
    public bool MinigameSuccess { get; set; }
}

public class S_FinishInteract {
    public int CharacterDBId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public InteractType InteractType { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
}

public class S_TimeOver {
    public int CharacterDBId { get; set; }
    public int RewardCoin { get; set; }
    public int DestroyedStone { get; set; }
    public int CreatedField { get; set; }
    public int CollectedSeed { get; set; }
    public int PlantedSeed { get; set; }
    public int RemovedWeed { get; set; }
    public int CatchedAnimal { get; set; }
    public int HarvestedCrops { get; set; }
}

public class C_FinishSessionGame {
    public int CharacterDBId { get; set; }
}

public class S_FinishSessionGame {
    public int CharacterDBId { get; set; }
}

public class C_MoveRoom {
    public int CharacterDBId { get; set; }
    public RoomType StartRoomType { get; set; }
    public RoomType EndRoomType { get; set; }
}

public class S_MoveRoom {
    public int CharacterDBId { get; set; }
    public RoomType StartRoomType { get; set; }
    public RoomType EndRoomType { get; set; }
}

public class S_Ping {

}

public class C_Ping {

}

public class C_ActiveSkill {
    public int CharacterDBId { get; set; }
    public int SkillId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
}

public class S_ActiveSkill {
    public int SkillCharacterDBId { get; set; }
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public bool Ok { get; set; }
    public string ErrorMessage { get; set; }
    public int ErrorCode { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_RemoveScarecrow {
    public int ObjectId { get; set; }
}

public class S_ChangeSeason {
    public Season Season { get; set; }
    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
}

public class S_AnimalMove {
    public int AnimalId { get; set; }
    public PlayerDirection Direction { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_AnimalMoveEnd {
    public int AnimalId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class S_RemoveFieldObjectByAnimal {
    public int ObjectId { get; set; }
}

public class S_CheckTime {
    public int Interval { get; set; }
    public int TickAmount { get; set; }
}

public class S_ChangeObjectHp {
    public ObjectType ObjectType { get; set; }
    public int ObjectId { get; set; }
    public int Hp { get; set; }
}

public class S_ChangeObject {
    public List<int> ObjectIds { get; set; } = new List<int>();
    public ObjectType From { get; set; }
    public ObjectType To { get; set; }
}

public class S_ChangeHp {
    public int TargetCharacterDBId { get; set; }
    public int Hp { get; set; }
}

public class S_Exhaust {
    public int TargetCharacterDBId { get; set; }
    public bool Exhaust { get; set; }
}

public class S_SenseSeeds {
    public int CharacterDBId { get; set; }
    public List<ObjectData> Seeds { get; set; } = new List<ObjectData>();
}

public class C_ChangeSeasonByButton {

}

public class C_EquipItem {
    public int CharacterDBId { get; set; }
    public int ItemDBId { get; set; }
    public bool Equip { get; set; }
}

public class S_EquipItem {
    public int CharacterDBId { get; set; }
    public int ItemDBId { get; set; }
    public int ItemTemplateId { get; set; }
    public bool Equip { get; set; }
}

public class C_GetInventory {
    public int CharacterDBId { get; set; }
}

public class S_GetInventory {
    public int CharacterDBId { get; set; }
    public List<ItemInfoData> Inventory { get; set; } = new List<ItemInfoData>();
}

public class C_GetItem {
    public int CharacterDBId { get; set; }
    public ItemType ItemType { get; set; }
    public int ItemID { get; set; }
}

public class S_GetItem {
    public int CharacterDBId { get; set; }
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public ItemInfoData ItemInfoData { get; set; }
}

public class C_ThrowItem {
    public int CharacterDBId { get; set; }
    public int ItemID { get; set; }
    public int Count { get; set; }
}

public class S_ThrowItem {
    public int CharacterDBId { get; set; }
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public ItemInfoData ItemInfoData { get; set; }
}

public class C_PurchaseItem {
    public int CharacterDBId { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
}

public class S_PurchaseItem {
    public int CharacterDBId { get; set; }
    public ItemInfoData ItemInfoData { get; set; }
    public int Money { get; set; }
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
}

public class C_SellItem {
    public int CharacterDBId { get; set; }
    public int ItemDBId { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
}

public class S_SellItem {
    public int CharacterDBId { get; set; }
    public ItemInfoData ItemInfoData { get; set; }
    public int Money { get; set; }
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
}

public class C_GachaItem {
    public int CharacterDBId { get; set; }
}

public class S_GachaItem {
    public int CharacterDBId { get; set; }
    public ItemInfoData ItemInfoData { get; set; }
    public int Money { get; set; }
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
}
public class C_Emotion {
    public int CharacterDBId { get; set; }
    public int EmotionId { get; set; }
}

public class S_Emotion {
    public int CharacterDBId { get; set; }
    public int EmotionId { get; set; }
}

public class S_NPCMove {
    public int ObjectId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class C_PayOffDept {
    public int CharacterDBId { get; set; }
    public int MoneyForDept { get; set; }
}

public class S_PayOffDept {
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public int CharacterDBId { get; set; }
    public int Money { get; set; }
    public int Dept { get; set; }
}

public class C_EnhanceSkill {
    public int CharacterDBId { get; set; }
    public SkillId SkillId { get; set; }
}

public class S_EnhanceSkill {
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public int CharacterDBId { get; set; }
    public int Money { get; set; }
    public SkillId SkillId { get; set; }
    public int SkillLevel { get; set; }
}

public class C_ChooseSkill {
    public int CharacterDBId { get; set; }
    public SkillId SkillId { get; set; }
}

public class S_ChooseSkill {
    public bool Ok { get; set; }
    public int ErrorCode { get; set; }
    public int ChosenCharacterDBId { get; set; }
    public SkillId SkillId { get; set; }
}

public class S_PartyMemberReady {
    public int ReadyCharacterDBId { get; set; }
    public bool AllMemberReady { get; set; }
}

public class C_GetRanking {
    public int CharacterDBId { get; set; }
}

public class S_GetRanking {
    public List<PlayerRanking> PlayerRankings { get; set; } = new List<PlayerRanking>();
    public int MyCharacterDBId { get; set; }
    public int MyRanking { get; set; }
    public int MyMoney { get; set; }
    public int MinutesUntilUpdate { get; set; }
}

public class C_LoadingComplete {
    public int CharacterDBId { get; set; }
    public bool LoadingComplete { get; set; }
}

public class S_LoadingComplete {

}

public class MoveVector3 {
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
}

public class CharacterData {
    public int CharacterDBId { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
    public string Nickname { get; set; }
    public int SkillId { get; set; }
    public int SkillLevel { get; set; }
}

public class ObjectData {
    public int ObjectId { get; set; }
    public ObjectType ObjectType { get; set; }
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float PosZ { get; set; }
}

public class ItemInfoData {
    public int ItemDBId { get; set; }
    public ItemType ItemType { get; set; }
    public int TemplateId { get; set; }
    public int Count { get; set; }
    public int Slot { get; set; }
    public bool Equip { get; set; }
}

public class PlayerRanking {
    public string Nickname { get; set; }
    public int Ranking { get; set; }
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