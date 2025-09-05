namespace Server.Data {
    public interface ILoader<Key, Value> {
        Dictionary<Key, Value> MakeDict();
    }
    public class DataManager {
        public static Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
        public static Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();

        public static void LoadData() {
            SkillDict = LoadJson<SkillLoader, int, SkillData>("SkillData").MakeDict();
            ItemDict = new ItemLoader("ItemData").MakeDict();
        }

        static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value> {
            string text;
            if (ConfigManager.Config.Deploy) {
                text = File.ReadAllText($"{ConfigManager.Config.oracleDataPath}/{path}.json");
            }
            else {
                text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{path}.json");
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }

    }
}
