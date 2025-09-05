namespace Server.Data {
    [Serializable]
    public class ServerConfig {
        public string dataPath;
        public string connectionString;
        public string connectOracleString;
        public string walletLocation;
        public int MsPerFrame;
        public bool Deploy;
        public string oracleDataPath;
        public string oracleWalletLocation;
        public string connectRedisIP;
        public string redisPassword;
    }

    class ConfigManager {
        public static ServerConfig Config { get; private set; }

        public static void LoadConfig() {
            string text = File.ReadAllText("/app/config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}
