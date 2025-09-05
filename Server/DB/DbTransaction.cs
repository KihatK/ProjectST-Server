using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Server.Data;
using Server.Game;
using Server.Job;
using Server.Object;
using Server.Redis;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Server.DB {
    public partial class DbTransaction : JobSerializer {
        public static int MaxSkillLevel = 5;
        public static DbTransaction Instance { get; } = new DbTransaction();
        enum LoginError {
            Success,
            NoCharacter,
        };

        #region CreateAccount
        public static void CreateAccount(string accountname, string password, ClientSession clientSession) {
            AccountDB accountDB = new AccountDB();
            accountDB.Accountname = accountname;
            accountDB.Password = password;

            Instance.Push(CreateAccountDB, accountDB, clientSession);
        }

        static void CreateAccountDB(AccountDB accountDB, ClientSession clientSession) {
            using (AppDbContext db = new AppDbContext()) {
                try {
                    AccountDB account = db.Accounts.Where(a => a.Accountname == accountDB.Accountname).FirstOrDefault();
                    S_CreateAccount createAccountPacket = new S_CreateAccount();

                    if (account != null) {
                        createAccountPacket.Ok = false;
                        createAccountPacket.ErrorMessage = "Accountname Already Exist";
                        createAccountPacket.ErrorCode = 1;
                        clientSession.SendPacket(createAccountPacket);
                        return;
                    }
                    db.Accounts.Add(accountDB);
                    bool success = db.SaveChangesEx();
                    if (success) {
                        //TODO: 성공했을 시 DB가 아닌 게임로직에서 처리할 일 푸시하기
                        createAccountPacket.Ok = true;
                        createAccountPacket.ErrorMessage = "Success";
                        createAccountPacket.ErrorCode = 0;
                        clientSession.SendPacket(createAccountPacket);
                    }
                    else {
                        //TODO: 실패했을 시 DB가 아닌 게임로직에서 처리할 일 푸시하기
                        createAccountPacket.Ok = false;
                        createAccountPacket.ErrorMessage = "Failed";
                        createAccountPacket.ErrorCode = 2;
                        clientSession.SendPacket(createAccountPacket);
                    }
                }
                catch (OracleException ex) {
                    S_CreateAccount createAccountPacket = new S_CreateAccount();
                    createAccountPacket.Ok = false;
                    createAccountPacket.ErrorCode = 100;
                    clientSession.SendPacket(createAccountPacket);
                    Console.WriteLine($"CreateAccountDB(OracleException) : {ex.ToString()}");

                }
                catch (Exception ex) {
                    S_CreateAccount createAccountPacket = new S_CreateAccount();
                    createAccountPacket.Ok = false;
                    createAccountPacket.ErrorCode = 200;
                    clientSession.SendPacket(createAccountPacket);
                    Console.WriteLine($"CreateAccountDB(Exception) : {ex.ToString()}");
                }
            }
        }

        #endregion

        #region CreateCharacter
        public static void CreateCharacter(string nickname, ClientSession clientSession) {
            CharacterDB characterDB = new CharacterDB();
            characterDB.Nickname = nickname;

            Instance.Push(CreateCharacterDB, characterDB, clientSession);
        }

        static void CreateCharacterDB(CharacterDB characterDB, ClientSession clientSession) {
            using (AppDbContext db = new AppDbContext()) {
                try {
                    CharacterDB character = db.Characters.Where(c => c.Nickname == characterDB.Nickname).FirstOrDefault();
                    S_CreateCharacter createCharacterPacket = new S_CreateCharacter();
                    if (character != null) {
                        createCharacterPacket.Ok = false;
                        createCharacterPacket.ErrorMessage = "Nickname Already Exist";
                        createCharacterPacket.ErrorCode = 1;
                        clientSession.SendPacket(createCharacterPacket);
                        return;
                    }
                    characterDB.Dept = 50000;
                    characterDB.AccountDBId = clientSession.AccountDBId;
                    db.Characters.Add(characterDB);
                    bool success = db.SaveChangesEx();
                    if (success) {
                        createCharacterPacket.Ok = true;
                        createCharacterPacket.ErrorMessage = "Success";
                        createCharacterPacket.ErrorCode = 0;
                        clientSession.SendPacket(createCharacterPacket);
                    }
                    else {
                        createCharacterPacket.Ok = false;
                        createCharacterPacket.ErrorMessage = "Failed";
                        createCharacterPacket.ErrorCode = 2;
                        clientSession.SendPacket(createCharacterPacket);
                    }
                }
                catch (OracleException ex) {
                    S_CreateCharacter createCharacterPacket = new S_CreateCharacter();
                    createCharacterPacket.Ok = false;
                    createCharacterPacket.ErrorCode = 100;
                    clientSession.SendPacket(createCharacterPacket);
                    Console.WriteLine($"CreateCharacterDB(OracleException) : {ex.ToString()}");
                }
                catch (Exception ex) {
                    S_CreateCharacter createCharacterPacket = new S_CreateCharacter();
                    createCharacterPacket.Ok = false;
                    createCharacterPacket.ErrorCode = 200;
                    clientSession.SendPacket(createCharacterPacket);
                    Console.WriteLine($"CreateCharacterDB(Exception) : {ex.ToString()}");
                }
            }
        }

        #endregion

        #region Login
        public static void OAuthLogin(string token, LoginType loginType, LoginRoom loginRoom, ClientSession clientSession) {
            Instance.Push(OAuthLoginDB, token, loginType, loginRoom, clientSession);
        }

        static void OAuthLoginDB(string token, LoginType loginType, LoginRoom loginRoom, ClientSession clientSession) {
            using (AppDbContext db = new AppDbContext()) {
                try {
                    AccountDB accountDB = db.Accounts.Where(a => a.Accountname == token).SingleOrDefault();
                    if (accountDB != null &&  accountDB.LoginType != (int)loginType) {
                        accountDB = null;
                    }
                    CharacterDB characterDB = null;
                    if (accountDB == null) {
                        accountDB = new AccountDB() {
                            Accountname = token,
                            Password = "1234",
                            LoginType = (int)loginType,
                        };
                        db.Accounts.Add(accountDB);
                        bool success = db.SaveChangesEx();

                        if (!success) {
                            S_Login loginPacket = new S_Login();
                            loginPacket.Ok = false;
                            loginPacket.ErrorCode = 5;
                            clientSession.SendPacket(loginPacket);
                        }
                    }

                    //TODO: Token에 대한 테이블이 변경되었는데 AsNoTracking을 추가해도 되는가
                    characterDB = db.Characters.AsNoTracking().Where(c => c.AccountDBId == accountDB.AccountDBId).FirstOrDefault();
                    
                    if (characterDB == null) {
                        GameLogic.Instance.GetRoom(RoomType.Login).Push(OAuthLoginResponse, LoginError.NoCharacter, loginRoom, clientSession, characterDB, accountDB.AccountDBId);
                    }
                    else
                    {
                        GameLogic.Instance.GetRoom(RoomType.Login).Push(OAuthLoginResponse, LoginError.Success, loginRoom, clientSession, characterDB, accountDB.AccountDBId);
                    }
                }
                catch (OracleException ex) {
                    S_Login loginPacket = new S_Login();
                    loginPacket.Ok = false;
                    loginPacket.ErrorCode = 100;
                    clientSession.SendPacket(loginPacket);
                    Console.WriteLine($"OAuthLoginDB(OracleException) : {ex.ToString()}");

                }
                catch (Exception ex) {
                    S_Login loginPacket = new S_Login();
                    loginPacket.Ok = false;
                    loginPacket.ErrorCode = 200;
                    clientSession.SendPacket(loginPacket);
                    Console.WriteLine($"OAuthLoginDB(Exception) : {ex.ToString()}");
                }
            }
        }

        static void OAuthLoginResponse(LoginError loginError, LoginRoom loginRoom, ClientSession clientSession, CharacterDB characterDB, int accountDBId) {
            S_Login loginPacket = new S_Login();
            if (loginError == LoginError.Success) {
                if (loginRoom.IsLoggedIn(characterDB.CharacterDBId)) {
                    loginPacket.Ok = false;
                    loginPacket.ErrorMessage = "이미 로그인 중인 계정입니다";
                    loginPacket.ErrorCode = 3;
                    loginPacket.AccountDBId = accountDBId;
                }
                else {
                    clientSession.CharacterDBId = characterDB.CharacterDBId;
                    clientSession.MyCharacter = new Character() {
                        CharacterDBId = characterDB.CharacterDBId,
                        Nickname = characterDB.Nickname,
                        Money = characterDB.Money,
                        Dept = characterDB.Dept,
                        Location = characterDB.Location,
                        FarmCoin = characterDB.FamrCoin,
                        AccountDBId = accountDBId,
                        FarmDBId = characterDB.FarmDBId,
                        Session = clientSession,
                    };
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Winter] = characterDB.WinterSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Spring] = characterDB.SpringSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Summer] = characterDB.SummerSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Autumn] = characterDB.AutumnSkillLevel;
                    loginRoom.LoginAccount(characterDB.CharacterDBId);
                    loginPacket.Ok = true;
                    loginPacket.ErrorMessage = "Success";
                    loginPacket.ErrorCode = 0;
                    loginPacket.AccountDBId = accountDBId;
                    loginPacket.CharacterDBId = characterDB.CharacterDBId;
                    loginPacket.Nickname = characterDB.Nickname;
                    loginPacket.Money = characterDB.Money;
                    loginPacket.Dept = characterDB.Dept;
                    RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, characterDB.Nickname, characterDB.Money);
                }
            }
            else if (loginError == LoginError.NoCharacter) {
                loginPacket.Ok = false;
                loginPacket.ErrorMessage = "캐릭터가 존재하지 않습니다";
                loginPacket.ErrorCode = 2;
                loginPacket.AccountDBId = accountDBId;
            }
            clientSession.AccountDBId = accountDBId;
            clientSession.SendPacket(loginPacket);
        }

        public static void LoginAccount(string accountname, string password, ClientSession clientSession, LoginRoom loginRoom) {
            Instance.Push(LoginAccountDB, accountname, password, clientSession, loginRoom);
        }

        static void LoginAccountDB(string  accountname, string password, ClientSession clientSession, LoginRoom loginRoom) {
            using (AppDbContext db = new AppDbContext()) {
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        AccountDB account = db.Accounts.AsNoTracking().Where(a => a.Accountname == accountname).FirstOrDefault();
                        if (account != null && account.Password != password) {
                            account = null;
                        }
                        S_Login loginPacket = new S_Login();


                        transaction.Commit();

                        if (account == null) {
                            //아이디가 존재하지 않거나 비밀번호가 다릅니다
                            loginPacket.Ok = false;
                            loginPacket.ErrorMessage = "아이디가 존재하지 않거나 비밀번호가 다릅니다";
                            loginPacket.ErrorCode = 1;
                            loginPacket.AccountDBId = -1;
                            clientSession.SendPacket(loginPacket);
                            return;
                        }

                        CharacterDB characterDB = db.Characters.AsNoTracking().Where(c => c.AccountDBId == account.AccountDBId).FirstOrDefault();

                        if (characterDB == null) {
                            //캐릭터가 존재하지 않습니다. 캐릭터 닉네임을 설정
                            loginPacket.Ok = false;
                            loginPacket.ErrorMessage = "캐릭터가 존재하지 않습니다";
                            loginPacket.ErrorCode = 2;
                            loginPacket.AccountDBId = account.AccountDBId;
                            CharacterDB character = null;
                            loginRoom.Push(LoginAccountResponse, loginPacket, clientSession, character, loginRoom);
                            return;
                        }
                        loginPacket.Ok = true;
                        loginPacket.ErrorMessage = "Success";
                        loginPacket.ErrorCode = 0;
                        loginPacket.AccountDBId = account.AccountDBId;
                        loginPacket.CharacterDBId = characterDB.CharacterDBId;
                        loginPacket.Nickname = characterDB.Nickname;
                        loginPacket.Money = characterDB.Money;
                        loginPacket.Dept = characterDB.Dept;
                        loginRoom.Push(LoginAccountResponse, loginPacket, clientSession, characterDB, loginRoom);
                    }
                    catch (OracleException ex) {
                        S_Login loginPacket = new S_Login();
                        loginPacket.Ok = false;
                        loginPacket.ErrorMessage = "서버 연결에 실패했습니다. 다시 시도해 주세요.";
                        loginPacket.ErrorCode = 100;
                        Console.WriteLine($"LoginAccountDB(OracleException) : {ex.ToString()}");
                        clientSession.SendPacket(loginPacket);
                    }
                    catch (Exception ex) {
                        S_Login loginPacket = new S_Login();
                        loginPacket.Ok = false;
                        loginPacket.ErrorMessage = "알 수 없는 오류가 발생했습니다.";
                        loginPacket.ErrorCode = 200; // 기타 오류 코드
                        Console.WriteLine($"LoginAccountDB(Exception) : {ex.ToString()}");
                        clientSession.SendPacket(loginPacket);
                    }
                }
            }
        }

        static void LoginAccountResponse(S_Login loginPacket, ClientSession clientSession, CharacterDB character, GameRoom room) {
            if (character != null) {
                LoginRoom loginRoom = room as LoginRoom;
                if (loginRoom.IsLoggedIn(character.CharacterDBId)) {
                    loginPacket = new S_Login() {
                        Ok = false,
                        ErrorMessage = "이미 로그인 중인 계정입니다",
                        ErrorCode = 3,
                    };
                }
                else {
                    clientSession.CharacterDBId = character.CharacterDBId;
                    clientSession.MyCharacter = new Character() {
                        CharacterDBId = character.CharacterDBId,
                        Nickname = character.Nickname,
                        Money = character.Money,
                        Dept = character.Dept,
                        Location = character.Location,
                        FarmCoin = character.FamrCoin,
                        AccountDBId = character.AccountDBId,
                        FarmDBId = character.FarmDBId,
                        Session = clientSession,
                    };
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Winter] = character.WinterSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Spring] = character.SpringSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Summer] = character.SummerSkillLevel;
                    clientSession.MyCharacter.skillLevel[(int)SkillId.Autumn] = character.AutumnSkillLevel;
                    loginRoom.LoginAccount(character.CharacterDBId);
                    RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, character.Nickname, character.Money);
                }
            }
            clientSession.AccountDBId = loginPacket.AccountDBId;
            clientSession.SendPacket(loginPacket);
        }
        #endregion

        public static void Dummy() {
            using (var db = new AppDbContext()) {
                db.Database.EnsureCreated();
                if (RedisServer.Instance.DataExist()) {
                    db.Accounts.FirstOrDefault();
                }
                else {
                    foreach (var character in db.Characters.Select(c => new { c.Nickname, c.Money }).ToList()) {
                        RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, character.Nickname, character.Money);
                    }
                }
            }
        }

        public static void DbPing() {
            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    var connection = db.Database.GetDbConnection();
                    connection.Open(); // 명시적으로 연결 열기

                    using (var transaction = db.Database.BeginTransaction()) {
                        try {
                            var ping = db.Accounts.FirstOrDefault(); // 단순 조회
                            db.Database.ExecuteSqlRaw("SELECT 1 FROM DUAL"); // 추가적인 쿼리 실행
                            db.SaveChanges(); // 변경 사항 저장 후
                            transaction.Commit(); // 트랜잭션 커밋
                        }
                        catch (OracleException ex) {
                            transaction.Rollback();
                            Console.WriteLine($"실패 이유 : {ex.ToString()}");

                        }
                        catch (Exception ex) {
                            transaction.Rollback();
                            Console.WriteLine($"실패 이유 : {ex.ToString()}");
                        }
                    }
                }
            });
            Instance.PushAfter(1000 * 60 * 25, DbPing);
        }

        public static void PayOffDept(Character character, int money) {
            S_PayOffDept deptPacket = new S_PayOffDept() {
                CharacterDBId = character.CharacterDBId,
            };

            if (character == null) {
                deptPacket.Ok = false;
                deptPacket.ErrorCode = 4;
                character.Session.SendPacket(deptPacket);
                return;
            }
            if (money <= 0) {
                //0원 이하의 돈으로 갚으려할 때
                deptPacket.Ok = false;
                deptPacket.ErrorCode = 3;
                character.Session.SendPacket(deptPacket);
                return;
            }
            if (character.Dept <= 0) {
                //이미 빚을 다 갚았을 때
                deptPacket.Ok = false;
                deptPacket.ErrorCode = 1;
                character.Session.SendPacket(deptPacket);
                return;
            }

            if (character.Dept < money) {
                //빚보다 많은 돈으로 갚으려할 때 소모되는 돈을 빚만큼으로 다시 설정
                money = character.Dept;
            }
            if (character.Money < money) {
                //돈이 부족함
                deptPacket.Ok = false;
                deptPacket.ErrorCode = 2;
                character.Session.SendPacket(deptPacket);
                return;
            }

            //메모리에 선적용
            character.Money -= money;
            character.Dept -= money;

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                        if (characterDB == null) {
                            //Error
                            deptPacket.Ok = false;
                            deptPacket.ErrorCode = 4;
                            character.Session.SendPacket(deptPacket);
                            return;
                        }

                        if (characterDB.Money < money) {
                            //돈이 부족함
                            deptPacket.Ok = false;
                            deptPacket.ErrorCode = 2;
                            character.Session.SendPacket(deptPacket);
                            return;
                        }

                        if (characterDB.Dept <= 0) {
                            //이미 빚을 다 갚았을 때
                            deptPacket.Ok = false;
                            deptPacket.ErrorCode = 1;
                            character.Session.SendPacket(deptPacket);
                            return;
                        }

                        characterDB.Money -= money;
                        characterDB.Dept -= money;

                        db.SaveChanges();

                        transaction.Commit();

                        deptPacket.Ok = true;
                        deptPacket.ErrorCode = 0;
                        deptPacket.Money = characterDB.Money;
                        deptPacket.Dept = characterDB.Dept;
                        character.Session.SendPacket(deptPacket);
                        RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, characterDB.Nickname, characterDB.Money);
                    }
                    catch (OracleException ex) {
                        transaction.Rollback();
                        character.Money += money;
                        character.Dept += money;
                        deptPacket.Ok = false;
                        deptPacket.ErrorCode = 5;
                        character.Session.SendPacket(deptPacket);
                        Console.WriteLine($"PayOffDept 실패: {character.CharacterDBId} : {money} 빚갚기 안됨");
                        Console.WriteLine($"실패 이유 : {ex.ToString()}");

                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        character.Money += money;
                        character.Dept += money;
                        deptPacket.Ok = false;
                        deptPacket.ErrorCode = 6;
                        character.Session.SendPacket(deptPacket);
                        Console.WriteLine($"PayOffDept 실패: {character.CharacterDBId} : {money} 빚갚기 안됨");
                        Console.WriteLine($"실패 이유 : {ex.ToString()}");
                    }
                }
            });
        }

        public static void EnhanceSkill(Character character, SkillId skillId) {
            S_EnhanceSkill skillPacket = new S_EnhanceSkill() {
                CharacterDBId = character.CharacterDBId,
            };
            if (character == null) {
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 4;
                character.Session.SendPacket(skillPacket);
                return;
            }
            if (character.Dept > 0) {
                //빚을 다 갚은 후에 강화 가능
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 1;
                character.Session.SendPacket(skillPacket);
                return;
            }

            int curSkillLevel;
            if (character.skillLevel.TryGetValue((int)skillId, out curSkillLevel) == false) {
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 6;
                character.Session.SendPacket(skillPacket);
                return;
            }

            if (curSkillLevel >= MaxSkillLevel) {
                //최대 레벨
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 2;
                character.Session.SendPacket(skillPacket);
                return;
            }

            SkillData skillData;
            if (!DataManager.SkillDict.TryGetValue((int)skillId, out skillData) ||
                !skillData.level.TryGetValue(curSkillLevel + 1, out var skillLevelData) ||
                !skillLevelData.TryGetValue("price", out float moneyFloat)) {
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 6;
                character.Session.SendPacket(skillPacket);
                return;
            }

            int money = (int)moneyFloat;

            if (character.Money < money) {
                skillPacket.Ok = false;
                skillPacket.ErrorCode = 3;
                character.Session.SendPacket(skillPacket);
                return;
            }

            character.Money -= money;
            character.skillLevel[(int)skillId] += 1;

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                        if (characterDB == null) {
                            skillPacket.Ok = false;
                            skillPacket.ErrorCode = 4;
                            character.Session.SendPacket(skillPacket);
                            return;
                        }

                        if (characterDB.Money < money) {
                            skillPacket.Ok = false;
                            skillPacket.ErrorCode = 3;
                            character.Session.SendPacket(skillPacket);
                            return;
                        }

                        characterDB.Money -= money;
                        if (skillId == SkillId.Winter) {
                            characterDB.WinterSkillLevel = curSkillLevel + 1;
                        }
                        else if (skillId == SkillId.Spring) {
                            characterDB.SpringSkillLevel = curSkillLevel + 1;
                        }
                        else if (skillId == SkillId.Summer) {
                            characterDB.SummerSkillLevel = curSkillLevel + 1;
                        }
                        else if (skillId == SkillId.Autumn) {
                            characterDB.AutumnSkillLevel = curSkillLevel + 1;
                        }

                        db.SaveChanges();

                        transaction.Commit();

                        skillPacket.Ok = true;
                        skillPacket.ErrorCode = 0;
                        skillPacket.Money = characterDB.Money;
                        skillPacket.SkillId = skillId;
                        skillPacket.SkillLevel = curSkillLevel + 1;
                        character.Session.SendPacket(skillPacket);
                        RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, characterDB.Nickname, characterDB.Money);
                    }
                    catch (OracleException ex) {
                        transaction.Rollback();
                        character.Money += money;
                        character.skillLevel[(int)skillId] -= 1;
                        skillPacket.Ok = false;
                        skillPacket.ErrorCode = 5;
                        character.Session.SendPacket(skillPacket);
                        Console.WriteLine($"EnhanceSkill 실패: {character.CharacterDBId} : {money} 스킬강화 안됨");
                        Console.WriteLine($"실패 이유 : {ex.ToString()}");

                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        character.Money += money;
                        character.skillLevel[(int)skillId] -= 1;
                        skillPacket.Ok = false;
                        skillPacket.ErrorCode = 6;
                        character.Session.SendPacket(skillPacket);
                        Console.WriteLine($"EnhanceSkill 실패: {character.CharacterDBId} : {money} 스킬강화 안됨");
                        Console.WriteLine($"실패 이유 : {ex.ToString()}");
                    }
                }
            });
        }

        public static void GetReward(Character character, int money) {
            if (character == null) {
                return;
            }

            if (money < 0) {
                return;
            }

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext())
                using (var transaction = db.Database.BeginTransaction()) {
                    try {
                        CharacterDB characterDB = db.Characters.Find(character.CharacterDBId);
                        if (characterDB == null) {
                            return;
                        }
                        characterDB.Money += money;
                        db.Entry(characterDB).Property(nameof(CharacterDB.Money)).IsModified = true;
                        db.SaveChanges();

                        transaction.Commit();
                        RedisServer.Instance.Push(RedisServer.Instance.ChangeCharacterMoney, characterDB.Nickname, characterDB.Money);
                    }
                    catch (OracleException ex) {
                        transaction.Rollback();
                        character.Money -= money;
                        Console.WriteLine($"GetReward 실패: {character.CharacterDBId} : {money} 추가 안됨");
                        Console.WriteLine($"실패 이유(Oracle) : {ex.ToString()}");
                        Console.WriteLine($"실패 이유(Oracle, Inner) : {ex.InnerException.Message}");

                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        character.Money -= money;
                        Console.WriteLine($"GetReward 실패: {character.CharacterDBId} : {money} 추가 안됨");
                        Console.WriteLine($"실패 이유(Exception) : {ex.ToString()}");
                        Console.WriteLine($"실패 이유(Exception, Inner) : {ex.InnerException.Message}");
                    }
                }
            });
        }
    }
}