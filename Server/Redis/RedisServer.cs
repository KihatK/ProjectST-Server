using Server.Data;
using Server.Job;
using Server.Object;
using StackExchange.Redis;

namespace Server.Redis {
    class RedisServer : JobSerializer {
        #region Singleton
        private static RedisServer instance = null;
        private RedisServer() { }

        public static RedisServer Instance {
            get {
                if (instance == null) {
                    instance = new RedisServer();
                }
                return instance;
            }
        }
        #endregion
        public int minutesToUpdate { get; private set; } = 1;
        IDatabase db;
        string _sortedSetKey = "character_money";
        List<PlayerRanking> _ranking = new List<PlayerRanking>();

        public void RunRedisServer() {
            var options = new ConfigurationOptions() {
                EndPoints = { ConfigManager.Config.connectRedisIP },
                Password = ConfigManager.Config.redisPassword,
                ConnectTimeout = 10 * 1000,
                AsyncTimeout = 10 * 1000,
            };
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            db = redis.GetDatabase();
        }

        public async void ChangeCharacterMoney(string nickname, int money) {
            try {
                await db.SortedSetAddAsync(_sortedSetKey, nickname, money);
            }
            catch (RedisTimeoutException ex) {
                Console.WriteLine($"ChangeCharacterMoney: Redis timeout occurred: {ex.Message}");
            }
        }

        public async void RemoveCharacter(string nickname) {
            try {
                await db.SortedSetRemoveAsync(_sortedSetKey, nickname);
            }
            catch (RedisTimeoutException ex) {
                Console.WriteLine($"RemoveCharacter: Redis timeout occurred: {ex.Message}");
            }
        }

        public bool DataExist() {
            try {
                return db.KeyExists(_sortedSetKey);
            }
            catch (RedisTimeoutException ex) {
                Console.WriteLine($"DataExist: Redis timeout occurred: {ex.Message}");
                return false;
            }
        }

        //public async void TryUpdateRanking() {
        //    await UpdateRankingEvery5Min();
        //    PushAfter(1000 * 60, TryUpdateRanking);
        //}

        //public async Task UpdateRankingEvery5Min() {
        //    minutesToUpdate -= 1;
        //    if (minutesToUpdate == 0) {
        //        await UpdateRanking();
        //        minutesToUpdate = 10;
        //    }
        //}

        //async Task UpdateRanking() {
        //    try {
        //        var topCharacters = await db.SortedSetRangeByScoreWithScoresAsync(_sortedSetKey, order: Order.Descending, take: 10);
        //        List<PlayerRanking> ranking = new List<PlayerRanking>();

        //        for (int i = 0; i < topCharacters.Length; i++) {
        //            PlayerRanking playerRanking = new PlayerRanking() {
        //                Nickname = topCharacters[i].Element,
        //                Ranking = i + 1,
        //                Money = (int)topCharacters[i].Score,
        //            };
        //            ranking.Add(playerRanking);
        //        }
        //        _ranking = ranking;
        //    }
        //    catch (RedisTimeoutException ex) {
        //        Console.WriteLine($"UpdateRanking: Redis timeout occurred: {ex.Message}");
        //    }
        //}

        public async void GetRanking(Character character) {
            try {
                S_GetRanking rankingPacket = new S_GetRanking();

                var topCharacters = await db.SortedSetRangeByScoreWithScoresAsync(_sortedSetKey, order: Order.Descending, take: 10);
                for (int i = 0; i < topCharacters.Length; i++) {
                    PlayerRanking playerRanking = new PlayerRanking() {
                        Nickname = topCharacters[i].Element,
                        Ranking = i + 1,
                        Money = (int)topCharacters[i].Score,
                    };
                    rankingPacket.PlayerRankings.Add(playerRanking);
                }

                long? rank = -1;
                for (int i = 0; i < _ranking.Count; i++) {
                    if (_ranking[i].Nickname == character.Nickname) {
                        rank = _ranking[i].Ranking;
                    }
                }
                if (rank == -1) {
                    rank = await db.SortedSetRankAsync(_sortedSetKey, character.Nickname, Order.Descending) + 1;
                }

                rankingPacket.MinutesUntilUpdate = minutesToUpdate;
                rankingPacket.MyCharacterDBId = character.CharacterDBId;
                rankingPacket.MyRanking = rank.HasValue ? (int)rank.Value : -1;
                rankingPacket.MyMoney = character.Money;
                character.Session.SendPacket(rankingPacket);
            }
            catch (RedisTimeoutException ex) {
                Console.WriteLine($"GetRanking: Redis timeout occurred: {ex.Message}");
            }
        }

        public List<PlayerRanking> GetRanking() {
            return _ranking;
        }

        public async void GetMyRanking(Character character) {
            try {
                long? rank = -1;
                for (int i = 0; i < _ranking.Count; i++) {
                    if (_ranking[i].Nickname == character.Nickname) {
                        rank =  _ranking[i].Ranking;
                    }
                }
                if (rank == -1) {
                    rank = await db.SortedSetRankAsync(_sortedSetKey, character.Nickname, Order.Descending) + 1;
                }

                S_GetRanking rankingPacket = new S_GetRanking();
                rankingPacket.PlayerRankings = GetRanking();
                rankingPacket.MinutesUntilUpdate = minutesToUpdate;
                rankingPacket.MyCharacterDBId = character.CharacterDBId;
                rankingPacket.MyRanking = rank.HasValue ? (int)rank.Value : -1;
                rankingPacket.MyMoney = character.Money;
                character.Session.SendPacket(rankingPacket);
            }
            catch (RedisTimeoutException ex) {
                Console.WriteLine($"GetMyRanking: Redis timeout occurred: {ex.Message}");
            }
        }
    }
}
